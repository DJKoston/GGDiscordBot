using DiscordBot.DAL;
using DiscordBot.DAL.Models.Twitter;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Twitter
{
    public interface ITwitterService
    {
        Task AddNewMonitorAsync(ulong guildId, ulong channelId, string twitterUserName, string lastTweetId, string dateTime);
        Task RemoveMonitorAsync(ulong guildId, string twitterUserName);
        List<Tweet> GetGuildMonitors(ulong guildId);
        Task MarkTweetAsPosted (Tweet tweet);
        List<Tweet> GetTweetsToPost();
        List<Tweet> GetAllMonitoredAccounts();
    }

    public class TwitterService : ITwitterService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public TwitterService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddNewMonitorAsync(ulong guildId, ulong channelId, string twitterUserName, string lastTweetId, string dateTime)
        {
            using var context = new RPGContext(_options);

            var newMonitor = new Tweet
            {
                GuildID = guildId,
                ChannelID = channelId,
                TwitterUser = twitterUserName,
                LastTweetLink = lastTweetId,
                LastTweetDateTime = dateTime,
                IsPosted = true
            };

            await context.AddAsync(newMonitor);

            await context.SaveChangesAsync();
        }

        public async Task RemoveMonitorAsync(ulong guildId, string twitterUserName)
        {
            using var context = new RPGContext(_options);

            var monitorToRemove = await context.Tweets.FirstOrDefaultAsync(x => x.GuildID == guildId && x.TwitterUser == twitterUserName);

            context.Remove(monitorToRemove);
            await context.SaveChangesAsync();
        }

        public List<Tweet> GetGuildMonitors(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return context.Tweets.Where(x => x.GuildID == guildId).ToList();
        }

        public async Task MarkTweetAsPosted(Tweet tweet)
        {
            using var context = new RPGContext(_options);

            tweet.IsPosted = true;
            context.Update(tweet);
            await context.SaveChangesAsync();
        }

        public List<Tweet> GetTweetsToPost()
        {
            using var context = new RPGContext(_options);

            return context.Tweets.Where(x => x.IsPosted == false).ToList();
        }

        public List<Tweet> GetAllMonitoredAccounts()
        {
            using var context = new RPGContext(_options);

            return context.Tweets.Where(x=> x.TwitterUser != null).Distinct().ToList();
        }
    }
}
