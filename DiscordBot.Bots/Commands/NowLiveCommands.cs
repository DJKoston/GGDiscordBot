using DiscordBot.Core.Services.Configs;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Client;

namespace DiscordBot.Bots.Commands
{
    [Group("NowLive")]
    [Aliases("nl")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    public class NowLiveCommands : BaseCommandModule
    {
        private readonly IGuildStreamerConfigService _guildStreamerConfigService;
        private readonly RPGContext _context;
        private readonly IMessageStoreService _messageStoreService;

        public NowLiveCommands(IGuildStreamerConfigService guildStreamerConfigService, RPGContext context, IMessageStoreService messageStoreService)
        {
            _guildStreamerConfigService = guildStreamerConfigService;
            _context = context;
            _messageStoreService = messageStoreService;
        }
        
        [Command("addstreamer")]
        public async Task AddStreamer(CommandContext ctx, string twitchStreamer, DiscordChannel announceChannel)
        {
            var config = new GuildStreamerConfig
            {
                AnnounceChannelId = announceChannel.Id,
                GuildId = ctx.Guild.Id,
                StreamerId = twitchStreamer,
            };

            await _guildStreamerConfigService.CreateNewGuildStreamerConfig(config);

            await ctx.Channel.SendMessageAsync($"{twitchStreamer} will now be announced in {announceChannel.Mention}");
        }

        [Command("liststreamers")]
        public async Task ListStreamers(CommandContext ctx)
        {
            var streamers = _context.GuildStreamerConfigs.Where(x => x.GuildId == ctx.Guild.Id);
            
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Streamers to announce in {ctx.Guild.Name}",
                Color = DiscordColor.Purple
            };

            foreach(GuildStreamerConfig streamer in streamers)
            {
                DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                embed.AddField($"{streamer.StreamerId}", $"{channel.Mention}", true);
            }

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("removestreamer")]
        public async Task RemoveStreamer(CommandContext ctx, string twitchStreamer)
        {
            var config = await _guildStreamerConfigService.GetConfigToDelete(ctx.Guild.Id, twitchStreamer);
            var messages = await _messageStoreService.GetMessageStore(ctx.Guild.Id, twitchStreamer);

            DiscordChannel channel = ctx.Guild.GetChannel(config.AnnounceChannelId);

            await ctx.Channel.SendMessageAsync($"{twitchStreamer} will no longer be announced in {channel.Mention}");

            await _guildStreamerConfigService.RemoveGuildStreamerConfig(config);

            if(messages == null) { return; }

            else
            {
                DiscordMessage message = await channel.GetMessageAsync(messages.AnnouncementMessageId);

                await message.DeleteAsync();
                await _messageStoreService.RemoveMessageStore(messages);
            }
        }

    }
}
