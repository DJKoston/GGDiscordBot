using DiscordBot.Bots.Commands;
using DiscordBot.Core.Services.Configs;
using DiscordBot.Core.Services.CustomCommands;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.Core.Services.ReactionRoles;
using DiscordBot.Core.ViewModels;
using DiscordBot.DAL.Models.Configs;
using DiscordBot.DAL.Models.MessageStores;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace DiscordBot.Bots
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public LiveStreamMonitorService Monitor;
        public TwitchAPI api;

        
        public Bot(IServiceProvider services, IConfiguration configuration)
        {
            _profileService = services.GetService<IProfileService>();
            _experienceService = services.GetService<IExperienceService>();
            _customCommandService = services.GetService<ICustomCommandService>();
            _reactionRoleService = services.GetService<IReactionRoleService>();
            _nitroBoosterRoleConfigService = services.GetService<INitroBoosterRoleConfigService>();
            _welcomeMessageConfigService = services.GetService<IWelcomeMessageConfigService>();
            _guildStreamerConfigService = services.GetService<IGuildStreamerConfigService>();
            _messageStoreService = services.GetService<IMessageStoreService>();

            api = new TwitchAPI();

            var token = configuration["token"];
            var prefix = configuration["prefix"];
            var clientid = configuration["twitch-clientid"];
            var accesstoken = configuration["twitch-accesstoken"];
            var refreshtoken = configuration["twitch-refreshtoken"];

            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

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
            Client.Heartbeated += OnHeartbeat;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { prefix },
                EnableDms = true,
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
            Commands.RegisterCommands<NowLiveCommands>();
            Commands.RegisterCommands<ProfileCommands>();
            Commands.RegisterCommands<QuoteCommands>();
            Commands.RegisterCommands<ReactionRoleCommands>();
            Commands.RegisterCommands<RoleCommands>();

            Commands.CommandErrored += OnCommandErrored;

            Monitor = new LiveStreamMonitorService(api, 60);

            Monitor.OnStreamOnline += GGOnStreamOnline;
            Monitor.OnStreamOffline += GGOnStreamOffline;
            Monitor.OnStreamUpdate += GGOnStreamUpdate;

            Console.WriteLine("Connecting");
            Client.ConnectAsync();
            Console.WriteLine("Connected!");
        }

        private readonly IReactionRoleService _reactionRoleService;
        private readonly IProfileService _profileService;
        private readonly IExperienceService _experienceService;
        private readonly INitroBoosterRoleConfigService _nitroBoosterRoleConfigService;
        private readonly IWelcomeMessageConfigService _welcomeMessageConfigService;
        private readonly IGuildStreamerConfigService _guildStreamerConfigService;
        private readonly IMessageStoreService _messageStoreService;

        private async Task OnHeartbeat(HeartbeatEventArgs e)
        {
            var lst = _guildStreamerConfigService.GetGuildStreamerList();

            Monitor.SetChannelsByName(lst);

            var currentTime = DateTime.Now;

            if((currentTime.Hour == 2) && (currentTime.Minute == 55))
            {
                DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Name == "Generation Gamers");
                DiscordChannel gamesChannel = guild.Channels.Values.FirstOrDefault(x => x.Name == "discord-games");

                DiscordMember apocalyptic = await guild.GetMemberAsync(176666155103158273);
                DiscordMember djkoston = await guild.GetMemberAsync(331933713816616961);

                

                var embed = new DiscordEmbedBuilder
                {
                    Title = "The Bot will be unavaliable for the next 35 mins.",
                    Description = $"The server the bot runs on is creating a backup and to prevent loss of data during the backup the bot has been shutdown.\n\nIt will automatically come back up at 3:30am (UK Time).\n\nIf it has not come back up and it is after 3:30am (UK Time). Please DM {djkoston.Mention}",
                    Color = DiscordColor.DarkRed,
                };

                await gamesChannel.SendMessageAsync(embed: embed);
                await djkoston.SendMessageAsync(embed: embed);
                await apocalyptic.SendMessageAsync(embed: embed);

                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.ListeningTo,
                    Name = $"The Server Backup",
                }, UserStatus.DoNotDisturb);

                Environment.Exit(1);
            }

        }

        private void GGOnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            DiscordClient d = Client;

            var configs = _guildStreamerConfigService.GetGuildStreamerConfig(e.Stream.UserName);

            foreach (GuildStreamerConfig config in configs)
            {
                DiscordGuild guild = d.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if(guild == null) { continue; }

                var storedMessage = _messageStoreService.GetMessageStore(config.GuildId, e.Stream.UserName).Result;

                var messageId = storedMessage.AnnouncementMessageId;

                DiscordChannel channel = guild.GetChannel(storedMessage.AnnouncementChannelId);

                DiscordMessage message = channel.GetMessageAsync(messageId).Result;

                var stream = api.V5.Streams.GetStreamByUserAsync(e.Stream.UserId).Result;
                var streamer = api.V5.Users.GetUserByIDAsync(e.Stream.UserId).Result;

                if((storedMessage.StreamGame != stream.Stream.Game) && (storedMessage.StreamTitle != e.Stream.Title))
                {
                    var toReplaceMessage = config.AnnouncementMessage;
                    var channelReplace = toReplaceMessage.Replace("%USER%", e.Stream.UserName);
                    var userReplace = channelReplace.Replace("_", "\\_");
                    var gameReplace = userReplace.Replace("%GAME%", stream.Stream.Game);
                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Description = $"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})",
                        Color = color,
                    };

                    embed.AddField("Game:", stream.Stream.Game);
                    embed.AddField("Followers:", stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###"), true);
                    embed.AddField("Total Viewers:", stream.Stream.Channel.Views.ToString("###,###,###,###,###,###"), true);

                    embed.WithThumbnail(streamer.Logo);
                    embed.WithImageUrl(stream.Stream.Preview.Large);
                    embed.WithFooter($"Stream went live at: {e.Stream.StartedAt}", "https://www.iconfinder.com/data/icons/social-messaging-ui-color-shapes-2-free/128/social-twitch-circle-512.png");

                    DiscordEmbed updatedEmbed = embed;

                    message.ModifyAsync(announcementMessage, embed: updatedEmbed);

                    _messageStoreService.RemoveMessageStore(storedMessage);

                    var messageStore = new NowLiveMessages
                    {
                        GuildId = storedMessage.GuildId,
                        StreamerId = storedMessage.StreamerId,
                        AnnouncementChannelId = storedMessage.AnnouncementChannelId,
                        AnnouncementMessageId = storedMessage.AnnouncementMessageId,
                        StreamTitle = e.Stream.Title,
                        StreamGame = stream.Stream.Game,
                    };

                    _messageStoreService.CreateNewMessageStore(messageStore);

                    continue;
                }

                if(storedMessage.StreamGame != stream.Stream.Game)
                {
                    var toReplaceMessage = config.AnnouncementMessage;
                    var channelReplace = toReplaceMessage.Replace("%USER%", e.Stream.UserName);
                    var userReplace = channelReplace.Replace("_", "\\_");
                    var gameReplace = userReplace.Replace("%GAME%", stream.Stream.Game);
                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} "); 

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Description = $"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})",
                        Color = color,
                    };

                    embed.AddField("Game:", stream.Stream.Game);
                    embed.AddField("Followers:", stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###"), true);
                    embed.AddField("Total Viewers:", stream.Stream.Channel.Views.ToString("###,###,###,###,###,###"), true);

                    embed.WithThumbnail(streamer.Logo);
                    embed.WithImageUrl(stream.Stream.Preview.Large);
                    embed.WithFooter($"Stream went live at: {e.Stream.StartedAt}", "https://www.iconfinder.com/data/icons/social-messaging-ui-color-shapes-2-free/128/social-twitch-circle-512.png");

                    DiscordEmbed updatedEmbed = embed;

                    message.ModifyAsync(announcementMessage, embed: updatedEmbed);

                    _messageStoreService.RemoveMessageStore(storedMessage);

                    var messageStore = new NowLiveMessages
                    {
                        GuildId = storedMessage.GuildId,
                        StreamerId = storedMessage.StreamerId,
                        AnnouncementChannelId = storedMessage.AnnouncementChannelId,
                        AnnouncementMessageId = storedMessage.AnnouncementMessageId,
                        StreamTitle = e.Stream.Title,
                        StreamGame = stream.Stream.Game,
                    };

                    _messageStoreService.CreateNewMessageStore(messageStore);
                }

                if (storedMessage.StreamTitle != e.Stream.Title)
                {
                    var toReplaceMessage = config.AnnouncementMessage;
                    var channelReplace = toReplaceMessage.Replace("%USER%", e.Stream.UserName);
                    var userReplace = channelReplace.Replace("_", "\\_");
                    var gameReplace = userReplace.Replace("%GAME%", stream.Stream.Game);
                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Description = $"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})",
                        Color = color,
                    };

                    embed.AddField("Game:", stream.Stream.Game);
                    embed.AddField("Followers:", stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###"), true);
                    embed.AddField("Total Viewers:", stream.Stream.Channel.Views.ToString("###,###,###,###,###,###"), true);

                    embed.WithThumbnail(streamer.Logo);
                    embed.WithImageUrl(stream.Stream.Preview.Large);
                    embed.WithFooter($"Stream went live at: {e.Stream.StartedAt}", "https://www.iconfinder.com/data/icons/social-messaging-ui-color-shapes-2-free/128/social-twitch-circle-512.png");

                    DiscordEmbed updatedEmbed = embed;

                    message.ModifyAsync(announcementMessage, embed: updatedEmbed);

                    _messageStoreService.RemoveMessageStore(storedMessage);

                    var messageStore = new NowLiveMessages
                    {
                        GuildId = storedMessage.GuildId,
                        StreamerId = storedMessage.StreamerId,
                        AnnouncementChannelId = storedMessage.AnnouncementChannelId,
                        AnnouncementMessageId = storedMessage.AnnouncementMessageId,
                        StreamTitle = e.Stream.Title,
                        StreamGame = stream.Stream.Game,
                    };

                    _messageStoreService.CreateNewMessageStore(messageStore);
                }
            }
        }

        private void GGOnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            DiscordClient d = Client;

            var configs = _guildStreamerConfigService.GetGuildStreamerConfig(e.Stream.UserName);

            foreach(GuildStreamerConfig config in configs)
            {
                var gsConfig = _guildStreamerConfigService.GetGuildStreamerConfig(config.StreamerId);

                var storedMessage = _messageStoreService.GetMessageStore(config.GuildId, config.StreamerId).Result;

                if (storedMessage != null) { continue; }

                DiscordGuild guild = d.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if(guild == null) { continue; }

                DiscordChannel channel = guild.GetChannel(config.AnnounceChannelId);

                var stream = api.V5.Streams.GetStreamByUserAsync(e.Stream.UserId).Result;
                var streamer = api.V5.Users.GetUserByIDAsync(e.Stream.UserId).Result;

                var toReplaceMessage = config.AnnouncementMessage;
                var channelReplace = toReplaceMessage.Replace("%USER%", e.Stream.UserName);
                var userReplace = channelReplace.Replace("_", "\\_");
                var gameReplace = userReplace.Replace("%GAME%", stream.Stream.Game);
                var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                var color = new DiscordColor("9146FF");

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{e.Stream.UserName} has gone live!",
                    Description = $"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})",
                    Color = color,
                };

                embed.AddField("Game:", stream.Stream.Game);
                embed.AddField("Followers:", stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###"), true);
                embed.AddField("Total Viewers:", stream.Stream.Channel.Views.ToString("###,###,###,###,###,###"), true);

                embed.WithThumbnail(streamer.Logo);
                embed.WithImageUrl(stream.Stream.Preview.Large);
                embed.WithFooter($"Stream went live at: {e.Stream.StartedAt}", "https://www.iconfinder.com/data/icons/social-messaging-ui-color-shapes-2-free/128/social-twitch-circle-512.png");


                DiscordMessage sentMessage = channel.SendMessageAsync(announcementMessage, embed: embed).Result;

                var messageStore = new NowLiveMessages
                {
                    GuildId = config.GuildId,
                    StreamerId = config.StreamerId,
                    AnnouncementChannelId = channel.Id,
                    AnnouncementMessageId = sentMessage.Id,
                    StreamTitle = e.Stream.Title,
                    StreamGame = stream.Stream.Game,
                };

                _messageStoreService.CreateNewMessageStore(messageStore);
            }

        }

        private void GGOnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            DiscordClient d = Client;

            var configs = _guildStreamerConfigService.GetGuildStreamerConfig(e.Stream.UserName);

            foreach (GuildStreamerConfig config in configs)
            {
                DiscordGuild guild = d.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);
                
                var storedMessage = _messageStoreService.GetMessageStore(config.GuildId, e.Stream.UserName).Result;

                var messageId = storedMessage.AnnouncementMessageId;

                var channel = guild.GetChannel(storedMessage.AnnouncementChannelId);

                DiscordMessage message = channel.GetMessageAsync(messageId).Result;

                message.DeleteAsync();

                _messageStoreService.RemoveMessageStore(storedMessage);
            }
        }

        private async Task OnPresenceUpdated(PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot) { return; }

            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);

            if (guild == null) { return; }

            DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);

            if (member == null) { return; }

            if (e.User.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
            {
                DiscordRole generationGamers = guild.GetRole(411304802883207169);
                DiscordRole ggNowLive = guild.GetRole(745018263456448573);
                DiscordRole NowLive = guild.GetRole(745018328179015700);

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
                DiscordRole generationGamers = guild.GetRole(411304802883207169);
                DiscordRole ggNowLive = guild.GetRole(745018263456448573);
                DiscordRole NowLive = guild.GetRole(745018328179015700);

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
            

        private async Task OnGuildAvaliable(GuildCreateEventArgs e)
        {
            var lst = _guildStreamerConfigService.GetGuildStreamerList();

            foreach (string streamerList in lst)
            {
                var configs = _guildStreamerConfigService.GetGuildStreamerConfig(streamerList);

                var storedMessage = _messageStoreService.GetMessageStore(e.Guild.Id, streamerList).Result;

                if (storedMessage == null) { continue; }

                var user = await api.V5.Users.GetUserByNameAsync(streamerList);
             
                var isStreaming = await api.V5.Streams.BroadcasterOnlineAsync(user.Matches.First().Id);

                if(isStreaming == true) { continue; }

                var messageId = storedMessage.AnnouncementMessageId;

                var channel = e.Guild.GetChannel(storedMessage.AnnouncementChannelId);

                var message = await channel.GetMessageAsync(messageId);

                await message.DeleteAsync();

                await _messageStoreService.RemoveMessageStore(storedMessage);
            }

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
                        await gglivemember.GrantRoleAsync(ggNowLive);
                    }
                }

                foreach (DiscordMember nullmember in nullwithNowLive)
                {
                    if (nullmember.Roles.Contains(ggNowLive))
                    {
                        await nullmember.RevokeRoleAsync(ggNowLive);
                    }

                    else
                    {

                    }

                }

                foreach (DiscordMember ggwaslivemember in ggwaslive)
                {
                    if (ggwaslivemember.Roles.Contains(ggNowLive))
                    {
                        await ggwaslivemember.RevokeRoleAsync(ggNowLive);
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
                        await otherlivemember.GrantRoleAsync(NowLive);
                    }

                }

                foreach (DiscordMember nullmember in otherwithNowLive)
                {
                    if (nullmember.Roles.Contains(NowLive))
                    {
                        await nullmember.RevokeRoleAsync(NowLive);
                    }

                    else
                    {

                    }

                }

                foreach (DiscordMember otherwaslivemember in otherwaslive)
                {
                    if (otherwaslivemember.Roles.Contains(NowLive))
                    {
                        await otherwaslivemember.RevokeRoleAsync(NowLive);
                    }

                    else
                    {

                    }

                }

                return;
            }

            return;
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

        private async Task OnClientReady(ReadyEventArgs e)
        {
            var lst = _guildStreamerConfigService.GetGuildStreamerList();

            Monitor.SetChannelsByName(lst);

            Monitor.Start();

            int guilds = e.Client.Guilds.Count();

            if(guilds == 1)
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

            return;
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate) { return; }

            DiscordGuild guild = e.Client.Guilds.Values.FirstOrDefault(x => x.Id == e.Guild.Id);
            DiscordMember memberCheck = await guild.GetMemberAsync(e.Author.Id);

            if (e.Author.IsBot)
            {
                return;
            }

            var NBConfig = _nitroBoosterRoleConfigService.GetNitroBoosterConfig(e.Guild.Id).Result;

            if(NBConfig == null)
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

            DiscordRole NitroBooster = guild.GetRole(NBConfig.RoleId);

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
