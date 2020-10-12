using DiscordBot.Core.Services.Configs;
using DiscordBot.Core.Services.ReactionRoles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using DiscordBot.DAL.Models.ReactionRoles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{


    [Group("Admin")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    
    public class AdminCommands : BaseCommandModule
    {
        private readonly IGuildStreamerConfigService _guildStreamerConfigService;
        private readonly RPGContext _context;
        private readonly IMessageStoreService _messageStoreService;
        private readonly IConfiguration _configuration;
        private readonly IGameChannelConfigService _gameChannelConfigService;

        public AdminCommands(IGuildStreamerConfigService guildStreamerConfigService, RPGContext context, IMessageStoreService messageStoreService, IConfiguration configuration, IGameChannelConfigService gameChannelConfigService)
        {
            _guildStreamerConfigService = guildStreamerConfigService;
            _context = context;
            _messageStoreService = messageStoreService;
            _configuration = configuration;
            _gameChannelConfigService = gameChannelConfigService;
        }

        [Command("ping")]
        public async Task PingTime(CommandContext ctx)
        {
            var pingtime = ctx.Client.Ping.ToString();

            await ctx.Channel.SendMessageAsync($"Phew! I made it over the airwaves! This round trip took {pingtime}ms!").ConfigureAwait(false);
        }


        [Command("dtstart")]
        public async Task DowntimeStart(CommandContext ctx, [RemainingText] string reason)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Discord Bot Downtime",
                Color = DiscordColor.Blue,
                Description = $"The Discord Bot is going down for some maintainence or updates."
            };

            embed.AddField("Reason", reason);

            var configuredGamesChannels = _context.GameChannelConfigs.Where(x => x.ChannelId != 0);

            foreach(GameChannelConfig channel in configuredGamesChannels)
            {
                DiscordGuild guild = ctx.Client.Guilds.Values.FirstOrDefault(x => x.Id == channel.GuildId);

                if(guild == null) { continue; }

                DiscordChannel gamesChannel = guild.Channels.Values.FirstOrDefault(x => x.Id == channel.ChannelId);

                if(gamesChannel == null) { continue; }

                await gamesChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }

            await ctx.Client.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = ActivityType.Watching,
                Name = "For my new update!",
            }, UserStatus.DoNotDisturb);
        }

        [Command("dtcomplete")]
        public async Task DowntimeComplete(CommandContext ctx, [RemainingText] string update)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Discord Bot Downtime",
                Color = DiscordColor.Blue,
                Description = $"The Discord Bot is back up after some maintainence or updates."
            };

            embed.AddField("Updates", update);

            var configuredGamesChannels = _context.GameChannelConfigs.Where(x => x.ChannelId != 0);

            foreach (GameChannelConfig channel in configuredGamesChannels)
            {
                DiscordGuild guild = ctx.Client.Guilds.Values.FirstOrDefault(x => x.Id == channel.GuildId);

                if (guild == null) { continue; }

                DiscordChannel gamesChannel = guild.Channels.Values.FirstOrDefault(x => x.Id == channel.ChannelId);

                if (gamesChannel == null) { continue; }

                await gamesChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
        }
    }
}
