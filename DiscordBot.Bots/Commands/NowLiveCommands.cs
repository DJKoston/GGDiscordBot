using DiscordBot.Core.Services.Configs;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;

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
        private readonly IConfiguration _configuration;

        public NowLiveCommands(IGuildStreamerConfigService guildStreamerConfigService, RPGContext context, IMessageStoreService messageStoreService, IConfiguration configuration)
        {
            _guildStreamerConfigService = guildStreamerConfigService;
            _context = context;
            _messageStoreService = messageStoreService;
            _configuration = configuration;
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

            var clientid = _configuration["twitch-clientid"];
            var accesstoken = _configuration["twitch-accesstoken"];

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
                StreamerName = getStreamId.DisplayName
            };

            await _guildStreamerConfigService.CreateNewGuildStreamerConfig(config);

            await ctx.Channel.SendMessageAsync($"{getStreamId.DisplayName} will now be announced in {announceChannel.Mention}");
        }

        [Command("liststreamers")]
        public async Task ListStreamers(CommandContext ctx)
        {
            var streamers = _context.GuildStreamerConfigs.Where(x => x.GuildId == ctx.Guild.Id).OrderBy(x => x.StreamerName);

            var streamerspage1 = streamers.Take(24);
            var streamerspage2 = streamers.Skip(24).Take(24);
            var streamerspage3 = streamers.Skip(48).Take(24);
            var streamerspage4 = streamers.Skip(72).Take(24);

            var api = new TwitchAPI();

            var clientid = _configuration["twitch-clientid"];
            var accesstoken = _configuration["twitch-accesstoken"];

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

            if(streamerspage1 != null) 
            {
                var embed1 = new DiscordEmbedBuilder
                {
                    Title = $"Streamers to announce in {ctx.Guild.Name}",
                    Color = DiscordColor.Purple
                };

                foreach (GuildStreamerConfig streamer in streamerspage1)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                    var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                    embed1.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                }

                if (embed1.Fields.Count != 0) { await ctx.Channel.SendMessageAsync(embed: embed1).ConfigureAwait(false); }
            }

            if (streamerspage2 != null)
            {
                var embed2 = new DiscordEmbedBuilder
                {
                    Title = $"Streamers to announce in {ctx.Guild.Name}",
                    Color = DiscordColor.Purple
                };

                foreach (GuildStreamerConfig streamer in streamerspage2)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                    var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                    embed2.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                }

                if (embed2.Fields.Count != 0) { await ctx.Channel.SendMessageAsync(embed: embed2).ConfigureAwait(false); }
            }

            if (streamerspage3 != null)
            {
                var embed3 = new DiscordEmbedBuilder
                {
                    Title = $"Streamers to announce in {ctx.Guild.Name}",
                    Color = DiscordColor.Purple
                };

                foreach (GuildStreamerConfig streamer in streamerspage3)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                    var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                    embed3.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                }

                if(embed3.Fields.Count != 0) { await ctx.Channel.SendMessageAsync(embed: embed3).ConfigureAwait(false); }
            }

            if (streamerspage4 != null)
            {
                var embed4 = new DiscordEmbedBuilder
                {
                    Title = $"Streamers to announce in {ctx.Guild.Name}",
                    Color = DiscordColor.Purple
                };

                foreach (GuildStreamerConfig streamer in streamerspage4)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                    var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                    embed4.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                }

                if (embed4.Fields.Count != 0) { await ctx.Channel.SendMessageAsync(embed: embed4).ConfigureAwait(false); }
            }
            

        }

        [Command("monitor")]
        public async Task ViewMonitorLoads(CommandContext ctx)
        {
            var firstlst = _guildStreamerConfigService.GetGuildStreamerList();

            var lst = firstlst.Distinct();

            var embed = new DiscordEmbedBuilder
            {
                Title = "GG-Bot Twitch Monitor Loadbalancing",
                Color = DiscordColor.Orange,
            };

            if(lst.Count() == 0) { embed.AddField("Monitor 1:", "Offline"); }
            else { embed.AddField("Monitor 1:", $"Monitoring {lst.Count()} Channels"); }


            embed.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("removestreamer")]
        public async Task RemoveStreamer(CommandContext ctx, string twitchStreamer)
        {
            var api = new TwitchAPI();

            var clientid = _configuration["twitch-clientid"];
            var accesstoken = _configuration["twitch-accesstoken"];

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
