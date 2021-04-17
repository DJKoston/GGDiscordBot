using DiscordBot.Bots.Commands;
using DiscordBot.Bots.Handlers.HelpFormatters;
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
using System.Diagnostics;
using System.IO;
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
            _nowLiveRoleConfigService = services.GetService<INowLiveRoleConfigService>();
            _goodBotBadBotService = services.GetService<IGoodBotBadBotService>();
            _currencyNameService = services.GetService<ICurrencyNameConfigService>();

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
                Intents = DiscordIntents.All
            };

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

            Client = new DiscordClient(config);

            Client.Heartbeated += OnHeartbeat;
            Client.Ready += OnClientReady;
            Client.MessageCreated += OnMessageCreated;
            Client.ClientErrored += OnClientErrored;
            Client.GuildMemberAdded += OnMemberJoin;
            Client.GuildMemberRemoved += OnMemberLeave;
            Client.MessageReactionAdded += OnReactionAdded;
            Client.MessageReactionRemoved += OnReactionRemoved;
            Client.GuildAvailable += OnGuildAvaliable;
            Client.PresenceUpdated += OnPresenceUpdated;
            Client.GuildCreated += OnGuildJoin;
            Client.GuildUnavailable += OnGuildUnAvaliable;

            var botVersion = typeof(Bot).Assembly.GetName().Version.ToString();

            Log("-----------------------------");
            Log($"Logging Started.");
            Log($"Bot Version: {botVersion}");

            Console.WriteLine($"Current Bot Version: {botVersion}");

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

            Commands.SetHelpFormatter<CustomHelpFormatter>();

            Commands.RegisterCommands<ConfigCommands>();
            Commands.RegisterCommands<GameCommands>();
            Commands.RegisterCommands<ManageCommands>();
            Commands.RegisterCommands<MiscCommands>();
            Commands.RegisterCommands<ModCommands>();
            Commands.RegisterCommands<NowLiveCommands>();
            Commands.RegisterCommands<ProfileCommands>();
            Commands.RegisterCommands<QuoteCommands>();
            Commands.RegisterCommands<ReactionRoleCommands>();
            Commands.RegisterCommands<SetupCommands>();
            Commands.RegisterCommands<StreamerCommands>();
            Commands.RegisterCommands<SuggestionCommands>();
            Log("Command Registered.");

            Commands.CommandErrored += OnCommandErrored;
            Commands.CommandExecuted += OnCommandExecuted;
            Log("Command Events Subscribed.");

            Monitor = new LiveStreamMonitorService(api, 60);
            Log("Twitch Monitor Created.");
            
            Monitor.OnStreamOnline += GGOnStreamOnline;
            Monitor.OnStreamOffline += GGOnStreamOffline;
            Log("Twitch Monitor Events Subscribed.");

            var lst = _guildStreamerConfigService.GetGuildStreamerList();
            Log("Retrieved Streamers to Monitor.");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Monitor.SetChannelsById(lst);
            Console.WriteLine("Connecting to Twitch Service...");
            Log("Connecting to Twitch Service.");

            Monitor.Start();
            if (Monitor.Enabled)
            {
                Console.WriteLine("Connected to Twitch Service!");
                Log("Connected to Twitch Service.");
                Console.WriteLine($"Bot has started monitoring {lst.Count} Channels.");
                Log($"Bot has started monitoring {lst.Count} Channels.");
            }

            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Connecting to Discord Service...");
            Log("Connecting to Discord Service.");
            Console.ResetColor();
            Client.ConnectAsync();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Connected to Discord!");
            Log("Connected to Discord Service.");
            Console.ResetColor();
        }

        private Task OnCommandExecuted(CommandsNextExtension c, CommandExecutionEventArgs e)
        {
            Log($"{e.Context.Member.DisplayName} ran command !{e.Command.Name} in the #{e.Context.Channel.Name} Channel in {e.Context.Guild.Name}");

            return Task.CompletedTask;
        }

        private readonly IReactionRoleService _reactionRoleService;
        private readonly IProfileService _profileService;
        private readonly IExperienceService _experienceService;
        private readonly INitroBoosterRoleConfigService _nitroBoosterRoleConfigService;
        private readonly IWelcomeMessageConfigService _welcomeMessageConfigService;
        private readonly IGuildStreamerConfigService _guildStreamerConfigService;
        private readonly IMessageStoreService _messageStoreService;
        private readonly ICustomCommandService _customCommandService;
        private readonly INowLiveRoleConfigService _nowLiveRoleConfigService;
        private readonly IGoodBotBadBotService _goodBotBadBotService;
        private readonly ICurrencyNameConfigService _currencyNameService;

        private async Task OnHeartbeat(DiscordClient c, HeartbeatEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"Heartbeated. Ping: {e.Ping}ms");
            Console.ResetColor();

            // pull list of streamers in NL feature from DB
            var lst = _guildStreamerConfigService.GetGuildStreamerList();

            // Sets the channels the bot needs to monitor.
            Monitor.SetChannelsById(lst);

            var guildCount = c.Guilds.Count();

            var botStartTime = Process.GetCurrentProcess().StartTime;

            var botUptime = DateTime.Now - botStartTime;

            var currentStatus = c.CurrentUser.Presence.Activity;

            var liveChannels = Monitor.LiveStreams.Count();

            if (currentStatus.Name == null)
            {
                if (guildCount == 1)
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Server!",
                    }, UserStatus.Online);
                }

                else
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Servers!",
                    }, UserStatus.Online);
                }

                return;
            }

            //Servers to Uptime
            if (currentStatus.Name.Contains("Server"))
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Playing,
                    Name = $"Online for: {botUptime.Days}d {botUptime.Hours:00}h {botUptime.Minutes:00}m",
                }, UserStatus.Online);

                return;
            }

            if (currentStatus.Name.Contains("Servers!"))
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Playing,
                    Name = $"Online for: {botUptime.Days}d {botUptime.Hours:00}h {botUptime.Minutes:00}m",
                }, UserStatus.Online);

                return;
            }

            //Uptme to Twitch Channels
            if (currentStatus.Name.Contains("Online for:"))
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.ListeningTo,
                    Name = $"{lst.Count()} Twitch Channels!",
                }, UserStatus.Online);

                return;
            }

            //Twitch Channels to Twitch Streamers
            if (currentStatus.Name.Contains("Twitch Channels!"))
            {
                if (liveChannels == 0)
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{liveChannels} Twitch Streams!",
                    }, UserStatus.Online);
                }

                if (liveChannels == 1)
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{liveChannels} Twitch Stream!",
                    }, UserStatus.Online);
                }

                else
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{liveChannels} Twitch Streams!",
                    }, UserStatus.Online);
                }

                return;
            }

            //Twitch Channels to Servers
            if (currentStatus.Name.Contains("Twitch Stream!"))
            {
                if (guildCount == 1)
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Server!",
                    }, UserStatus.Online);
                }

                else
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Servers!",
                    }, UserStatus.Online);
                }

                return;
            }

            if (currentStatus.Name.Contains("Twitch Streams!"))
            {
                if (guildCount == 1)
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Server!",
                    }, UserStatus.Online);
                }

                else
                {
                    await Client.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Servers!",
                    }, UserStatus.Online);
                }

                return;
            }
        }

        private void GGOnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{e.Stream.UserName} just went Live!");
            Log($"{e.Stream.UserName} just went Live.");
            Log($"Game is: {e.Stream.GameName}");
            Log($"Stream Started at: {e.Stream.StartedAt}");
            Console.ResetColor();
            
            var configs = _guildStreamerConfigService.GetGuildStreamerConfig(e.Stream.UserId);

            foreach (GuildStreamerConfig config in configs)
            {
                Log("Getting Message Store Info");
                var storedMessage = _messageStoreService.GetMessageStore(config.GuildId, config.StreamerId).Result;

                if (storedMessage != null) { continue; }

                Log("Getting Guild");
                DiscordGuild guild = Client.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { continue; }

                Log("Getting Discord Channel to post Now Live");
                DiscordChannel channel = guild.GetChannel(config.AnnounceChannelId);

                Log("Getting Stream Info");
                var stream = api.V5.Streams.GetStreamByUserAsync(e.Stream.UserId).Result;

                Log("Getting Streamer Info");
                var streamer = api.V5.Users.GetUserByIDAsync(e.Stream.UserId).Result;

                if (e.Stream.UserName.Contains("_"))
                {
                    var toReplaceMessage = config.AnnouncementMessage;

                    Log("Replacing _ with \\_");
                    var username = e.Stream.UserName.Replace("_", "\\_");

                    Log($"Username _ replaced to resolve to: {username}");
                    var channelReplace = toReplaceMessage.Replace("%USER%", username);

                    string gameReplace = null;

                    Log("Replacing Game");
                    if (e.Stream.GameName == null) { Log("Replacing Game with Default"); gameReplace = channelReplace.Replace("%GAME%", "A Game"); }
                    if(e.Stream.GameName != null) { Log("Replacing Game with Twitch Game"); gameReplace = channelReplace.Replace("%GAME%", e.Stream.GameName); }
                    

                    Log($"URL is: https://twitch.tv/{e.Stream.UserName}");
                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    Log("Setting Discord Colour");
                    var color = new DiscordColor("9146FF");

                    Log("Creating Embed");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Color = color,
                    };

                    Log("Checking if the Stream Title is null.");
                    if (e.Stream.Title != null) { embed.WithDescription($"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})"); }

                    Log($"Follower Count is: {stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###")}");
                    embed.AddField("Followers:", stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###"), true);

                    Log($"Total Viewers are: {stream.Stream.Channel.Views.ToString("###,###,###,###,###,###")}");
                    embed.AddField("Total Viewers:", stream.Stream.Channel.Views.ToString("###,###,###,###,###,###"), true);

                    var twitchImageURL = $"{stream.Stream.Preview.Large}?={DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}"; 

                    embed.WithThumbnail(streamer.Logo);
                    embed.WithImageUrl(twitchImageURL);

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
                    Log($"Created Message Store for {e.Stream.UserName}");
                }

                else
                {
                    var toReplaceMessage = config.AnnouncementMessage;

                    var username = e.Stream.UserName;

                    var channelReplace = toReplaceMessage.Replace("%USER%", username);

                    var gameReplace = channelReplace.Replace("%GAME%", e.Stream.GameName);

                    Log($"URL is: https://twitch.tv/{e.Stream.UserName}");
                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Color = color,
                    };

                    if (e.Stream.Title != null) { embed.WithDescription($"[{e.Stream.Title}](https://twitch.tv/{streamer.Name})"); }

                    Log($"Follower Count is: {stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###")}");
                    embed.AddField("Followers:", stream.Stream.Channel.Followers.ToString("###,###,###,###,###,###"), true);

                    Log($"Total Viewers are: {stream.Stream.Channel.Views.ToString("###,###,###,###,###,###")}");
                    embed.AddField("Total Viewers:", stream.Stream.Channel.Views.ToString("###,###,###,###,###,###"), true);

                    var twitchImageURL = $"{stream.Stream.Preview.Large}?={DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}";

                    embed.WithThumbnail(streamer.Logo);
                    embed.WithImageUrl(twitchImageURL);
                    embed.WithFooter($"Stream went live at: {e.Stream.StartedAt}", "https://www.iconfinder.com/data/icons/social-messaging-ui-color-shapes-2-free/128/social-twitch-circle-512.png");

                    DiscordMessage sentMessage = channel.SendMessageAsync(announcementMessage, embed: embed).Result;

                    if (guild.Id == 136613758045913088) { Log("Skipped saving Message Store for ProjectExie Server"); return; }

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
                    Log($"Created Message Store for {e.Stream.UserName}");
                }
            }

        }

        private void GGOnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{e.Stream.UserName} just went Offline!");
            Log($"{e.Stream.UserName} just went Offline.");
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
                Log($"Deleted Message Store for {e.Stream.UserName}");
            }
        }

        private async Task OnPresenceUpdated(DiscordClient c, PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot) { return; }

            var configs = _nowLiveRoleConfigService.GetAllConfigs();

            foreach(NowLiveRoleConfig config in configs)
            {
                DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { continue; }

                DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);

                if (member == null) { continue; }

                if (e.User.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
                {
                    DiscordRole NowLive = guild.GetRole(config.RoleId);

                    if (member.Roles.Contains(NowLive)) { continue; }

                    else
                    {
                        await member.GrantRoleAsync(NowLive);
                        Log($"Granted {NowLive.Name} Role to {member.Username} in {guild.Name}.");
                    }

                }

                else
                {
                    DiscordRole NowLive = guild.GetRole(config.RoleId);

                    if (member.Roles.Contains(NowLive))
                    {
                        await member.RevokeRoleAsync(NowLive);
                        Log($"Revoked {NowLive.Name} Role from {member.Username} in {guild.Name}.");
                    }
                }
            }

            return;
        }

        private async Task OnGuildJoin(DiscordClient c, GuildCreateEventArgs e)
        {
            Log($"GG-Bot has been added to the {e.Guild.Name} Discord Server.");

            var members = await e.Guild.GetAllMembersAsync().ConfigureAwait(false);
            var profiles = members.Where(x => x.IsBot == false);

            foreach (DiscordMember profile in profiles)
            {
                if (profile.IsBot)
                {
                    continue;
                }

                await _profileService.GetOrCreateProfileAsync(profile.Id, e.Guild.Id, profile.Username);

                Console.WriteLine($"Profile created for {profile.DisplayName} in {e.Guild.Name}");
                Log($"Profile created for {profile.DisplayName} in {e.Guild.Name}");
            }
        }

        private Task OnGuildUnAvaliable(DiscordClient c, GuildDeleteEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{e.Guild.Name} is now Unavaliable");
            Log($"{e.Guild.Name} is now Unavaliable");
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
                    var storedMessage = await _messageStoreService.GetMessageStore(e.Guild.Id, streamerList);

                    if (storedMessage == null) { continue; }

                    var user = await api.V5.Users.GetUserByIDAsync(streamerList);

                    var isStreaming = await api.V5.Streams.BroadcasterOnlineAsync(user.Id);

                    if (isStreaming == true) { continue; }
                    Console.WriteLine($"{user.DisplayName} is Offline. Deleting Message and Message Store Data.");
                    Log($"{user.DisplayName} is Offline. Deleting Message and Message Store Data.");

                    var messageId = storedMessage.AnnouncementMessageId;

                    var channel = e.Guild.GetChannel(storedMessage.AnnouncementChannelId);

                    var message = channel.GetMessageAsync(messageId).Result;

                    await message.DeleteAsync();

                    await _messageStoreService.RemoveMessageStore(storedMessage);
                    Log("Deleted Stored Message in Message Store");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{e.Guild.Name} is now Avaliable");
                Console.ResetColor();

                DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == e.Guild.Id);

                var config = await _nowLiveRoleConfigService.GetNowLiveRoleConfig(e.Guild.Id);

                if (config == null) { return; }

                var allMembers = await guild.GetAllMembersAsync();

                DiscordRole NowLive = guild.GetRole(config.RoleId);

                var withNowLive = allMembers.Where(x => x.Roles.Contains(NowLive));
                var withoutNowLive = allMembers.Except(withNowLive);

                foreach (DiscordMember withRole in withNowLive)
                {
                    if (withRole.Presence == null)
                    {
                        await withRole.RevokeRoleAsync(NowLive);

                        continue;
                    }

                    if (withRole.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
                    {
                        continue;
                    }

                    else
                    {
                        await withRole.RevokeRoleAsync(NowLive);

                        continue;
                    }
                }

                foreach (DiscordMember withoutRole in withoutNowLive)
                {
                    if (withoutRole.Presence == null) { continue; }

                    if (withoutRole.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
                    {
                        await withoutRole.GrantRoleAsync(NowLive);

                        continue;
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
            Log($"Revoked {role.Name} Role from {member.Username} in {guild.Name}.");

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
            Log($"Granted {role.Name} Role to {member.Username} in {guild.Name}.");

            return;
        }

        private async Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            if(Environment.MachineName == "Haydon-PC")
            {
                await Client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = "the latest test build",
                }, UserStatus.Idle);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{c.CurrentUser.Username} is Ready");
            Log($"{c.CurrentUser.Username} is Ready.");
            Console.ResetColor();
        }

        private async Task OnMessageCreated(DiscordClient c, MessageCreateEventArgs e)
        {
            DiscordEmoji praisedEmote = DiscordEmoji.FromName(c, ":blush:");
            DiscordEmoji scoldedEmote = DiscordEmoji.FromName(c, ":disappointed:");

            if (e.Message.Content.ToLower().Contains("good bot")) { await _goodBotBadBotService.AddGoodBot(); await e.Message.CreateReactionAsync(praisedEmote); Log($"The bot was praised by {e.Author.Username}."); }

            if (e.Message.Content.ToLower().Contains("bad bot")) { await _goodBotBadBotService.AddBadBot(); await e.Message.CreateReactionAsync(scoldedEmote); await e.Channel.SendMessageAsync($"I'm sorry {e.Author.Mention}, I'll try to do better 😞😞"); Log($"The bot was scolded by {e.Author.Username}."); }

            var rnd = new Random();
            var rndGif = rnd.Next(1, 3);

            if (e.Message.Content.ToLower().Contains("covid") && !e.Message.Author.Username.Contains("GG-Bot"))
            {
                if (rndGif == 1)
                {


                    await e.Message.RespondAsync("https://media.giphy.com/media/STfLOU6iRBRunMciZv/giphy.gif").ConfigureAwait(false);
                }

                else if (rndGif == 2)
                {
                    await e.Message.RespondAsync("No... No COVID!\n\nhttps://tenor.com/view/no-consuela-familyguy-gif-7535890").ConfigureAwait(false);
                }
            }

            if (e.Channel.IsPrivate) { return; }

            if (e.Author.IsBot) { return; }

            if (e.Message.Content.Contains("!")) { return; }

            if (e.Guild.Id == 136613758045913088) { return; }

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

                var CNConfig = await _currencyNameService.GetCurrencyNameConfig(e.Guild.Id);

                var currencyName = "Gold";

                if (CNConfig == null) { currencyName = "Gold"; }
                else { currencyName = CNConfig.CurrencyName; }

                var leveledUpEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level:###,###,###,###,###}!",
                    Description = $"{member.DisplayName} has been given {levelUpGold:###,###,###,###,###} {currencyName} for Levelling Up!",
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

                var CNConfig = await _currencyNameService.GetCurrencyNameConfig(e.Guild.Id);

                var currencyName = "Gold";

                if (CNConfig == null) { currencyName = "Gold"; }
                else { currencyName = CNConfig.CurrencyName; }

                var leveledUpEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level:###,###,###,###,###}!",
                    Description = $"{member.DisplayName} has been given {levelUpGold:###,###,###,###,###} {currencyName} for Levelling Up!",
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

                var CNConfig = await _currencyNameService.GetCurrencyNameConfig(e.Guild.Id);

                var currencyName = "Gold";

                if (CNConfig == null) { currencyName = "Gold"; }
                else { currencyName = CNConfig.CurrencyName; }

                var leveledUpEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level:###,###,###,###,###}!",
                    Description = $"{member.DisplayName} has been given {levelUpGold:###,###,###,###,###} {currencyName} for Levelling Up!",
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
            Log(exceptionMessage);

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

        private async Task OnMemberJoin(DiscordClient c, GuildMemberAddEventArgs e)
        {
            Log($"{e.Member.DisplayName} just joined {e.Guild.Name}.");

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
            //Log($"{e.Member.DisplayName} just left {e.Guild.Name}.");


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

        public static void Log(string logItem)
        {
            var applicationName = "";

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment.ToLower() == "development") { applicationName = "GGBotTest"; }
            else if (environment.ToLower() == "live") { applicationName = "GGBot"; }
          
            if (!Directory.Exists($"\\Logs\\{applicationName}\\{DateTime.Now.Year}\\{DateTime.Now.Month}\\"))
            {
                Directory.CreateDirectory($"\\Logs\\{applicationName}\\{ DateTime.Now.Year}\\{ DateTime.Now.Month}\\");
            }

            using StreamWriter w = File.AppendText($"\\Logs\\{applicationName}\\{DateTime.Now.Year}\\{DateTime.Now.Month}\\{DateTime.Today.ToLongDateString()}.txt");

            w.WriteLine($"{DateTime.Now}: {logItem}");

            w.Close();
            w.Dispose();
        }
    }
}
