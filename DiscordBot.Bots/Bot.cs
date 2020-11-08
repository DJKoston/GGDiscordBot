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
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
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
            _gameChannelConfigService = services.GetService<IGameChannelConfigService>();
            _nowLiveRoleConfigService = services.GetService<INowLiveRoleConfigService>();

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

            Client.Heartbeated += OnHeartbeat;
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
            Client.GuildUnavailable += OnGuildUnAvaliable;

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
            Commands.RegisterCommands<StreamerCommands>();
            Commands.RegisterCommands<SuggestionCommands>();

            Commands.CommandErrored += OnCommandErrored;

            Monitor = new LiveStreamMonitorService(api, 60);

            Monitor.OnStreamOnline += GGOnStreamOnline;
            Monitor.OnStreamOffline += GGOnStreamOffline;

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
        private readonly ICustomCommandService _customCommandService;
        private readonly IGameChannelConfigService _gameChannelConfigService;
        private readonly INowLiveRoleConfigService _nowLiveRoleConfigService;

        private async Task OnHeartbeat(DiscordClient c, HeartbeatEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Heartbeated. Ping: {e.Ping}ms");
            Console.ResetColor();

            // pull list of streamers in NL feature from DB
            var lst = _guildStreamerConfigService.GetGuildStreamerList();

            // Sets the channels the bot needs to monitor.
            if (lst.Count() != 0) { Monitor.SetChannelsById(lst); }

            // If the Monitor is disabled - It will enable the monitor.
            if (lst.Count() != 0 && Monitor.Enabled == false) { Monitor.Start(); Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($"Twitch Monitor 1 has started monitoring {lst.Count} Channels."); Console.ResetColor(); }

            var currentTime = DateTime.Now;

            if ((currentTime.Hour == 2) && (currentTime.Minute == 55))
            {
                DiscordGuild guild = Client.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);

                DiscordMember apocalyptic = guild.GetMemberAsync(176666155103158273).Result;
                DiscordMember djkoston = guild.GetMemberAsync(331933713816616961).Result;

                var embed = new DiscordEmbedBuilder
                {
                    Title = "The Bot will be unavaliable for the next 15-60 mins.",
                    Description = $"The server the bot runs on is creating a backup and to prevent loss of data during the backup the bot has been shutdown.\n\nDepending on what time the backup completes it will automatically come back up between 3:10am and 4:00am (UK Time).\n\nIf it has not come back up by 4:00am (UK Time). Please DM {djkoston.Mention}",
                    Color = DiscordColor.DarkRed,
                };

                var configuredGamesChannels = _gameChannelConfigService.GetGameChannelConfigs();

                foreach (GameChannelConfig channel in configuredGamesChannels)
                {
                    DiscordGuild guild1 = c.Guilds.Values.FirstOrDefault(x => x.Id == channel.GuildId);

                    if (guild1 == null) { continue; }

                    DiscordChannel gamesChannel = guild1.Channels.Values.FirstOrDefault(x => x.Id == channel.ChannelId);

                    if (gamesChannel == null) { continue; }

                    await gamesChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                }

                await djkoston.SendMessageAsync(embed: embed).ConfigureAwait(false);
                await apocalyptic.SendMessageAsync(embed: embed).ConfigureAwait(false);

                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.ListeningTo,
                    Name = $"The Server Backup",
                }, UserStatus.DoNotDisturb);

                Environment.Exit(1);
            }

            return;
        }

        private void GGOnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{e.Stream.UserName} just went Live!");
            Console.ResetColor();
            
            var configs = _guildStreamerConfigService.GetGuildStreamerConfig(e.Stream.UserId);

            foreach (GuildStreamerConfig config in configs)
            {
                var storedMessage = _messageStoreService.GetMessageStore(config.GuildId, config.StreamerId).Result;

                if (storedMessage != null) { continue; }

                DiscordGuild guild = Client.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { continue; }

                DiscordChannel channel = guild.GetChannel(config.AnnounceChannelId);

                var stream = api.V5.Streams.GetStreamByUserAsync(e.Stream.UserId).Result;
                var streamer = api.V5.Users.GetUserByIDAsync(e.Stream.UserId).Result;

                var toReplaceMessage = config.AnnouncementMessage;

                var channelReplace = toReplaceMessage.Replace("%USER%", e.Stream.UserName);

                if (e.Stream.UserName.Contains("_"))
                {
                    var userReplace = channelReplace.Replace("_", "\\_");

                    var gameReplace = userReplace.Replace("%GAME%", stream.Stream.Game);
                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Color = color,
                    };

                    if (e.Stream.Title != null) { embed.WithDescription($"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})"); }

                    if (stream.Stream.Game != null) { embed.AddField("Game:", stream.Stream.Game); }

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

                    if (streamer.DisplayName == config.StreamerName)
                    {
                        continue;
                    }

                    else
                    {
                        config.StreamerName = streamer.DisplayName;

                        _guildStreamerConfigService.EditUser(config);

                        Console.WriteLine($"{config.StreamerId}'s name has been set to {streamer.DisplayName}");
                    }
                }

                else
                {
                    var gameReplace = channelReplace.Replace("%GAME%", stream.Stream.Game);
                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Color = color,
                    };

                    if (e.Stream.Title != null) { embed.WithDescription($"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})"); }

                    if (stream.Stream.Game != "\"\"") { embed.AddField("Game:", stream.Stream.Game); }

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

                    if (streamer.DisplayName == config.StreamerName)
                    {
                        continue;
                    }

                    else
                    {
                        config.StreamerName = streamer.DisplayName;

                        _guildStreamerConfigService.EditUser(config);

                        Console.WriteLine($"{config.StreamerId}'s name has been set to {streamer.DisplayName}");
                    }
                }
            }

        }

        private void GGOnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{e.Stream.UserName} just went Offline!");
            Console.ResetColor();

            var streamUser = api.V5.Users.GetUserByNameAsync(e.Stream.UserName).Result;

            var streamResults = streamUser.Matches.FirstOrDefault();

            var streamerId = streamResults.Id;

            var getStreamId = api.V5.Channels.GetChannelByIDAsync(streamerId).Result;

            var configs = _guildStreamerConfigService.GetGuildStreamerConfig(getStreamId.Id);

            foreach (GuildStreamerConfig config in configs)
            {
                DiscordGuild guild = Client.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { continue; }

                var storedMessage = _messageStoreService.GetMessageStore(config.GuildId, getStreamId.Id).Result;

                var messageId = storedMessage.AnnouncementMessageId;

                var channel = guild.GetChannel(storedMessage.AnnouncementChannelId);

                DiscordMessage message = channel.GetMessageAsync(messageId).Result;

                if (message == null) { continue; }

                message.DeleteAsync();

                _messageStoreService.RemoveMessageStore(storedMessage);
            }
        }

        private async Task OnPresenceUpdated(DiscordClient c, PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot) { return; }

            var configs = _nowLiveRoleConfigService.GetAllConfigs();

            foreach(NowLiveRoleConfig config in configs)
            {
                DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { return; }

                DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);

                if (member == null) { return; }

                if (e.User.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
                {
                    DiscordRole NowLive = guild.GetRole(745018328179015700);

                    if (member.Roles.Contains(NowLive)) { return; }

                    else
                    {
                        await member.GrantRoleAsync(NowLive);
                    }

                }

                else
                {
                    DiscordRole NowLive = guild.GetRole(config.RoleId);

                    if (member.Roles.Contains(NowLive))
                    {
                        await member.RevokeRoleAsync(NowLive);
                    }
                }
            }

            return;
        }

        private async Task OnGuildJoin(DiscordClient c, GuildCreateEventArgs e)
        {
            int guilds = c.Guilds.Count();

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

        private async Task OnGuildLeave(DiscordClient c, GuildDeleteEventArgs e)
        {
            int guilds = c.Guilds.Count();

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

        private Task OnGuildUnAvaliable(DiscordClient c, GuildDeleteEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{e.Guild.Name} is now Unavaliable");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        private Task OnGuildAvaliable(DiscordClient c, GuildCreateEventArgs e)
        {
            new Thread(async () =>
            {
                var lst = _guildStreamerConfigService.GetGuildStreamerList();

                foreach (string streamerList in lst)
                {
                    var configs = _guildStreamerConfigService.GetGuildStreamerConfig(streamerList);

                    var storedMessage = _messageStoreService.GetMessageStore(e.Guild.Id, streamerList).Result;

                    if (storedMessage == null) { continue; }

                    var user = await api.V5.Users.GetUserByIDAsync(streamerList);

                    var isStreaming = await api.V5.Streams.BroadcasterOnlineAsync(user.Id);

                    if (isStreaming == true) { continue; }

                    var messageId = storedMessage.AnnouncementMessageId;

                    var channel = e.Guild.GetChannel(storedMessage.AnnouncementChannelId);

                    var message = await channel.GetMessageAsync(messageId);

                    await message.DeleteAsync();

                    await _messageStoreService.RemoveMessageStore(storedMessage);
                }

                if (e.Guild.Id == 246691304447279104)
                {
                    var currentTime1 = DateTime.Now;

                    DiscordGuild guild1 = c.Guilds.Values.FirstOrDefault(x => x.Id == 246691304447279104);

                    if ((currentTime1.Hour == 03) && (currentTime1.Minute > 09) && (currentTime1.Minute < 60))
                    {
                        DiscordMember apocalyptic = await guild1.GetMemberAsync(176666155103158273);
                        DiscordMember djkoston = await guild1.GetMemberAsync(331933713816616961);

                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "The Bot is now back online!",
                            Description = $"The server the bot runs on has finished the backup and the bot should now respond to requests.\n\nIf it still isnt responding, please DM {djkoston.Mention}",
                            Color = DiscordColor.DarkGreen,
                        };

                        if (djkoston != null) { await djkoston.SendMessageAsync(embed: embed).ConfigureAwait(false); }

                        if (apocalyptic != null) { await apocalyptic.SendMessageAsync(embed: embed).ConfigureAwait(false); }
                    }

                }

                var currentTime = DateTime.Now;

                if ((currentTime.Hour == 03) && (currentTime.Minute > 09) && (currentTime.Minute < 60))
                {
                    GameChannelConfig channel = await _gameChannelConfigService.GetGameChannelConfigService(e.Guild.Id);

                    if (channel == null) { return; }

                    DiscordGuild guild2 = c.Guilds.Values.FirstOrDefault(x => x.Id == channel.GuildId);

                    DiscordMember djkoston = await guild2.GetMemberAsync(331933713816616961);

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "The Bot is now back online!",
                        Description = $"The server the bot runs on has finished the backup and the bot should now respond to requests.\n\nIf it still isnt responding, please DM {djkoston.Mention}",
                        Color = DiscordColor.DarkGreen,
                    };

                    DiscordChannel gamesChannel = guild2.Channels.Values.FirstOrDefault(x => x.Id == channel.ChannelId);

                    if (gamesChannel != null) { await gamesChannel.SendMessageAsync(embed: embed).ConfigureAwait(false); }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{e.Guild.Name} is now Avaliable");
                Console.ResetColor();

                DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == e.Guild.Id);

                var config = await _nowLiveRoleConfigService.GetNowLiveRoleConfig(e.Guild.Id);

                if(config == null) { return; }

                var allMembers = await guild.GetAllMembersAsync();

                DiscordRole NowLive = guild.GetRole(config.RoleId);

                var nullother = allMembers.Where(x => x.Presence == null);
                var otherwithNowLive = nullother.Where(x => x.Roles.Contains(NowLive));
                var othermembers = allMembers.Except(nullother);
                var otherlivemembers = othermembers.Where(x => x.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)));
                var othernotlivemembers = othermembers.Except(otherlivemembers);
                var otherwaslive = othernotlivemembers.Where(x => x.Roles.Contains(NowLive));

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

            }).Start();

            return Task.CompletedTask;
        }

        private async Task OnReactionRemoved(DiscordClient c, MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot)
            {
                return;
            }

            var reactionRole = _reactionRoleService.GetReactionRole(e.Guild.Id, e.Channel.Id, e.Message.Id, e.Emoji.Id, e.Emoji.Name).Result;

            if (reactionRole == null) { return; }

            DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == reactionRole.GuildId);
            DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
            DiscordRole role = guild.GetRole(reactionRole.RoleId);

            await member.RevokeRoleAsync(role);

            return;
        }

        private async Task OnReactionAdded(DiscordClient c, MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot)
            {
                return;
            }

            var reactionRole = _reactionRoleService.GetReactionRole(e.Guild.Id, e.Channel.Id, e.Message.Id, e.Emoji.Id, e.Emoji.Name).Result;

            if (reactionRole == null) { return; }

            DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == reactionRole.GuildId);
            DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
            DiscordRole role = guild.GetRole(reactionRole.RoleId);

            await member.GrantRoleAsync(role);

            return;
        }

        private async Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            int guilds = c.Guilds.Count();

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

            if(Environment.MachineName == "HAYDON-PC")
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = "the latest test build",
                }, UserStatus.Idle);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{c.CurrentUser.Username} is Ready");
            Console.ResetColor();
        }

        private async Task OnMessageCreated(DiscordClient c, MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate) { return; }

            if (e.Author.IsBot) { return; }

            if (e.Message.Content.Contains("!")) { return; }

            DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == e.Guild.Id);
            DiscordMember memberCheck = await guild.GetMemberAsync(e.Author.Id);

            var NBConfig = _nitroBoosterRoleConfigService.GetNitroBoosterConfig(e.Guild.Id).Result;

            if (NBConfig == null)
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

        private Task OnClientErrored(DiscordClient c, ClientErrorEventArgs e)
        {
            var innerException = e.Exception.InnerException;
            var exceptionMessage = e.Exception.Message;

            Console.WriteLine(innerException);
            Console.WriteLine(exceptionMessage);

            return Task.CompletedTask;
        }

        private async Task OnCommandErrored(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);

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

        private async Task OnNewMember(DiscordClient c, GuildMemberAddEventArgs e)
        {
            var WMConfig = _welcomeMessageConfigService.GetWelcomeMessageConfig(e.Guild.Id).Result;

            if (WMConfig == null) { return; }

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

        private async Task OnMemberLeave(DiscordClient c, GuildMemberRemoveEventArgs e)
        {
            var WMConfig = _welcomeMessageConfigService.GetWelcomeMessageConfig(e.Guild.Id).Result;

            if (WMConfig == null) { return; }

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
