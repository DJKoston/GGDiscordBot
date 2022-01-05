namespace DiscordBot.Bots.Commands
{
    [Group("nl")]
    [Aliases("NowLive")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    public class NowLiveCommands : BaseCommandModule
    {
        [Group("twitch")]
        public class TwitchCommands : BaseCommandModule
        {
            private readonly INowLiveStreamerService _nowLiveStreamerService;
            private readonly RPGContext _context;
            private readonly INowLiveMessageService _nowLiveMessageService;
            private readonly IConfiguration _configuration;

            public TwitchCommands(INowLiveStreamerService guildStreamerConfigService, RPGContext context, INowLiveMessageService messageStoreService, IConfiguration configuration)
            {
                _nowLiveStreamerService = guildStreamerConfigService;
                _context = context;
                _nowLiveMessageService = messageStoreService;
                _configuration = configuration;
            }

            [Command("add")]
            public async Task AddStreamer(CommandContext ctx, string twitchStreamer, DiscordChannel announceChannel)
            {
                var announcementMessage = "%USER% has gone live streaming %GAME%! You should check them out over at: %URL%";

                await AddStreamerAsync(ctx, twitchStreamer, announceChannel, announcementMessage);
            }

            [Command("add")]
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

                if (searchStreamer.Total == 0)
                {
                    var messageBuilder1 = new DiscordMessageBuilder
                    {
                        Content = $"There was no channel with the username {twitchStreamer}.",
                    };

                    messageBuilder1.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder1).ConfigureAwait(false);

                    return;
                }

                var stream = await api.V5.Users.GetUserByNameAsync(twitchStreamer);

                if (stream.Total == 0)
                {
                    var messageBuilder2 = new DiscordMessageBuilder
                    {
                        Content = $"There was no channel with the username {twitchStreamer}.",
                    };

                    messageBuilder2.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder2).ConfigureAwait(false);

                    return;
                }

                var streamResults = stream.Matches.FirstOrDefault();

                if (streamResults.DisplayName.ToLower() != twitchStreamer.ToLower())
                {
                    var messageBuilder3 = new DiscordMessageBuilder
                    {
                        Content = $"There was no channel with the username {twitchStreamer}.",
                    };

                    messageBuilder3.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder3).ConfigureAwait(false);

                    return;
                }

                var streamerId = streamResults.Id;

                var getStreamId = await api.V5.Channels.GetChannelByIDAsync(streamerId);

                var config = new NowLiveStreamer
                {
                    AnnounceChannelId = announceChannel.Id,
                    GuildId = ctx.Guild.Id,
                    StreamerId = getStreamId.Id,
                    AnnouncementMessage = announcementMessage,
                    StreamerName = getStreamId.DisplayName
                };

                await _nowLiveStreamerService.CreateNewNowLiveStreamer(config);

                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = $"{getStreamId.DisplayName} will now be announced in {announceChannel.Mention}",
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }

            [Command("list")]
            public async Task ListStreamers(CommandContext ctx)
            {
                var streamers = _context.NowLiveStreamers.Where(x => x.GuildId == ctx.Guild.Id).OrderBy(x => x.StreamerName);

                var streamerspage1 = streamers.Take(24);
                var streamerspage2 = streamers.Skip(24).Take(24);
                var streamerspage3 = streamers.Skip(48).Take(24);
                var streamerspage4 = streamers.Skip(72).Take(24);
                var streamerspage5 = streamers.Skip(96).Take(24);

                var api = new TwitchAPI();

                var clientid = _configuration["twitch-clientid"];
                var accesstoken = _configuration["twitch-accesstoken"];

                api.Settings.ClientId = clientid;
                api.Settings.AccessToken = accesstoken;

                if (streamerspage1 != null)
                {
                    var embed1 = new DiscordEmbedBuilder
                    {
                        Title = $"{streamers.Count()} Twitch Streamers being announced in {ctx.Guild.Name}",
                        Color = DiscordColor.Purple
                    };

                    foreach (NowLiveStreamer streamer in streamerspage1)
                    {
                        DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                        var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                        embed1.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                    }

                    if (embed1.Fields.Count != 0)
                    {
                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = embed1,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
                    }
                }

                if (streamerspage2 != null)
                {
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Title = $"Twitch Streamers being announced in {ctx.Guild.Name}",
                        Color = DiscordColor.Purple
                    };

                    foreach (NowLiveStreamer streamer in streamerspage2)
                    {
                        DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                        var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                        embed2.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                    }

                    if (embed2.Fields.Count != 0)
                    {
                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = embed2,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
                    }
                }

                if (streamerspage3 != null)
                {
                    var embed3 = new DiscordEmbedBuilder
                    {
                        Title = $"Twitch Streamers being announced in {ctx.Guild.Name}",
                        Color = DiscordColor.Purple
                    };

                    foreach (NowLiveStreamer streamer in streamerspage3)
                    {
                        DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                        var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                        embed3.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                    }

                    if (embed3.Fields.Count != 0)
                    {
                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = embed3,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
                    }
                }

                if (streamerspage4 != null)
                {
                    var embed4 = new DiscordEmbedBuilder
                    {
                        Title = $"Twitch Streamers being announced in {ctx.Guild.Name}",
                        Color = DiscordColor.Purple
                    };

                    foreach (NowLiveStreamer streamer in streamerspage4)
                    {
                        DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                        var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                        embed4.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                    }

                    if (embed4.Fields.Count != 0)
                    {
                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = embed4,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
                    }
                }

                if (streamerspage5 != null)
                {
                    var embed5 = new DiscordEmbedBuilder
                    {
                        Title = $"Twitch Streamers being announced in {ctx.Guild.Name}",
                        Color = DiscordColor.Purple
                    };

                    foreach (NowLiveStreamer streamer in streamerspage5)
                    {
                        DiscordChannel channel = ctx.Guild.GetChannel(streamer.AnnounceChannelId);

                        var stream = await api.V5.Users.GetUserByIDAsync(streamer.StreamerId);

                        embed5.AddField($"{stream.DisplayName}", $"{channel.Mention}", true);
                    }

                    if (embed5.Fields.Count != 0)
                    {
                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = embed5,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
                    }
                }
            }

            [Command("remove")]
            public async Task RemoveStreamer(CommandContext ctx, string twitchStreamer)
            {
                var api = new TwitchAPI();

                var clientid = _configuration["twitch-clientid"];
                var accesstoken = _configuration["twitch-accesstoken"];

                api.Settings.ClientId = clientid;
                api.Settings.AccessToken = accesstoken;

                var searchStreamer = await api.V5.Search.SearchChannelsAsync(twitchStreamer);

                if (searchStreamer.Total == 0)
                {
                    var messageBuilder1 = new DiscordMessageBuilder
                    {
                        Content = $"There was no channel with the username {twitchStreamer}.",
                    };

                    messageBuilder1.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder1).ConfigureAwait(false);

                    return;
                }

                var stream = await api.V5.Users.GetUserByNameAsync(twitchStreamer);

                var streamResults = stream.Matches.FirstOrDefault();

                var streamerId = streamResults.Id;

                var getStreamId = await api.V5.Channels.GetChannelByIDAsync(streamerId);

                var config = await _nowLiveStreamerService.GetStreamerToDelete(ctx.Guild.Id, getStreamId.Id);

                if (config == null)
                {
                    var messageBuilder2 = new DiscordMessageBuilder
                    {
                        Content = $"No configuration found for {twitchStreamer}, do `!nl liststreamers` to see the list of streamers you can remove.",
                    };

                    messageBuilder2.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder2).ConfigureAwait(false);

                    return;
                }

                var messages = await _nowLiveMessageService.GetMessageStore(ctx.Guild.Id, getStreamId.Id);

                DiscordChannel channel = ctx.Guild.GetChannel(config.AnnounceChannelId);

                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = $"{getStreamId.DisplayName} will no longer be announced in {channel.Mention}",
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                await _nowLiveStreamerService.RemoveNowLiveStreamer(config);

                if (messages == null) { return; }

                else
                {
                    DiscordMessage message = await channel.GetMessageAsync(messages.AnnouncementMessageId);

                    await message.DeleteAsync();
                    await _nowLiveMessageService.RemoveMessageStore(messages);
                }
            }
        }
    }
}