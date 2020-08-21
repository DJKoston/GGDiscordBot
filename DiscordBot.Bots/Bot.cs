using DiscordBot.Bots.Commands;
using DiscordBot.Core.Services.CustomCommands;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.Core.ViewModels;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot.Bots
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public Bot(IServiceProvider services)
        {
            _profileService = services.GetService<IProfileService>();
            _experienceService = services.GetService<IExperienceService>();
            _customCommandService = services.GetService<ICustomCommandService>();

            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;
            Client.MessageCreated += OnMessageCreated;
            Client.ClientErrored += OnClientErrored;
            Client.GuildMemberAdded += OnNewMember;
            Client.GuildMemberRemoved += OnMemberLeave;
            Client.MessageReactionAdded += OnReactionAdded;
            Client.MessageReactionRemoved += OnReactionRemoved;
            Client.ChannelCreated += OnChannelCreated;
            Client.ChannelDeleted += OnChannelDeleted;
            Client.GuildAvailable += OnGuildAvaliable;
            Client.PresenceUpdated += OnPresenceUpdated;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                Services = services,

            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<AdminCommands>();
            Commands.RegisterCommands<GameCommands>();
            Commands.RegisterCommands<LeaveRoleCommands>();
            Commands.RegisterCommands<ManageCommands>();
            Commands.RegisterCommands<MiscCommands>();
            Commands.RegisterCommands<ModCommands>();
            Commands.RegisterCommands<ProfileCommands>();
            Commands.RegisterCommands<QuoteCommands>();
            Commands.RegisterCommands<RoleCommands>();

            Commands.CommandErrored += OnCommandErrored;

            Console.WriteLine("Connecting");
            Client.ConnectAsync();
            Console.WriteLine("Connected!");
        }

        private Task OnPresenceUpdated(PresenceUpdateEventArgs e)
        {
            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);
            DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);

            DiscordRole generationGamers = guild.GetRole(411304802883207169);
            DiscordRole ggNowLive = guild.GetRole(745018263456448573);
            DiscordRole NowLive = guild.GetRole(745018328179015700);

            if (e.User.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
            {
                if (member.Roles.Contains(generationGamers))
                {
                    member.GrantRoleAsync(ggNowLive);
                }

                else
                {
                    member.GrantRoleAsync(NowLive);
                }
                
            }

            else
            {
                if (member.Roles.Contains(ggNowLive))
                {
                    member.RevokeRoleAsync(ggNowLive);
                }

                if (member.Roles.Contains(NowLive))
                {
                    member.RevokeRoleAsync(NowLive);
                }
            }

            return Task.CompletedTask;
        }

        private Task OnGuildAvaliable(GuildCreateEventArgs e)
        {
            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);

            var allMembers = guild.GetAllMembersAsync().Result;

            DiscordRole generationGamers = guild.GetRole(411304802883207169);
            DiscordRole ggNowLive = guild.GetRole(745018263456448573);
            DiscordRole NowLive = guild.GetRole(745018328179015700);

            var ggmemberslist = allMembers.Where(x => x.Roles.Contains(generationGamers));
            var nullgg = ggmemberslist.Where(x => x.Presence == null);
            var nullwithNowLive = nullgg.Where(x => x.Roles.Contains(ggNowLive));
            var ggmembers = ggmemberslist.Except(nullgg);
            var gglivemembers = ggmembers.Where(x => x.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)));
            var ggnotlivemembers = ggmembers.Except(gglivemembers);
            var ggwaslive = ggnotlivemembers.Where(x => x.Roles.Contains(ggNowLive));

            var othermemberslist = allMembers.Except(ggmemberslist);
            var nullother = othermemberslist.Where(x => x.Presence == null);
            var otherwithNowLive = nullother.Where(x => x.Roles.Contains(NowLive));
            var othermembers = othermemberslist.Except(nullother);
            var otherlivemembers = othermembers.Where(x => x.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)));
            var othernotlivemembers = othermembers.Except(otherlivemembers);
            var otherwaslive = othernotlivemembers.Where(x => x.Roles.Contains(NowLive));

            foreach (DiscordMember gglivemember in gglivemembers)
            {
                if (gglivemember.Roles.Contains(ggNowLive))
                {

                }

                else
                {
                    gglivemember.GrantRoleAsync(ggNowLive);
                }
            }

            foreach (DiscordMember nullmember in nullwithNowLive)
            {
                if (nullmember.Roles.Contains(ggNowLive))
                {
                    nullmember.RevokeRoleAsync(ggNowLive);
                }

                else
                {
                    
                }
                
            }

            foreach (DiscordMember ggwaslivemember in ggwaslive)
            {
                if (ggwaslivemember.Roles.Contains(ggNowLive))
                {
                    ggwaslivemember.RevokeRoleAsync(ggNowLive);
                }

                else
                {

                }
                
            }

            foreach (DiscordMember otherlivemember in otherlivemembers)
            {
                if (otherlivemember.Roles.Contains(NowLive))
                {

                }

                else
                {
                    otherlivemember.GrantRoleAsync(NowLive);
                }
                
            }

            foreach (DiscordMember nullmember in otherwithNowLive)
            {
                if (nullmember.Roles.Contains(NowLive))
                {
                    nullmember.RevokeRoleAsync(NowLive);
                }

                else
                {
                    
                }
                
            }

            foreach (DiscordMember otherwaslivemember in otherwaslive)
            {
                if (otherwaslivemember.Roles.Contains(NowLive))
                {
                    otherwaslivemember.RevokeRoleAsync(NowLive);
                }

                else
                {

                }
                
            }

            return Task.CompletedTask;

        }

        private Task OnChannelDeleted(ChannelDeleteEventArgs e)
        {
            var guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);
            var channels = guild.GetChannelsAsync();
            var channelCount = channels.Result.Count();

            Client.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = ActivityType.Watching,
                Name = $"{channelCount} Channels in GG!",
            }, UserStatus.Online);

            return Task.CompletedTask;
        }

        private Task OnChannelCreated(ChannelCreateEventArgs e)
        {
            var guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);
            var channels = guild.GetChannelsAsync();
            var channelCount = channels.Result.Count();

            Client.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = ActivityType.Watching,
                Name = $"{channelCount} Channels in GG!",
            }, UserStatus.Online);

            return Task.CompletedTask;
        }

        private async Task OnReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot)
            {
                return;
            }

            var guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Name == "Generation Gamers");
            var member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
            var channel = guild.Channels.Values.FirstOrDefault(x => x.Name == "role-management");

            DiscordMessage streamerMessage = await channel.GetMessageAsync(538530070441099278);
            DiscordMessage platformMessage = await channel.GetMessageAsync(538530958488371204);
            DiscordMessage announcementMessage = await channel.GetMessageAsync(612698615894245378);

            DiscordRole twitchStreamerRole = guild.GetRole(414301182136418316);
            DiscordRole streamerRole = guild.GetRole(541321351084900382);
            DiscordRole pcRole = guild.GetRole(513177707954307086);
            DiscordRole xboxRole = guild.GetRole(513177622516334604);
            DiscordRole playstationRole = guild.GetRole(513177669014388789);
            DiscordRole announcementRole = guild.GetRole(612691988281688105);

            DiscordEmoji twitchEmoji = DiscordEmoji.FromGuildEmote(e.Client, 725823600346529795);
            DiscordEmoji playstationEmoji = DiscordEmoji.FromGuildEmote(e.Client, 538531970343501824);
            DiscordEmoji xboxEmoji = DiscordEmoji.FromGuildEmote(e.Client, 538531692378324997);
            DiscordEmoji pcEmoji = DiscordEmoji.FromGuildEmote(e.Client, 538531394251653121);
            DiscordEmoji announcementEmoji = DiscordEmoji.FromName(e.Client, ":one:");


            if (e.Message.Id == streamerMessage.Id)
            {
                if (e.Emoji == twitchEmoji)
                {
                    await member.RevokeRoleAsync(twitchStreamerRole);
                    await member.RevokeRoleAsync(streamerRole);

                    return;
                }
                return;
            }

            if(e.Message.Id == platformMessage.Id)
            {
                if(e.Emoji == playstationEmoji)
                {
                    await member.RevokeRoleAsync(playstationRole);

                    return;
                }

                if(e.Emoji == xboxEmoji)
                {
                    await member.RevokeRoleAsync(xboxRole);

                    return;
                }

                if(e.Emoji == pcEmoji)
                {
                    await member.RevokeRoleAsync(pcRole);

                    return;
                }
            }

            if(e.Message.Id == announcementMessage.Id)
            {
                if (e.Emoji == announcementEmoji)
                {
                    await member.GrantRoleAsync(announcementRole);

                    return;
                }
            }

            return;
        }

        private async Task OnReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot)
            {
                return;
            }

            var guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Name == "Generation Gamers");
            var member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
            var channel = guild.Channels.Values.FirstOrDefault(x => x.Name == "role-management");

            DiscordMessage streamerMessage = await channel.GetMessageAsync(538530070441099278);
            DiscordMessage platformMessage = await channel.GetMessageAsync(538530958488371204);
            DiscordMessage announcementMessage = await channel.GetMessageAsync(612698615894245378);

            DiscordRole twitchStreamerRole = guild.GetRole(414301182136418316);
            DiscordRole streamerRole = guild.GetRole(541321351084900382);
            DiscordRole pcRole = guild.GetRole(513177707954307086);
            DiscordRole xboxRole = guild.GetRole(513177622516334604);
            DiscordRole playstationRole = guild.GetRole(513177669014388789);
            DiscordRole announcementRole = guild.GetRole(612691988281688105);

            DiscordEmoji twitchEmoji = DiscordEmoji.FromGuildEmote(e.Client, 725823600346529795);
            DiscordEmoji playstationEmoji = DiscordEmoji.FromGuildEmote(e.Client, 538531970343501824);
            DiscordEmoji xboxEmoji = DiscordEmoji.FromGuildEmote(e.Client, 538531692378324997);
            DiscordEmoji pcEmoji = DiscordEmoji.FromGuildEmote(e.Client, 538531394251653121);
            DiscordEmoji announcementEmoji = DiscordEmoji.FromName(e.Client, ":one:");


            if (e.Message.Id == streamerMessage.Id)
            {
                if (e.Emoji == twitchEmoji)
                {
                    await member.GrantRoleAsync(twitchStreamerRole);
                    await member.GrantRoleAsync(streamerRole);

                    return;
                }
                return;
            }

            if (e.Message.Id == platformMessage.Id)
            {
                if (e.Emoji == playstationEmoji)
                {
                    await member.GrantRoleAsync(playstationRole);

                    return;
                }

                if (e.Emoji == xboxEmoji)
                {
                    await member.GrantRoleAsync(xboxRole);

                    return;
                }

                if (e.Emoji == pcEmoji)
                {
                    await member.GrantRoleAsync(pcRole);

                    return;
                }
            }

            if (e.Message.Id == announcementMessage.Id)
            {
                if (e.Emoji == announcementEmoji)
                {
                    await member.RevokeRoleAsync(announcementRole);

                    return;
                }
            }

            return;
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);
            var channels = guild.GetChannelsAsync();
            var channelCount = channels.Result.Count();

            Client.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = ActivityType.Watching,
                Name = $"{channelCount} Channels in GG!",
            }, UserStatus.Online);

            return Task.CompletedTask;
        }

        private readonly IProfileService _profileService;
        private readonly IExperienceService _experienceService;

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
            {
                return;
            }

            var member = e.Guild.Members[e.Author.Id];

            var randomNumber = new Random();

            int randXP = randomNumber.Next(50);

            GrantXpViewModel viewModel = await _experienceService.GrantXpAsync(e.Author.Id, e.Guild.Id, randXP, e.Author.Username);

            if (!viewModel.LevelledUp) { return; }

            Profile profile = await _profileService.GetOrCreateProfileAsync(e.Author.Id, e.Guild.Id, e.Author.Username);

            int levelUpGold = profile.Level * 100;

            var leveledUpEmbed = new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level.ToString("###,###,###,###,###")}!",
                Description = $"{member.DisplayName} has been given {levelUpGold.ToString("###,###,###,###,###")} Gold for Levelling Up!",
                Color = DiscordColor.Gold,
            };

            leveledUpEmbed.WithThumbnail(member.AvatarUrl);

            await e.Channel.SendMessageAsync(embed: leveledUpEmbed).ConfigureAwait(false);

            return;
        }

        private Task OnClientErrored(ClientErrorEventArgs e)
        {
            var innerException = e.Exception.InnerException;
            var exceptionMessage = e.Exception.Message;

            Console.WriteLine(innerException);
            Console.WriteLine(exceptionMessage);

            return Task.CompletedTask;
        }

        private readonly ICustomCommandService _customCommandService;

        private async Task OnCommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception.Message is "Specified command was not found.")
            {
                var command = await _customCommandService.GetCommandAsync(e.Context.Message.Content.ToString()).ConfigureAwait(false);

                if (command == null)
                {
                    return;
                }

                await e.Context.Channel.SendMessageAsync(command.Action);

                return;
            }

            if (e.Exception is ChecksFailedException exception)
            {

                if (e.Exception is ChecksFailedException)
                {
                    if (e.Exception is CommandNotFoundException)
                    {
                        var command = await _customCommandService.GetCommandAsync(e.Context.Message.Content.ToString()).ConfigureAwait(false);

                        if (command == null)
                        {
                            return;
                        }

                        await e.Context.Channel.SendMessageAsync(command.Action);

                        return;
                    }

                    var properError = exception;

                    if (properError.FailedChecks[0] is RequireRolesAttribute)
                    {
                        var permissionEmbed = new DiscordEmbedBuilder
                        {
                            Title = "You do not have permission to run this command!",
                            Description = "Seek for those permissions or just stop being a jerk trying to use command's you're not allowed to touch!",
                            Color = DiscordColor.IndianRed,
                        };

                        await e.Context.Channel.SendMessageAsync(embed: permissionEmbed);
                        return;
                    }

                    if (properError.FailedChecks[0] is CooldownAttribute attribute)
                    {
                        var remainingCooldown = attribute.GetRemainingCooldown(e.Context);

                        var cooldownHours = remainingCooldown.Hours;
                        var cooldownMins = remainingCooldown.Minutes;
                        var cooldownSeconds = remainingCooldown.Seconds;

                        if (!cooldownHours.Equals(0))
                        {
                            var cooldownEmbed = new DiscordEmbedBuilder
                            {
                                Title = "This command is still on cooldown!",
                                Description = $"Please try again in {cooldownHours} Hours, {cooldownMins} Minutes, {cooldownSeconds} Seconds!",
                                Color = DiscordColor.IndianRed,
                            };

                            await e.Context.Channel.SendMessageAsync(embed: cooldownEmbed);

                            return;
                        }

                        if (!cooldownMins.Equals(0))
                        {
                            var cooldownEmbed = new DiscordEmbedBuilder
                            {
                                Title = "This command is still on cooldown!",
                                Description = $"Please try again in {cooldownMins} Minutes, {cooldownSeconds} Seconds!",
                                Color = DiscordColor.IndianRed,
                            };

                            await e.Context.Channel.SendMessageAsync(embed: cooldownEmbed);

                            return;
                        }

                        if (cooldownMins.Equals(0) && cooldownHours.Equals(0))
                        {
                            var cooldownEmbed = new DiscordEmbedBuilder
                            {
                                Title = "This command is still on cooldown!",
                                Description = $"Please try again in {cooldownSeconds} seconds!",
                                Color = DiscordColor.IndianRed,
                            };

                            await e.Context.Channel.SendMessageAsync(embed: cooldownEmbed);

                            return;
                        }

                        return;
                    }

                }
            }

            return;
        }

        private Task OnNewMember(GuildMemberAddEventArgs e)
        {
            _profileService.GetOrCreateProfileAsync(e.Member.Id, e.Guild.Id, e.Member.Username);

            var rules = e.Guild.Channels.Values.FirstOrDefault(x => x.Name == "discord-rules");
            var welcome = e.Guild.Channels.Values.FirstOrDefault(x => x.Name == "new-members");

            var joinEmbed = new DiscordEmbedBuilder
            {
                Title = $"Welcome to the Server {e.Member.DisplayName}",
                Description = $"Please read the {rules.Mention} channel before using the discord!",
                ImageUrl = "https://cdn.discordapp.com/attachments/615296929995292678/615297983470239811/NiceScratchyAllensbigearedbat-size_restricted.gif",
                Color = DiscordColor.Purple,
            };

            var totalMembers = e.Guild.MemberCount;
            var otherMembers = totalMembers - 1;

            joinEmbed.WithThumbnail(e.Member.AvatarUrl);
            joinEmbed.AddField($"Once again welcome to the server!", $"Thanks for joining the other {otherMembers.ToString("###,###,###,###,###")} of us!");

            welcome.SendMessageAsync(e.Member.Mention, embed: joinEmbed);

            return Task.CompletedTask;
        }

        private Task OnMemberLeave(GuildMemberRemoveEventArgs e)
        {
            _profileService.DeleteProfileAsync(e.Member.Id, e.Guild.Id, e.Member.Username);

            var welcome = e.Guild.Channels.Values.FirstOrDefault(x => x.Name == "new-members");

            var leaveEmbed = new DiscordEmbedBuilder
            {
                Title = $"Big Oof! {e.Member.DisplayName} has just left the server!",
                Description = $"Byeeeeeee, Don't let the door hit you on the way out!",
                ImageUrl = "https://cdn.discordapp.com/attachments/482347038734811154/612706560195428362/giphy.gif",
                Color = DiscordColor.Yellow,
            };

            leaveEmbed.WithThumbnail(e.Member.AvatarUrl);

            welcome.SendMessageAsync(embed: leaveEmbed);

            return Task.CompletedTask;
        }
    }
}
