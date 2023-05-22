using DiscordBot.Core.Services.YouTube;
using DiscordBot.DAL.Models.YouTube;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace DiscordBot.Bots.Commands
{
    [Group("youtube")]
    [RequirePermissions(Permissions.Administrator)]
    public class YouTubeCommands : BaseCommandModule
    {
        private readonly IYouTubeService _youtubeService;
        private readonly IConfiguration _configuration;

       public YouTubeCommands(IYouTubeService youtubeService, IConfiguration configuration)
        {
            _youtubeService = youtubeService;
            _configuration = configuration;
        }

        [Command("add")]
        public async Task AddYouTuber(CommandContext ctx, string youtubeHandle, DiscordChannel channel)
        {
            var youtubeClient = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _configuration["youtube-token"],
                ApplicationName = this.GetType().Assembly.GetName().Name,
            });

            var ytChannels = youtubeClient.Search.List("snippet");
            ytChannels.Q = youtubeHandle;
            ytChannels.Type = "channel";
            ytChannels.MaxResults = 1;

            var channelResponse = await ytChannels.ExecuteAsync();
            var channelResult = channelResponse.Items.FirstOrDefault();

            var userName = channelResult.Snippet.Title;
            var userId = channelResult.Id.ChannelId;

            var IsInDatabase = _youtubeService.GetGuildYouTubers(ctx.Guild.Id).FirstOrDefault(x => x.YouTubeUserId == userId);

            if (IsInDatabase != null) { await ctx.RespondAsync("This channel is already being announced in this Discord."); return; }

            var newYouTuber = new YTAccounts
            {
                GuildID = ctx.Guild.Id,
                ChannelID = channel.Id,
                LastVideoDateTime = DateTime.UtcNow.ToString(),
                LastVideoURL = "",
                YouTubeUserId = userId
            };

            await _youtubeService.AddNewYouTuber(newYouTuber);

            await ctx.RespondAsync($"I will now check {userName}'s channel: https://youtube.com/channel/{channelResult.Id.ChannelId} every 30 mins to see if they have released a new video.\n\nIf this is the wrong channel, please remove try again using your YouTube User ID, it will look something like this: 'UCuAXFkgsw1L7xaCfnd5JJOw'\n\nTo remove this channel, do `!youtube remove {userId}`");
        }

        [Command("remove")]
        public async Task RemoveYouTuber(CommandContext ctx, string youtubeUserId)
        {
            var IsInDatabase = _youtubeService.GetGuildYouTubers(ctx.Guild.Id).FirstOrDefault(x => x.YouTubeUserId == youtubeUserId);

            if (IsInDatabase == null) { await ctx.RespondAsync("I can't remove what isn't there. This channel isnt being announced in the Discord."); return; }

            await _youtubeService.RemoveYouTuber(IsInDatabase);

            await ctx.RespondAsync($"I have successfully removed {IsInDatabase.YouTubeUserId} from being announced in this discord.");
        }
    }
}
