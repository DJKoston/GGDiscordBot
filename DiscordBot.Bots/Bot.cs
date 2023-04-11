using DiscordBot.Core.Services.Music;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using Microsoft.AspNetCore.Identity;
using Tweetinvi;
using TwitchLib.Communication.Interfaces;


namespace DiscordBot.Bots
{
    public class Bot
    {
        public DiscordClient DiscordClient { get; private set; }
        public CommandsNextExtension DiscordCommands { get; private set; }
        public SlashCommandsExtension DiscordSlashCommands { get; private set; }

        public LavalinkExtension LavaLink { get; private set; }
        public ConnectionEndpoint LavaLinkEndpoint;
        public LavalinkConfiguration LavalinkConfiguration;
        public LavalinkNodeConnection nodeConnection;

        public System.Timers.Timer TwitterTimer = new();

        public TwitterClient TwitterClient { get; private set; }

        private readonly IConfiguration _configuration;
        public string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public LiveStreamMonitorService Monitor;
        public TwitchAPI Twitch;

        public ConsoleColor twitchColor = ConsoleColor.DarkMagenta;
        public ConsoleColor discordColor = ConsoleColor.DarkCyan;
        public ConsoleColor twitterColor = ConsoleColor.Cyan;
        public ConsoleColor fail = ConsoleColor.Red;
        public int statusPosition = 0;
        public int messageDeletionStatus = 0;

