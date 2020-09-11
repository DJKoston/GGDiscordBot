using DiscordBot.Bots.Commands;
using DiscordBot.Core.Services.Configs;
using DiscordBot.Core.Services.CustomCommands;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.Core.Services.ReactionRoles;
using DiscordBot.Core.ViewModels;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bots
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public Bot(IServiceProvider services, IConfiguration configuration)
        {
            _profileService = services.GetService<IProfileService>();
            _experienceService = services.GetService<IExperienceService>();
            _customCommandService = services.GetService<ICustomCommandService>();
            _reactionRoleService = services.GetService<IReactionRoleService>();
            _nitroBoosterRoleConfigService = services.GetService<INitroBoosterRoleConfigService>();
            _welcomeMessageConfigService = services.GetService<IWelcomeMessageConfigService>();

            var token = configuration["token"];
            var prefix = configuration["prefix"];

            var config = new DiscordConfiguration
            {
                Token = token,
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
            Client.GuildAvailable += OnGuildAvaliable;
            Client.PresenceUpdated += OnPresenceUpdated;
            Client.GuildCreated += OnGuildJoin;
            Client.GuildDeleted += OnGuildLeave;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                Services = services,

            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<AdminCommands>();
            Commands.RegisterCommands<ConfigCommands>();
            Commands.RegisterCommands<GameCommands>();
            Commands.RegisterCommands<LeaveRoleCommands>();
            Commands.RegisterCommands<ManageCommands>();
            Commands.RegisterCommands<MiscCommands>();
            Commands.RegisterCommands<ModCommands>();
            Commands.RegisterCommands<ProfileCommands>();
            Commands.RegisterCommands<QuoteCommands>();
            Commands.RegisterCommands<ReactionRoleCommands>();
            Commands.RegisterCommands<RoleCommands>();

            Commands.CommandErrored += OnCommandErrored;

            Console.WriteLine("Connecting");
            Client.ConnectAsync();
            Console.WriteLine("Connected!");
        }

        private readonly IReactionRoleService _reactionRoleService;
        private readonly IProfileService _profileService;
        private readonly IExperienceService _experienceService;
        private readonly INitroBoosterRoleConfigService _nitroBoosterRoleConfigService;
        private readonly IWelcomeMessageConfigService _welcomeMessageConfigService;

        private async Task OnPresenceUpdated(PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot) { return; }

            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);
            if (guild == null) { return; }
            DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
            if (guild == null) { return; }
            

            DiscordRole generationGamers = guild.GetRole(411304802883207169);
            DiscordRole ggNowLive = guild.GetRole(745018263456448573);
            DiscordRole NowLive = guild.GetRole(745018328179015700);

            if (e.User.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
            {
                if (member.Roles.Contains(generationGamers))
                {
                    await member.GrantRoleAsync(ggNowLive);
                }

                else
                {
                    await member.GrantRoleAsync(NowLive);
                }
                
            }

            else
            {
                if (member.Roles.Contains(ggNowLive))
                {
                    await member.RevokeRoleAsync(ggNowLive);
                }

                if (member.Roles.Contains(NowLive))
                {
                    await member.RevokeRoleAsync(NowLive);
                }
            }

            return;
        }

        private async Task OnGuildJoin(GuildCreateEventArgs e)
        {
            int guilds = e.Client.Guilds.Count();

            if (guilds == 1)
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"{guilds} Server!",
                }, UserStatus.Online);
            }

            else
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"{guilds} Servers!",
                }, UserStatus.Online);
            }
        }

        private async Task OnGuildLeave(GuildDeleteEventArgs e)
        {
            int guilds = e.Client.Guilds.Count();

            if (guilds == 1)
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"{guilds} Server!",
                }, UserStatus.Online);
            }

            else
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"{guilds} Servers!",
                }, UserStatus.Online);
            }

        }

        private Task OnGuildAvaliable(GuildCreateEventArgs e)
        {
            if (e.Guild.Id == 246691304447279104)
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

            return Task.CompletedTask;
        }

        private async Task OnReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot)
            {
                return;
            }

            var reactionRole = _reactionRoleService.GetReactionRole(e.Guild.Id, e.Channel.Id, e.Message.Id, e.Emoji.Id, e.Emoji.Name).Result;

            if (reactionRole == null) { return; }

            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == reactionRole.GuildId);
            DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
            DiscordRole role = guild.GetRole(reactionRole.RoleId);

            await member.RevokeRoleAsync(role);

            return;
        }

        private async Task OnReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot)
            {
                return;
            }

            var reactionRole = _reactionRoleService.GetReactionRole(e.Guild.Id, e.Channel.Id, e.Message.Id, e.Emoji.Id, e.Emoji.Name).Result;

            if(reactionRole == null) { return; }

            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == reactionRole.GuildId);
            DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
            DiscordRole role = guild.GetRole(reactionRole.RoleId);

            await member.GrantRoleAsync(role);

            return;
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            int guilds = e.Client.Guilds.Count();

            if(guilds == 1)
            {
                Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"{guilds} Server!",
                }, UserStatus.Online);
            }

            else
            {
                Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"{guilds} Servers!",
                }, UserStatus.Online);
            }

            return Task.CompletedTask;
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
            {
                return;
            }

            var NBConfig = _nitroBoosterRoleConfigService.GetNitroBoosterConfig(e.Guild.Id).Result;

            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == e.Guild.Id);
            DiscordRole NitroBooster = guild.GetRole(NBConfig.RoleId);
            DiscordMember memberCheck = await guild.GetMemberAsync(e.Author.Id);

            if (memberCheck.Roles.Contains(NitroBooster))
            {
                var member = e.Guild.Members[e.Author.Id];

                var randomNumber = new Random();

                int randXP = randomNumber.Next(50);

                int NitroXP = randXP * 2;

                GrantXpViewModel viewModel = await _experienceService.GrantXpAsync(e.Author.Id, e.Guild.Id, NitroXP, e.Author.Username);

                if (!viewModel.LevelledUp) { return; }

                Profile profile = await _profileService.GetOrCreateProfileAsync(e.Author.Id, e.Guild.Id, e.Author.Username);

                int levelUpGold = profile.Level * 100;

                var leveledUpEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level:###,###,###,###,###}!",
                    Description = $"{member.DisplayName} has been given {levelUpGold:###,###,###,###,###} Gold for Levelling Up!",
                    Color = DiscordColor.Gold,
                };

                leveledUpEmbed.WithThumbnail(member.AvatarUrl);

                await e.Channel.SendMessageAsync(embed: leveledUpEmbed).ConfigureAwait(false);

                return;
            }

            else
            {
                var member = e.Guild.Members[e.Author.Id];

                var randomNumber = new Random();

                int randXP = randomNumber.Next(50);

                GrantXpViewModel viewModel = await _experienceService.GrantXpAsync(e.Author.Id, e.Guild.Id, randXP, e.Author.Username);

                if (!viewModel.LevelledUp) { return; }

                Profile profile = await _profileService.GetOrCreateProfileAsync(e.Author.Id, e.Guild.Id, e.Author.Username);

                int levelUpGold = (profile.Level * 100);

                var leveledUpEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level:###,###,###,###,###}!",
                    Description = $"{member.DisplayName} has been given {levelUpGold:###,###,###,###,###} Gold for Levelling Up!",
                    Color = DiscordColor.Gold,
                };

                leveledUpEmbed.WithThumbnail(member.AvatarUrl);

                await e.Channel.SendMessageAsync(embed: leveledUpEmbed).ConfigureAwait(false);

                return;
            }

            
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
                var command = await _customCommandService.GetCommandAsync(e.Context.Message.Content.ToString(), e.Context.Guild.Id).ConfigureAwait(false);

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
                        var command = await _customCommandService.GetCommandAsync(e.Context.Message.Content.ToString(), e.Context.Guild.Id).ConfigureAwait(false);

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

        private async Task OnNewMember(GuildMemberAddEventArgs e)
        {
            var WMConfig = _welcomeMessageConfigService.GetWelcomeMessageConfig(e.Guild.Id).Result;

            if(WMConfig == null) { return; }

            else
            {
                DiscordChannel welcome = e.Guild.GetChannel(WMConfig.ChannelId);

                if (e.Member.IsBot)
                {
                    var joinEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Welcome to the Server {e.Member.DisplayName}",
                        Description = $"{WMConfig.WelcomeMessage}",
                        ImageUrl = $"{WMConfig.WelcomeImage}",
                        Color = DiscordColor.Purple,
                    };

                    var totalMembers = e.Guild.MemberCount;
                    var otherMembers = totalMembers - 1;

                    joinEmbed.WithThumbnail(e.Member.AvatarUrl);
                    joinEmbed.AddField($"Once again welcome to the server!", $"Thanks for joining the other {otherMembers:###,###,###,###,###} of us!");

                    await welcome.SendMessageAsync(e.Member.Mention, embed: joinEmbed);
                }

                else
                {
                    await _profileService.GetOrCreateProfileAsync(e.Member.Id, e.Guild.Id, e.Member.Username);

                    var joinEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Welcome to the Server {e.Member.DisplayName}",
                        Description = $"{WMConfig.WelcomeMessage}",
                        ImageUrl = $"{WMConfig.WelcomeImage}",
                        Color = DiscordColor.Purple,
                    };

                    var totalMembers = e.Guild.MemberCount;
                    var otherMembers = totalMembers - 1;

                    joinEmbed.WithThumbnail(e.Member.AvatarUrl);
                    joinEmbed.AddField($"Once again welcome to the server!", $"Thanks for joining the other {otherMembers:###,###,###,###,###} of us!");

                    await welcome.SendMessageAsync(e.Member.Mention, embed: joinEmbed);
                }
            }
        }

        private async Task OnMemberLeave(GuildMemberRemoveEventArgs e)
        {
            var WMConfig = _welcomeMessageConfigService.GetWelcomeMessageConfig(e.Guild.Id).Result;

            if(WMConfig == null) { return; }

            else
            {
                DiscordChannel welcome = e.Guild.GetChannel(WMConfig.ChannelId);

                if (e.Member.IsBot)
                {
                    var leaveEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Big Oof! {e.Member.DisplayName} has just left the server!",
                        Description = $"{WMConfig.LeaveMessage}",
                        ImageUrl = $"{WMConfig.LeaveImage}",
                        Color = DiscordColor.Yellow,
                    };

                    leaveEmbed.WithThumbnail(e.Member.AvatarUrl);

                    await welcome.SendMessageAsync(embed: leaveEmbed);
                }

                else
                {
                    await _profileService.DeleteProfileAsync(e.Member.Id, e.Guild.Id, e.Member.Username);

                    var leaveEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Big Oof! {e.Member.DisplayName} has just left the server!",
                        Description = $"{WMConfig.LeaveMessage}",
                        ImageUrl = $"{WMConfig.LeaveImage}",
                        Color = DiscordColor.Yellow,
                    };

                    leaveEmbed.WithThumbnail(e.Member.AvatarUrl);

                    await welcome.SendMessageAsync(embed: leaveEmbed);
                }
            }

            
            
        }
    }
}
