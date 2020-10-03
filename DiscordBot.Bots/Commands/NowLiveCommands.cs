using DiscordBot.Core.Services.Configs;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.V5.Models.Search;
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
            var announcementMessage = "%USER% has gone live streaming %GAME%! You should check them out over at: %URL%";

            await AddStreamerAsync(ctx, twitchStreamer, announceChannel, announcementMessage);
        }

        [Command("addstreamer")]
        public async Task AddStreamer(CommandContext ctx, string twitchStreamer, DiscordChannel announceChannel, string announcementMessage)
        {
            await AddStreamerAsync(ctx, twitchStreamer, announceChannel, announcementMessage);
        }

        public async Task AddStreamerAsync(CommandContext ctx, string twitchStreamer, DiscordChannel announceChannel, string announcementMessage)
        {
            var api = new TwitchAPI();

            var clientid = "gp762nuuoqcoxypju8c569th9wz7q5";
            var accesstoken = "j1oz9yx9b07c8j22vym2oba32qwnhb";

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

            var searchStreamer = await api.V5.Search.SearchChannelsAsync(twitchStreamer);

            if(searchStreamer.Total == 0) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {twitchStreamer}."); return; }

            var stream = await api.V5.Users.GetUserByNameAsync(twitchStreamer);

            if (stream.Total == 0) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {twitchStreamer}."); return; }

            var streamResults = stream.Matches.FirstOrDefault();

            if (streamResults.DisplayName.ToLower() != twitchStreamer.ToLower()) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {twitchStreamer}."); return; }

            var streamerId = streamResults.Id;

            var getStreamId = await api.V5.Channels.GetChannelByIDAsync(streamerId);

            var config = new GuildStreamerConfig
            {
                AnnounceChannelId = announceChannel.Id,
                GuildId = ctx.Guild.Id,
                StreamerId = getStreamId.Id,
                AnnouncementMessage = announcementMessage,
            };

            await _guildStreamerConfigService.CreateNewGuildStreamerConfig(config);

            await ctx.Channel.SendMessageAsync($"{getStreamId.DisplayName} will now be announced in {announceChannel.Mention}");
        }

        [Command("liststreamers")]
        public async Task ListStreamers(CommandContext ctx)
        {
            var streamers = _context.GuildStreamerConfigs.Where(x => x.GuildId == ctx.Guild.Id);

            var api = new TwitchAPI();

            var clientid = "gp762nuuoqcoxypju8c569th9wz7q5";
            var accesstoken = "j1oz9yx9b07c8j22vym2oba32qwnhb";

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Streamers to announce in {ctx.Guild.Name}",
                Color = DiscordColor.Purple
            };

            foreach(GuildStreamerConfig streamer in streamers)
            {
                DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                embed.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
            }

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("removestreamer")]
        public async Task RemoveStreamer(CommandContext ctx, string twitchStreamer)
        {
            var api = new TwitchAPI();

            var clientid = "gp762nuuoqcoxypju8c569th9wz7q5";
            var accesstoken = "j1oz9yx9b07c8j22vym2oba32qwnhb";

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

            var searchStreamer = await api.V5.Search.SearchChannelsAsync(twitchStreamer);

            if (searchStreamer.Total == 0) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {twitchStreamer}."); return; }

            var stream = await api.V5.Users.GetUserByNameAsync(twitchStreamer);

            var streamResults = stream.Matches.FirstOrDefault();

            var streamerId = streamResults.Id;

            var getStreamId = await api.V5.Channels.GetChannelByIDAsync(streamerId);

            var config = await _guildStreamerConfigService.GetConfigToDelete(ctx.Guild.Id, getStreamId.Id);
            if(config == null) { await ctx.Channel.SendMessageAsync($"No configuration found for {twitchStreamer}, do `!nl liststreamers` to see the list of streamers you can remove."); return; }

            var messages = await _messageStoreService.GetMessageStore(ctx.Guild.Id, getStreamId.Id);

            DiscordChannel channel = ctx.Guild.GetChannel(config.AnnounceChannelId);

            await ctx.Channel.SendMessageAsync($"{getStreamId.DisplayName} will no longer be announced in {channel.Mention}");

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