        public Bot(IServiceProvider services, IConfiguration configuration)
        {
            _configuration = configuration;

            var botVersion = typeof(Bot).Assembly.GetName().Version.ToString();
            Log("-----------------------------");
            Log("Logging Started.");
            Log($"Bot Version: {botVersion}");

            //Load Services
            Log("Loading Core Services...");
            _buttonRoleService = services.GetService<IButtonRoleService>();
            _currencyNameConfigService = services.GetService<ICurrencyNameConfigService>();
            _doubleXPRoleConfigService = services.GetService<IDoubleXPRoleConfigService>();
            _nowLiveRoleConfigService = services.GetService<INowLiveRoleConfigService>();
            _leaveMessageConfigService = services.GetService<ILeaveMessageConfigService>();
            _welcomeMessageConfigService = services.GetService<IWelcomeMessageConfigService>();
            _goodBotBadBotService = services.GetService<IGoodBotBadBotService>();
            _customCommandService = services.GetService<ICustomCommandService>();
            _musicService = services.GetService<IMusicService>();
            _nowLiveMessageService = services.GetService<INowLiveMessageService>();
            _nowLiveStreamerService =  services.GetService<INowLiveStreamerService>();
            _profileService = services.GetService<IProfileService>();
            _xpService = services.GetService<IXPService>();
            _reactionRoleService = services.GetService<IReactionRoleService>();
            _xpToggleService = services.GetService<IXPToggleService>();
            _twitterService = services.GetService<ITwitterService>();
            Log("Loaded Core Services.");

            //Get Configuration Information from appsettings.json
            Log("Getting Configuration Info...");
            var discordToken = configuration["discord-token"];
            var discordPrefix = configuration["discord-prefix"];
            var twitchClientId = configuration["twitch-clientid"];
            var twitchAccessToken = configuration["twitch-accesstoken"];
            var lavalinkPassword = configuration["lavalink-password"];
            var twitterBearerToken = configuration["twitter-bearer"];
            var twitterAPIKey = configuration["twitter-apikey"];
            var twitterAPIToken = configuration["twitter-apitoken"];
            var twitterAccessToken = configuration["twitter-accesstoken"];
            var twitterAccessTokenSecret = configuration["twitter-accesssecret"];
            Log("Configuration Info Retreived.");

            //Twitch Connection
            Log("Creating Twitch API Access...");
            Twitch = new TwitchAPI();

            Twitch.Settings.ClientId = twitchClientId;
            Twitch.Settings.AccessToken = twitchAccessToken;
            Log("Twitch API Access Created.");

            Log("Creating Twitter API Access...", twitterColor);
            TwitterClient = new TwitterClient(twitterAPIKey, twitterAPIToken, twitterBearerToken);
            Log("Created Twitter API Access.", twitterColor);

            Log("Creating Twitter Timer...");
            TwitterTimer.Interval = 10000;
            TwitterTimer.AutoReset = true;
            TwitterTimer.Elapsed += TwitterTimer_Elapsed;
            TwitterTimer.Start();
            Log("Twitter Timer Started.");

            //Discord Connection
            Log("Creating Discord Bot Configuration...");
            var discordConfig = new DiscordConfiguration()
            {
                Token = discordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Information,
                Intents = DiscordIntents.All,
            };
            Log("Discord Bot Configuration Created.");

            Log("Creating CommandsNext Config...");
            var discordCommandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { discordPrefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = services,
            };
            Log("CommandsNext Config Created.");

            Log("Creating SlashCommands Config...");
            var discordSlashCommandsConfig = new SlashCommandsConfiguration
            {
                Services = services,
            };
            Log("SlashCommands Config Created.");

            Log("Creating New Discord Client...");
            DiscordClient = new DiscordClient(discordConfig);
            DiscordCommands = DiscordClient.UseCommandsNext(discordCommandsConfig);
            DiscordCommands.SetHelpFormatter<CustomHelpFormatter>();
            DiscordSlashCommands = DiscordClient.UseSlashCommands(discordSlashCommandsConfig);
            Log("New Discord Client Created.");

            Log("Registering Discord Client Events...");
            DiscordClient.Heartbeated += DiscordHeartbeat;
            DiscordClient.Ready += DiscordClientReady;
            DiscordClient.MessageCreated += DiscordMessageCreated;
            DiscordClient.ClientErrored += DiscordClientErrored;
            DiscordClient.GuildMemberAdded += DiscordGuildMemberAdded;
            DiscordClient.GuildMemberRemoved += DiscordGuildMemberRemoved;
            DiscordClient.MessageReactionAdded += DiscordMessageReactionAdded;
            DiscordClient.MessageReactionRemoved += DiscordMessageReactionRemoved;
            DiscordClient.GuildAvailable += DiscordGuildAvailable;
            DiscordClient.PresenceUpdated += DiscordPresenceUpdated;
            DiscordClient.GuildCreated += DiscordGuildCreated;
            DiscordClient.GuildUnavailable += DiscordGuildUnavaliable;
            DiscordClient.ComponentInteractionCreated += DiscordComponentInteraction;
            Log("Discord Client Events Registered.");

            Log("Registering Discord Commands...");
            DiscordCommands.RegisterCommands<ButtonRoleCommands>();
            DiscordCommands.RegisterCommands<ConfigCommands>();
            DiscordCommands.RegisterCommands<CustomCommands>();
            DiscordCommands.RegisterCommands<GameCommands>();
            DiscordCommands.RegisterCommands<GitHubCommands>();
            DiscordCommands.RegisterCommands<MiscCommands>();
            DiscordCommands.RegisterCommands<ModCommands>();
            DiscordCommands.RegisterCommands<NowLiveCommands>();
            DiscordCommands.RegisterCommands<PollCommands>();
            DiscordCommands.RegisterCommands<ProfileCommands>();
            DiscordCommands.RegisterCommands<QuoteCommands>();
            DiscordCommands.RegisterCommands<ReactionRoleCommands>();
            DiscordCommands.RegisterCommands<SuggestionCommands>();
            DiscordCommands.RegisterCommands<TwitterCommands>();
            Log("Discord Commands Registered.");

            Log("Registering Discord Client Interactivity...");
            DiscordClient.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5),
            });
            Log("Discord Client Interactivity Registered.");

            Log("Registering Discord Command Events...");
            DiscordCommands.CommandErrored += OnDiscordCommandErrored;
            DiscordCommands.CommandExecuted += OnDiscordCommandExecuted;
            Log("Discord Command Events Registered.");

            Log("Registering Discord SlashCommand Events...");
            DiscordSlashCommands.SlashCommandExecuted += OnDiscordSlashCommandExecuted;
            Log("Discord SlashCommand Events Registered.");

            //Twitch Livestream Monitor
            Log("Creating Twitch Live Monitor...");
            Monitor = new LiveStreamMonitorService(Twitch, 60);
            Log("New Twitch Monitor Created.");

            Log("Registering Twitch Monitor Events...");
            Monitor.OnStreamOnline += OnTwitchStreamOnline;
            Monitor.OnStreamOffline += OnTwitchStreamOffline;
            Log("Twitch Monitor Events Registered.");
            
            Log("Retrieving Now Live Streamer List...");
            var lst = _nowLiveStreamerService.GetNowLiveStreamerList();
            Log("Now Live Streamer List Retrieved.");

            if(lst.Count != 0)
            {
                Log("Setting Twitch Monitor Channels...");
                Monitor.SetChannelsById(lst);
                Log("Twitch Monitor Channels Set.");

                Log("Starting Twitch Monitor...", twitchColor);
                Monitor.Start();

                if (Monitor.Enabled)
                {
                    Log("Twitch Monitor Started.", twitchColor);
                    Log($"Bot has started monitoring {lst.Count} Twitch Channels.", twitchColor);
                }
                else
                {
                    Log("Twitch Monitor failed to start.", fail);
                }
            }

            else
            {
                Log("No Channels have been submitted to monitor.", fail);
            }

            Log("Creating Lavalink Endpoints...");
            LavaLinkEndpoint = new ConnectionEndpoint
            {
                Hostname = _configuration["lavalink-server"],
                Port = 2333,
            };

            LavalinkConfiguration = new LavalinkConfiguration
            {
                Password = _configuration["lavalink-password"],
                RestEndpoint = LavaLinkEndpoint,
                SocketEndpoint = LavaLinkEndpoint,
            };
            Log("Lavalink Endpoints Created.");

            Log("Connecting to Discord...", discordColor);
            DiscordClient.ConnectAsync();
            Log("Connected to Discord.", discordColor);

            Log("Registering Slash Commands...");
            DiscordSlashCommands.RegisterCommands<GameSlashCommands>();
            DiscordSlashCommands.RegisterCommands<MiscSlashCommands>();
            DiscordSlashCommands.RegisterCommands<ProfileSlashCommands>();
            DiscordSlashCommands.RegisterCommands<MusicSlashCommands>();
            Log("Slash Commands Registered.");
        }

        public string currencyName;

        private readonly IButtonRoleService _buttonRoleService;
        private readonly ICurrencyNameConfigService _currencyNameConfigService;
        private readonly IDoubleXPRoleConfigService _doubleXPRoleConfigService;
        private readonly INowLiveRoleConfigService _nowLiveRoleConfigService;
        private readonly ILeaveMessageConfigService _leaveMessageConfigService;
        private readonly IWelcomeMessageConfigService _welcomeMessageConfigService;
        private readonly IGoodBotBadBotService _goodBotBadBotService;
        private readonly ICustomCommandService _customCommandService;
        private readonly IMusicService _musicService;
        private readonly INowLiveMessageService _nowLiveMessageService;
        private readonly INowLiveStreamerService _nowLiveStreamerService;
        private readonly IProfileService _profileService;
        private readonly IXPService _xpService;
        private readonly IReactionRoleService _reactionRoleService;
        private readonly IXPToggleService _xpToggleService;
        private readonly ITwitterService _twitterService;

        private async void TwitterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var users = _twitterService.GetAllMonitoredAccounts();

            foreach (var user in users)
            {
                var monitors = _twitterService.GetMonitorGuildInfo(user);

                foreach(var monitor in monitors)
                {
                    var tweetSearch = await TwitterClient.Search.SearchTweetsAsync($"from:{user} -is:retweet -filter:replies");
                    var firstTweet = tweetSearch.FirstOrDefault();

                    long firstTweetId = Int64.Parse(firstTweet.Id.ToString());
                    long monitorId = 0;

                    if (monitor.LastTweetLink == null) { monitorId = 0; }
                    else
                    {
                        monitorId = Int64.Parse(monitor.LastTweetLink);
                    }

                    if (monitorId >= firstTweetId) { continue; }

                    var guild = DiscordClient.Guilds.Values.FirstOrDefault(x => x.Id == monitor.GuildID);

                    if (guild == null) { continue; }

                    var channel = guild.GetChannel(monitor.ChannelID);

                    if (channel == null) { continue; }

                    await channel.SendMessageAsync(firstTweet.Url);
                    Log($"New Tweet from: {user}. Posted to: {guild.Name}", twitterColor);

                    await _twitterService.UpdateTweetLinkAsync(monitor, firstTweet.Id.ToString());
                }
            }
        }

        private async Task NodeConnection_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            var nextSong = await _musicService.GetNextSongAsync(sender.Guild.Id);

            if (nextSong == null)
            {
                await sender.DisconnectAsync();

                return;
            }

            LavalinkLoadResult loadResult;

            if (nextSong.SongURI.StartsWith("https://") || nextSong.SongURI.StartsWith("http://"))
            {
                var songUri = new Uri(nextSong.SongURI);

                loadResult = await nodeConnection.Rest.GetTracksAsync(songUri);
            }

            else
            {
                loadResult = await nodeConnection.Rest.GetTracksAsync(nextSong.SongURI);
            }

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await _musicService.RemoveNextSongAsync(sender.Guild.Id);

                await NodeConnection_PlaybackFinished(sender, e);
                return;
            }

            var track = loadResult.Tracks.First();

            await sender.PlayAsync(track);
            await _musicService.RemoveNextSongAsync(sender.Guild.Id);
        }

        private async Task NodeConnection_TrackStuck(LavalinkGuildConnection sender, TrackStuckEventArgs e)
        {
            if(e.Track.Title == "Your #1 Simulation Station")
            {
                var songUri = new Uri("http://stream.simulatorradio.com/stream.mp3");

                var loadResult = await nodeConnection.Rest.GetTracksAsync(songUri);

                var track = loadResult.Tracks.First();

                await sender.SetVolumeAsync(10);
                await sender.PlayAsync(track);
            }
        }

        private async Task DiscordComponentInteraction(DiscordClient c, ComponentInteractionCreateEventArgs e)
        {
            var button = await _buttonRoleService.GetButtonRole(e.Guild.Id, e.Id);

            var role = e.Guild.GetRole(button.RoleId);
            var member = await e.Guild.GetMemberAsync(e.User.Id);

            var memberRoles = member.Roles;

            if ((memberRoles.Contains(role) && (button.GiveRemove == "give" || button.GiveRemove == "add")) || (!memberRoles.Contains(role) && button.GiveRemove == "remove"))
            {
                await member.RevokeRoleAsync(role);

                var responseBuilder = new DiscordInteractionResponseBuilder
                {
                    Content = $"You no longer have the {role.Mention} Role!",
                    IsEphemeral = true,
                };

                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
            }

            else if ((!memberRoles.Contains(role) && (button.GiveRemove == "give" || button.GiveRemove == "add")) || (memberRoles.Contains(role) && button.GiveRemove == "remove"))
            {
                await member.GrantRoleAsync(role);

                var responseBuilder = new DiscordInteractionResponseBuilder
                {
                    Content = $"You now have the {role.Mention} Role!",
                    IsEphemeral = true,
                };

                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
            }
        }

        private void OnTwitchStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Log($"{e.Stream.UserName} just went Offline.", twitchColor);

            var configs = _nowLiveStreamerService.GetNowLiveStreamer(e.Stream.UserId);

            foreach (NowLiveStreamer config in configs)
            {
                DiscordGuild guild = DiscordClient.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { continue; }

                var storedMessage = _nowLiveMessageService.GetMessageStore(config.GuildId, e.Stream.UserId).Result;

                var messageId = storedMessage.AnnouncementMessageId;

                var channel = guild.GetChannel(storedMessage.AnnouncementChannelId);

                DiscordMessage message = channel.GetMessageAsync(messageId).Result;

                if (message == null) { continue; }

                message.DeleteAsync();

                _nowLiveMessageService.RemoveMessageStore(storedMessage);

                Log($"Deleted Message Store for {e.Stream.UserName}");
            }
        }

        private void OnTwitchStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            List<string> id = new()
            {
                e.Channel
            };
            
            var followers = Twitch.Helix.Users.GetUsersFollowsAsync(toId: e.Channel).Result.TotalFollows.ToString("###,###,###,###,###,###");
            var userLogo = Twitch.Helix.Users.GetUsersAsync(id).Result.Users.FirstOrDefault(x => x.DisplayName.ToLower() == e.Stream.UserName.ToLower()).ProfileImageUrl;
            var twitchLogo = "https://www.freepnglogos.com/uploads/purple-twitch-logo-png-18.png";

            TimeSpan t = e.Stream.StartedAt - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            var tUrl1 = e.Stream.ThumbnailUrl;
            var tUrl2 = tUrl1.Replace("{width}", "1280");
            var thumbnailUrl = tUrl2.Replace("{height}", "720");

            var twitchImageURL = $"{thumbnailUrl}?={DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}";

            Log($"{e.Stream.UserName} just went Live!", twitchColor);

            var configs = _nowLiveStreamerService.GetNowLiveStreamer(e.Stream.UserId);

            foreach (NowLiveStreamer config in configs)
            {
                var storedMessage = _nowLiveMessageService.GetMessageStore(config.GuildId, config.StreamerId).Result;

                if (storedMessage != null) { continue; }

                DiscordGuild guild = DiscordClient.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { continue; }

                DiscordChannel channel = guild.GetChannel(config.AnnounceChannelId);

                if (e.Stream.UserName.Contains('_'))
                {
                    var toReplaceMessage = config.AnnouncementMessage;

                    var username = e.Stream.UserName.Replace("_", "\\_");

                    var channelReplace = toReplaceMessage.Replace("%USER%", username);

                    string gameReplace = null;

                    if (e.Stream.GameName == null) { Log("Replacing Game with Default"); gameReplace = channelReplace.Replace("%GAME%", "A Game"); }
                    if (e.Stream.GameName != null) { Log("Replacing Game with Twitch Game"); gameReplace = channelReplace.Replace("%GAME%", e.Stream.GameName); }

                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Color = color,
                        ImageUrl = twitchImageURL,
                    };

                    if (e.Stream.Title != null) { embed.WithDescription($"[{e.Stream.Title}](https://twitch.tv/{e.Stream.UserName})"); }

                    embed.AddField("Followers:", followers, true);
                    embed.AddField("Went Live:", $"<t:{secondsSinceEpoch}:R>", true);
                    embed.WithThumbnail(userLogo);
                    embed.WithFooter($"Stream went live at: {e.Stream.StartedAt} UTC", twitchLogo);

                    DiscordMessage sentMessage = channel.SendMessageAsync(announcementMessage, embed: embed).Result;

                    var messageStore = new NowLiveMessage
                    {
                        GuildId = config.GuildId,
                        StreamerId = config.StreamerId,
                        AnnouncementChannelId = channel.Id,
                        AnnouncementMessageId = sentMessage.Id,
                        StreamTitle = e.Stream.Title,
                        StreamGame = e.Stream.GameName,
                    };

                    _nowLiveMessageService.CreateNewMessageStore(messageStore);
                    Log($"Created Message Store for {e.Stream.UserName}");
                }

                else
                {
                    var toReplaceMessage = config.AnnouncementMessage;

                    var username = e.Stream.UserName;

                    var channelReplace = toReplaceMessage.Replace("%USER%", username);

                    string gameReplace = null;

                    if (e.Stream.GameName == null) { Log("Replacing Game with Default"); gameReplace = channelReplace.Replace("%GAME%", "A Game"); }
                    if (e.Stream.GameName != null) { Log("Replacing Game with Twitch Game"); gameReplace = channelReplace.Replace("%GAME%", e.Stream.GameName); }

                    var announcementMessage = gameReplace.Replace("%URL%", $"https://twitch.tv/{e.Stream.UserName} ");

                    var color = new DiscordColor("9146FF");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Stream.UserName} has gone live!",
                        Color = color,
                        ImageUrl = twitchImageURL
                    };

                    if (e.Stream.Title != null) { embed.WithDescription($"[{e.Stream.Title}](https://twitch.tv/{e.Stream.UserName})"); }

                    embed.AddField("Followers:", followers, true);
                    embed.AddField("Went Live:", $"<t:{secondsSinceEpoch}:R>", true);
                    embed.WithThumbnail(userLogo);
                    embed.WithFooter($"Stream went live at: {e.Stream.StartedAt} UTC", twitchLogo);

                    DiscordMessage sentMessage = channel.SendMessageAsync(announcementMessage, embed: embed).Result;

                    var messageStore = new NowLiveMessage
                    {
                        GuildId = config.GuildId,
                        StreamerId = config.StreamerId,
                        AnnouncementChannelId = channel.Id,
                        AnnouncementMessageId = sentMessage.Id,
                        StreamTitle = e.Stream.Title,
                        StreamGame = e.Stream.GameName,
                    };

                    _nowLiveMessageService.CreateNewMessageStore(messageStore);
                    Log($"Created Message Store for {e.Stream.UserName}");
                }
            }
        }

        private Task OnDiscordSlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            Log($"{e.Context.Member.DisplayName} ran Slash Command \"{e.Context.CommandName}\" in {e.Context.Guild.Name} #{e.Context.Channel.Name}");

            return Task.CompletedTask;
        }

        private Task OnDiscordCommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            Log($"{e.Context.Member.DisplayName} ran Command !{e.Command.Name} in {e.Context.Guild.Name} #{e.Context.Channel.Name}");

            return Task.CompletedTask;
        }

        private async Task OnDiscordCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception.Message is "Specified command was not found.")
            {
                var command = await _customCommandService.GetCommandAsync(e.Context.Message.Content.ToString(), e.Context.Guild.Id);

                if (command == null)
                {
                    if(e.Context.Message.Content == "!cancel")
                    {
                        return;
                    }

                    Log($"{e.Context.Member.DisplayName} attempted to run {e.Context.Message.Content} in {e.Context.Guild.Name} #{e.Context.Channel.Name} - Exception: {e.Exception.Message}");

                    return;
                }

                await e.Context.Channel.SendMessageAsync(command.Action);
                Log($"{e.Context.Member.DisplayName} ran Command !{command.Trigger} in {e.Context.Guild.Name} #{e.Context.Channel.Name}");
            }

            else if (e.Exception is ChecksFailedException exception)
            {
                if (e.Exception is ChecksFailedException)
                {
                    var properError = exception;

                    if (e.Exception is CommandNotFoundException)
                    {
                        var command = await _customCommandService.GetCommandAsync(e.Context.Message.Content.ToString(), e.Context.Guild.Id);

                        if (command == null)
                        {
                            Log($"{e.Context.Member.DisplayName} attempted to run {e.Context.Message.Content} in {e.Context.Guild.Name} #{e.Context.Channel.Name} - Exception: Command doesn't exist.");
                            return;
                        }

                        await e.Context.Channel.SendMessageAsync(command.Action);
                        Log($"{e.Context.Member.DisplayName} ran Command {e.Context.Message.Content} in {e.Context.Guild.Name} #{e.Context.Channel.Name}");
                    }

                    else if (properError.FailedChecks[0] is RequireRolesAttribute)
                    {
                        var permissionEmbed = new DiscordEmbedBuilder
                        {
                            Title = "You do not have permission to run this command!",
                            Description = "Seek for those permissions or just stop being a jerk trying to use command's you're not allowed to touch!",
                            Color = DiscordColor.IndianRed,
                        };

                        await e.Context.Channel.SendMessageAsync(embed: permissionEmbed);
                        Log($"{e.Context.Member.DisplayName} attempted to run {e.Context.Message.Content} in {e.Context.Guild.Name} #{e.Context.Channel.Name} - Exception: User has no permission.");
                    }

                    else if (properError.FailedChecks[0] is CooldownAttribute attribute)
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
                            Log($"{e.Context.Member.DisplayName} attempted to run {e.Context.Message.Content} in {e.Context.Guild.Name} #{e.Context.Channel.Name} - Exception: Command on Cooldown");
                        }

                        else if (!cooldownMins.Equals(0))
                        {
                            var cooldownEmbed = new DiscordEmbedBuilder
                            {
                                Title = "This command is still on cooldown!",
                                Description = $"Please try again in {cooldownMins} Minutes, {cooldownSeconds} Seconds!",
                                Color = DiscordColor.IndianRed,
                            };

                            await e.Context.Channel.SendMessageAsync(embed: cooldownEmbed);
                            Log($"{e.Context.Member.DisplayName} attempted to run {e.Context.Message.Content} in {e.Context.Guild.Name} #{e.Context.Channel.Name} - Exception: Command on Cooldown");
                        }

                        else if (cooldownMins.Equals(0) && cooldownHours.Equals(0))
                        {
                            var cooldownEmbed = new DiscordEmbedBuilder
                            {
                                Title = "This command is still on cooldown!",
                                Description = $"Please try again in {cooldownSeconds} seconds!",
                                Color = DiscordColor.IndianRed,
                            };

                            await e.Context.Channel.SendMessageAsync(embed: cooldownEmbed);
                            Log($"{e.Context.Member.DisplayName} attempted to run !{e.Command.Name} in {e.Context.Guild.Name} #{e.Context.Channel.Name} - Exception: Command on Cooldown");

                        }
                    }
                }
            }
        }

        private Task DiscordGuildUnavaliable(DiscordClient c, GuildDeleteEventArgs e)
        {
            Log($"{e.Guild.Name} is now Unavaliable", fail);

            return Task.CompletedTask;
        }

        private async Task DiscordGuildCreated(DiscordClient c, GuildCreateEventArgs e)
        {
            Log($"GG-Bot has been added to the '{e.Guild.Name}' Discord Server.");

            var members = await e.Guild.GetAllMembersAsync();
            var profiles = members.Where(x => x.IsBot == false);

            foreach (DiscordMember profile in profiles)
            {
                if (!profile.IsBot)
                {
                    await _profileService.GetOrCreateProfileAsync(profile.Id, e.Guild.Id, profile.Username);

                    Log($"New Profile created for '{profile.DisplayName}' in '{e.Guild.Name}'");
                }
            }
        }

        private async Task DiscordPresenceUpdated(DiscordClient c, PresenceUpdateEventArgs e)
        {
            var configs = _nowLiveRoleConfigService.GetAllNowLiveRoles();

            foreach (NowLiveRoleConfig config in configs)
            {
                if (e.User.IsBot) { continue; }

                DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == config.GuildId);

                if (guild == null) { continue; }

                DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);

                if (member == null) { continue; }

                if (e.User.Presence.Activities.Any(x => x.ActivityType.Equals(ActivityType.Streaming)))
                {
                    DiscordRole NowLive = guild.GetRole(config.RoleId);

                    if (!member.Roles.Contains(NowLive)) 
                    {
                        await member.GrantRoleAsync(NowLive);
                        Log($"Granted {NowLive.Name} Role to {member.Username} in {guild.Name} through Now Live Role.");
                    }
                }

                else
                {
                    DiscordRole NowLive = guild.GetRole(config.RoleId);

                    if (member.Roles.Contains(NowLive))
                    {
                        await member.RevokeRoleAsync(NowLive);
                        Log($"Revoked {NowLive.Name} Role from {member.Username} in {guild.Name} through Now Live Role.");
                    }
                }
            }

            return;
        }

        private async Task DiscordGuildAvailable(DiscordClient c, GuildCreateEventArgs e)
        {
            Log($"'{e.Guild.Name}' is now Available");
            DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == e.Guild.Id);

            var config = await _nowLiveRoleConfigService.GetNowLiveRole(e.Guild.Id);

            if (config != null)
            {
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
            }
        }

        private async Task DiscordMessageReactionRemoved(DiscordClient c, MessageReactionRemoveEventArgs e)
        {
            if (!e.User.IsBot)
            {
                var reactionRole = _reactionRoleService.GetReactionRole(e.Guild.Id, e.Channel.Id, e.Message.Id, e.Emoji.Id, e.Emoji.Name).Result;

                if (reactionRole == null) { return; }

                DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == reactionRole.GuildId);
                DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
                DiscordRole role = guild.GetRole(reactionRole.RoleId);

                if (reactionRole.RemoveAddRole == "add")
                {
                    await member.RevokeRoleAsync(role);
                    Log($"Revoked {role.Name} Role from {member.Username} in {guild.Name} through Reaction Role.");
                }

                else if (reactionRole.RemoveAddRole == "remove")
                {
                    await member.GrantRoleAsync(role);
                    Log($"Granted {role.Name} Role to {member.Username} in {guild.Name} through Reaction Role.");
                }
            }
        }

        private async Task DiscordMessageReactionAdded(DiscordClient c, MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot)
            {
                DiscordEmoji emoji = e.Emoji;

                var reactionRole = await _reactionRoleService.GetReactionRole(e.Guild.Id, e.Channel.Id, e.Message.Id, e.Emoji.Id, e.Emoji.Name);

                if (reactionRole == null) { return; }

                DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == reactionRole.GuildId);
                DiscordMember member = guild.Members.Values.FirstOrDefault(x => x.Id == e.User.Id);
                DiscordRole role = guild.GetRole(reactionRole.RoleId);

                if (reactionRole.RemoveAddRole == "add")
                {
                    await member.GrantRoleAsync(role);
                    Log($"Granted {role.Name} Role to {member.Username} in {guild.Name} through Reaction Role.");
                }

                else if (reactionRole.RemoveAddRole == "remove")
                {
                    await member.RevokeRoleAsync(role);
                    Log($"Revoked {role.Name} Role from {member.Username} in {guild.Name} through Reaction Role.");
                }
            }
        }

        private async Task DiscordGuildMemberRemoved(DiscordClient c, GuildMemberRemoveEventArgs e)
        {
            Log($"{e.Member.DisplayName} just left {e.Guild.Name}.");

            var WMConfig = _leaveMessageConfigService.GetLeaveMessageConfig(e.Guild.Id).Result;

            if (WMConfig != null)
            {
                DiscordChannel welcome = e.Guild.GetChannel(WMConfig.ChannelId);

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

        private async Task DiscordGuildMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            Log($"{e.Member.DisplayName} just joined {e.Guild.Name}.");

            var WMConfig = _welcomeMessageConfigService.GetWelcomeMessage(e.Guild.Id).Result;

            if (WMConfig != null)
            {
                DiscordChannel welcome = e.Guild.GetChannel(WMConfig.ChannelId);

                if (!e.Member.IsBot)
                {
                    await _profileService.GetOrCreateProfileAsync(e.Member.Id, e.Guild.Id, e.Member.Username);
                }

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

        private Task DiscordClientErrored(DiscordClient c, ClientErrorEventArgs e)
        {
            Log(e.Exception.Message, fail);

            return Task.CompletedTask;
        }

        private async Task DiscordMessageCreated(DiscordClient c, MessageCreateEventArgs e)
        {
            if (e.Channel.Type.ToString() == "News")
            {
                await e.Channel.CrosspostMessageAsync(e.Message);
            }

            DiscordEmoji praisedEmote = DiscordEmoji.FromName(c, ":blush:");
            DiscordEmoji scoldedEmote = DiscordEmoji.FromName(c, ":disappointed:");

            if (e.Message.Content.ToLower().Contains("good bot")) { await _goodBotBadBotService.AddGoodBot(); await e.Message.CreateReactionAsync(praisedEmote); Log($"The bot was praised by {e.Author.Username}."); }

            if (e.Message.Content.ToLower().Contains("bad bot")) { await _goodBotBadBotService.AddBadBot(); await e.Message.CreateReactionAsync(scoldedEmote); await e.Channel.SendMessageAsync($"I'm sorry {e.Author.Mention}, I'll try to do better 😞😞"); Log($"The bot was scolded by {e.Author.Username}."); }

            if (e.Channel.IsPrivate) { return; }

            if (e.Author.IsBot) { return; }

            if (e.Message.Content.StartsWith('!')) { return; }

            DiscordGuild guild = c.Guilds.Values.FirstOrDefault(x => x.Id == e.Guild.Id);
            DiscordMember memberCheck = await guild.GetMemberAsync(e.Author.Id);

            var NBConfig = _doubleXPRoleConfigService.GetDoubleXPRole(e.Guild.Id).Result;
            var xpStatus = _xpToggleService.GetGuildConfig(e.Guild.Id).Result;

            if(xpStatus == null || xpStatus.Status == "enabled")
            {
                if (NBConfig == null)
                {
                    var member = e.Guild.Members[e.Author.Id];

                    var randomNumber = new Random();

                    int randXP = randomNumber.Next(50);

                    GrantXpViewModel viewModel = await _xpService.GrantXpAsync(e.Author.Id, e.Guild.Id, randXP, e.Author.Username);

                    if (!viewModel.LevelledUp) { return; }

                    Profile profile = await _profileService.GetOrCreateProfileAsync(e.Author.Id, e.Guild.Id, e.Author.Username);

                    int levelUpGold = (profile.Level * 100);

                    var CNConfig = await _currencyNameConfigService.GetCurrencyNameConfig(e.Guild.Id);

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

                    await e.Channel.SendMessageAsync(embed: leveledUpEmbed);

                    return;
                }

                DiscordRole NitroBooster = guild.GetRole(NBConfig.RoleId);

                if (memberCheck.Roles.Contains(NitroBooster))
                {
                    var member = e.Guild.Members[e.Author.Id];

                    var randomNumber = new Random();

                    int randXP = randomNumber.Next(50);

                    int NitroXP = randXP * 2;

                    GrantXpViewModel viewModel = await _xpService.GrantXpAsync(e.Author.Id, e.Guild.Id, NitroXP, e.Author.Username);

                    if (!viewModel.LevelledUp) { return; }

                    Profile profile = await _profileService.GetOrCreateProfileAsync(e.Author.Id, e.Guild.Id, e.Author.Username);

                    int levelUpGold = profile.Level * 100;

                    var CNConfig = await _currencyNameConfigService.GetCurrencyNameConfig(e.Guild.Id);

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

                    await e.Channel.SendMessageAsync(embed: leveledUpEmbed);

                    return;
                }

                else
                {
                    var member = e.Guild.Members[e.Author.Id];

                    var randomNumber = new Random();

                    int randXP = randomNumber.Next(50);

                    GrantXpViewModel viewModel = await _xpService.GrantXpAsync(e.Author.Id, e.Guild.Id, randXP, e.Author.Username);

                    if (!viewModel.LevelledUp) { return; }

                    Profile profile = await _profileService.GetOrCreateProfileAsync(e.Author.Id, e.Guild.Id, e.Author.Username);

                    int levelUpGold = (profile.Level * 100);

                    var CNConfig = await _currencyNameConfigService.GetCurrencyNameConfig(e.Guild.Id);

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

                    await e.Channel.SendMessageAsync(embed: leveledUpEmbed);

                    return;
                }
            }

            if(xpStatus.Status == "disabled")
            {
                return;
            }
        }

        private async Task DiscordClientReady(DiscordClient c, ReadyEventArgs e)
        {
            Log($"{c.CurrentUser.Username} is Ready.", ConsoleColor.Green);
            
            if(environmentName != "Development")
            {
                Log("Connecting to Lavalink...");
                LavaLink = DiscordClient.UseLavalink();
                await LavaLink.ConnectAsync(LavalinkConfiguration);
                nodeConnection = LavaLink.GetNodeConnection(LavaLinkEndpoint);
                nodeConnection.PlaybackFinished += NodeConnection_PlaybackFinished;
                nodeConnection.TrackStuck += NodeConnection_TrackStuck;
                await _musicService.RemoveAllSongsAsync();
                Log("Connected to Lavalink.");
            }
        }

        private async Task DiscordHeartbeat(DiscordClient c, HeartbeatEventArgs e)
        {
            Heartbeat($"Ping: {e.Ping}ms");
            
            if (messageDeletionStatus == 1)
            {
                new Thread(async () =>
                {
                    Log($"Deleting Now Live Messages", twitchColor);

                    var messages = _nowLiveMessageService.GetAllMessages();

                    foreach (NowLiveMessage message in messages)
                    {
                        DiscordGuild guild = DiscordClient.Guilds.Values.FirstOrDefault(x => x.Id == message.GuildId);

                        if (guild == null) { continue; }

                        var storedMessage = _nowLiveMessageService.GetMessageStore(message.GuildId, message.StreamerId).Result;

                        if (storedMessage == null) { continue; }

                        else
                        {
                            var messageId = storedMessage.AnnouncementMessageId;

                            var channel = guild.GetChannel(storedMessage.AnnouncementChannelId);

                            DiscordMessage discordMessage = channel.GetMessageAsync(messageId).Result;

                            if (discordMessage == null) { continue; }

                            await discordMessage.DeleteAsync();

                            await _nowLiveMessageService.RemoveMessageStore(storedMessage);
                        }
                    }
                    messageDeletionStatus = 2;

                    Log($"Now Live Messages Deleted.", twitchColor);
                }).Start();
            }

            if (messageDeletionStatus == 0) { messageDeletionStatus = 1; }
            
            var lst = _nowLiveStreamerService.GetNowLiveStreamerList();

            if(lst.Count != 0)
            {
                Monitor.SetChannelsById(lst);

                if(!Monitor.Enabled)
                {
                    Monitor.Start();
                }
            }

            var guildCount = c.Guilds.Count;

            var botStartTime = Process.GetCurrentProcess().StartTime;

            var botUptime = DateTime.Now - botStartTime;

            var liveChannels = Monitor.LiveStreams.Count;

            if (statusPosition == 0)
            {
                await DiscordClient.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = "myself start up.",
                }, UserStatus.DoNotDisturb);
                
                statusPosition++;

                return;
            }

            if (statusPosition == 1)
            {
                if (guildCount == 1)
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Server!",
                    }, UserStatus.Online);
                }

                else
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Servers!",
                    }, UserStatus.Online);
                }

                statusPosition++;

                return;
            }

            //Servers to Minecraft
            if (statusPosition == 2)
            {
                await DiscordClient.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Playing,
                    Name = $"minecraft.koston.eu",
                }, UserStatus.Online);

                statusPosition++;

                return;
            }

            //Minecraft to Uptime
            if (statusPosition == 3)
            {
                await DiscordClient.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Playing,
                    Name = $"Online for: {botUptime.Days}d {botUptime.Hours:00}h {botUptime.Minutes:00}m",
                }, UserStatus.Online);

                statusPosition++;

                return;
            }

            //Uptme to Twitch Channels
            if (statusPosition == 4)
            {
                await DiscordClient.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.ListeningTo,
                    Name = $"{lst.Count} Twitch Channels!",
                }, UserStatus.Online);

                statusPosition++;

                return;
            }

            //Twitch Channels to Twitch Streamers
            if (statusPosition == 5)
            {
                if (liveChannels == 0)
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{liveChannels} Twitch Streams!",
                    }, UserStatus.Online);
                }

                if (liveChannels == 1)
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{liveChannels} Twitch Stream!",
                    }, UserStatus.Online);
                }

                else
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{liveChannels} Twitch Streams!",
                    }, UserStatus.Online);
                }

                statusPosition++;

                return;
            }

            //Twitch Channels to Servers
            if (statusPosition == 6)
            {
                if (guildCount == 1)
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Server!",
                    }, UserStatus.Online);
                }

                else
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordActivity
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildCount} Servers!",
                    }, UserStatus.Online);
                }

                statusPosition = 1;

                return;
            }
        }

        public static void ConsoleLog(string logLine, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(logLine);
            Console.ResetColor();
        }

        public static void Log(string logItem, ConsoleColor color = ConsoleColor.White)
        {
            var directory = $"/home/container/Logs/{DateTime.Now.Year}/{DateTime.Now.Month:d2}";

            // logging strings
            var date = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss zzz}] ";
            var dateFileName = $"{DateTime.Now:dd MMMM yyyy}";
            var header = $"[Log ] ";
            var log = $"{logItem}\n";

            // log to console
            ConsoleLog(date);
            ConsoleLog(header, ConsoleColor.DarkYellow);
            ConsoleLog(log, color);

            // ensure directory exists
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // log to file
            using StreamWriter w = File.AppendText($"{directory}/{dateFileName}.txt");
            w.WriteLine($"{date}: {logItem}");

            w.Close();
            w.Dispose();
        }

        public static void Heartbeat(string message)
        {
            var date = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss zzz}] ";
            var header = $"[Heartbeat ] ";
            var log = $"{message}\n"; 

            ConsoleLog(date);
            ConsoleLog(header, ConsoleColor.DarkBlue);
            ConsoleLog(log);
        }
    }
}
