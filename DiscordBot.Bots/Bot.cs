using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using TwitchLib.Api;
using TwitchLib.Api.Services;

namespace DiscordBot.Bots
{
    public class Bot
    {
        public DiscordClient DiscordClient { get; private set; }
        public InteractivityExtension DiscordInteractivity { get; private set; }
        public CommandsNextExtension DiscordCommands { get; private set; }
        public LavalinkExtension LavaLink { get; private set; }
        public LavalinkConfiguration LavalinkConfiguration;
        public LavalinkNodeConnection LavalinkNodeConnection;
        public LiveStreamMonitorService Monitor;
        public TwitchAPI Twitch;

        public Bot(IServiceProvider services, IConfiguration configuration)
        {
            // Get Configuration Information from appsettings.json
            var discordToken = configuration["discord-token"];
            var discordPrefix = configuration["discord-prefix"];
            var twitchClientId = configuration["twitch-clientid"];
            var twitchAccessToken = configuration["twitch-accesstoken"];
            var lavalinkHostname = configuration["lavalink-host"];
            var lavalinkPassword = configuration["lavalink-password"];

            // Twitch Connection
            Twitch = new TwitchAPI();

            Twitch.Settings.ClientId = twitchClientId;
            Twitch.Settings.AccessToken = twitchAccessToken;

            // Discord Connection
            var discordConfig = new DiscordConfiguration
            {
                Token = discordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.All,
            };

            var discordCommandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { discordPrefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = services,
            };

            DiscordClient = new DiscordClient(discordConfig);

            /*
            DiscordClient.Heartbeated += DiscordHeartbeat;
            DiscordClient.Ready += DiscordClientReady;
            DiscordClient.MessageCreated += DiscordMessageCreated;
            DiscordClient.ClientErrored += DiscordClientErrored;
            DiscordClient.GuildMemberAdded += DiscordGuildMemberAdded;
            DiscordClient.GuildMemberRemoved += DiscordMemberRemoved;
            DiscordClient.MessageReactionAdded += DiscordMessageReactionAdded;
            DiscordClient.MessageReactionRemoved += DiscordMessageReactionRemoved;
            DiscordClient.GuildAvailable += DiscordGuildAvaliable;
            DiscordClient.PresenceUpdated += DiscordPresenceUpdated;
            DiscordClient.GuildCreated += DiscordGuildCreated;
            DiscordClient.GuildUnavailable += DiscordGuildUnavaliable;
            */

            DiscordClient.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5),
            });

            DiscordCommands = DiscordClient.UseCommandsNext(discordCommandsConfig);

            /*
            DiscordCommands.CommandErrored += OnDiscordCommandErrored;
            DiscordCommands.CommandExecuted += OnDiscordCommandExecuted;
            */

            DiscordClient.ConnectAsync();

            // Lavalink Connection
            var lavalinkEndpoint = new ConnectionEndpoint
            {
                Hostname = lavalinkHostname,
                Port = 2333,
            };

            LavalinkConfiguration = new LavalinkConfiguration
            {
                Password = lavalinkPassword,
                RestEndpoint = lavalinkEndpoint,
                SocketEndpoint = lavalinkEndpoint,
            };

            LavaLink = DiscordClient.UseLavalink();
            /*
            // Twitch Livestream Monitor
            Monitor = new LiveStreamMonitorService(Twitch, 60);

            var lst = new List<string>();

            Monitor.SetChannelsById(lst);

            Monitor.Start();

            if (Monitor.Enabled)
            {
                Console.WriteLine("Connected to Twitch Live Monitoring Service");
                Console.WriteLine($"Bot has started monitoring {lst.Count} Twitch Channels.");
            }
            */
        }
    }
}
