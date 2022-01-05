namespace DiscordBot.Bots.Commands
{
    [Group("streamer")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    public class StreamerCommands : BaseCommandModule
    {
        private readonly ICommunityStreamerService _communityStreamerService;
        private readonly INowLiveStreamerService _nowLiveStreamerService;
        private readonly IConfiguration _configuration;

        public StreamerCommands(ICommunityStreamerService communityStreamerService, INowLiveStreamerService nowLiveStreamerService, IConfiguration configuration)
        {
            _communityStreamerService = communityStreamerService;
            _nowLiveStreamerService = nowLiveStreamerService;
            _configuration = configuration;
        }

        [Command("approve")]
        public async Task ApproveStreamer(CommandContext ctx, int suggestionId, DiscordChannel announceChannel)
        {
            var suggestion = await _communityStreamerService.GetStreamer(ctx.Guild.Id, suggestionId);

            if (suggestion == null)
            {
                await ctx.Message.DeleteAsync();

                var approveRespnse = await ctx.Channel.SendMessageAsync("This ID does not exist. Please try again.").ConfigureAwait(false);

                Thread.Sleep(5000);

                await approveRespnse.DeleteAsync();

                return;
            }

            if (suggestion.DealtWith == "APPROVED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This request has already been approved!").ConfigureAwait(false);

                Thread.Sleep(5000);

                await approvedResponse.DeleteAsync();

                return;
            };

            if (suggestion.DealtWith == "DENIED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This request has already been rejected!").ConfigureAwait(false);

                Thread.Sleep(5000);

                await approvedResponse.DeleteAsync();

                return;
            }

            DiscordMember suggestor = await ctx.Guild.GetMemberAsync(suggestion.RequestorId);

            var api = new TwitchAPI();

            var clientid = _configuration["twitch-clientid"];
            var accesstoken = _configuration["twitch-accesstoken"];

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

            var searchStreamer = await api.V5.Search.SearchChannelsAsync(suggestion.StreamerName);

            if (searchStreamer.Total == 0) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {suggestion.StreamerName}."); return; }

            var stream = await api.V5.Users.GetUserByNameAsync(suggestion.StreamerName);

            if (stream.Total == 0) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {suggestion.StreamerName}."); return; }

            var streamResults = stream.Matches.FirstOrDefault();

            if (streamResults.DisplayName.ToLower() != suggestion.StreamerName.ToLower()) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {suggestion.StreamerName}."); return; }

            var streamerId = streamResults.Id;

            var getStreamId = await api.V5.Channels.GetChannelByIDAsync(streamerId);

            var announcementMessage = "%USER% has gone live streaming %GAME%! You should check them out over at: %URL%";

            var config = new NowLiveStreamer
            {
                AnnounceChannelId = announceChannel.Id,
                GuildId = ctx.Guild.Id,
                StreamerId = getStreamId.Id,
                AnnouncementMessage = announcementMessage,
                StreamerName = getStreamId.DisplayName
            };

            await _nowLiveStreamerService.CreateNewNowLiveStreamer(config);

            suggestion.DealtWith = "APPROVED";

            await _communityStreamerService.EditStreamer(suggestion);

            DiscordChannel channel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "streamers-to-approve");

            DiscordMessage message = await channel.GetMessageAsync(suggestion.RequestMessage);

            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = $"Request for streamer to be added to the bot by: {suggestor.DisplayName}",
                Description = suggestion.StreamerName,
                Color = DiscordColor.Green,
            };

            suggestionEmbed.AddField("This request Was Approved by:", $"{ctx.Member.Mention}");

            suggestionEmbed.WithFooter($"Request: {suggestion.Id}");

            DiscordEmbed newEmbed = suggestionEmbed;

            await message.ModifyAsync(embed: newEmbed).ConfigureAwait(false);

            await ctx.Message.DeleteAsync();

            var response = await ctx.Channel.SendMessageAsync($"Request {suggestion.Id} has been approved!").ConfigureAwait(false);

            Thread.Sleep(5000);

            await response.DeleteAsync();
        }

        [Command("reject")]
        public async Task Reject(CommandContext ctx, int suggestionId, [RemainingText] string reason)
        {
            var suggestion = await _communityStreamerService.GetStreamer(ctx.Guild.Id, suggestionId);

            if (suggestion == null)
            {
                await ctx.Message.DeleteAsync();

                var approveRespnse = await ctx.Channel.SendMessageAsync("This ID does not exist. Please try again.").ConfigureAwait(false);

                Thread.Sleep(5000);

                await approveRespnse.DeleteAsync();

                return;
            }

            if (suggestion.DealtWith == "APPROVED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This request has already been approved!").ConfigureAwait(false);

                Thread.Sleep(5000);

                await approvedResponse.DeleteAsync();

                return;
            };

            if (suggestion.DealtWith == "DENIED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This request has already been rejected!").ConfigureAwait(false);

                Thread.Sleep(5000);

                await approvedResponse.DeleteAsync();

                return;
            }

            DiscordMember suggestor = await ctx.Guild.GetMemberAsync(suggestion.RequestorId);

            if (reason == null) { }

            if (suggestor != null)
            {
                if (reason == null)
                {
                    await suggestor.SendMessageAsync($"Your Request to add `{suggestion.StreamerName}` to the Now Live bot has been rejected in the {ctx.Guild.Name} server!");
                }
                else
                {
                    await suggestor.SendMessageAsync($"Your Request to add `{suggestion.StreamerName}` to the Now Live bot has been approved in the {ctx.Guild.Name} server for the following reason:\n\n {reason}");
                }

            }

            suggestion.DealtWith = "DENIED";

            await _communityStreamerService.EditStreamer(suggestion);

            DiscordChannel channel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "streamers-to-approve");

            DiscordMessage message = await channel.GetMessageAsync(suggestion.RequestMessage);

            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = $"Request for streamer to be added to the bot by: {suggestor.DisplayName}",
                Description = suggestion.StreamerName,
                Color = DiscordColor.Red,
            };

            suggestionEmbed.AddField("This request Was Rejected by:", $"{ctx.Member.Mention}");

            suggestionEmbed.WithFooter($"Request: {suggestion.Id}");

            DiscordEmbed newEmbed = suggestionEmbed;

            await message.ModifyAsync(embed: newEmbed).ConfigureAwait(false);

            await ctx.Message.DeleteAsync();

            var response = await ctx.Channel.SendMessageAsync($"Suggestion {suggestion.Id} has been rejected!").ConfigureAwait(false);

            Thread.Sleep(5000);

            await response.DeleteAsync();
        }



    }
}
