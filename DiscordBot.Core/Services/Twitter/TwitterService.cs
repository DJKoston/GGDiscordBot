using DiscordBot.DAL;
using DiscordBot.DAL.Models.Twitter;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Twitter
{
    public interface ITwitterService
    {
        Task AddNewMonitorAsync(ulong guildId, ulong channelId, string twitterUserName);
        Task RemoveMonitorAsync(ulong guildId, string twitterUserName);
        Task UpdateTweetLinkAsync(Tweet monitor, string newTweetLink);
        List<Tweet> GetMonitorGuildInfo(string twitterUserName);
        List<string> GetAllMonitoredAccounts();
        List<Tweet> GetGuildMonitors(ulong guildId);
    }

    public class TwitterService : ITwitterService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public TwitterService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddNewMonitorAsync(ulong guildId, ulong channelId, string twitterUserName)
        {
            using var context = new RPGContext(_options);

            var newMonitor = new Tweet
            {
                GuildID = guildId,
                ChannelID = channelId,
                TwitterUser = twitterUserName
            };

            await context.AddAsync(newMonitor);

            await context.SaveChangesAsync();
        }

        public async Task RemoveMonitorAsync(ulong guildId, string twitterUserName)
        {
            using var context = new RPGContext(_options);

            var monitorToRemove = context.Tweets.FirstOrDefaultAsync(x => x.GuildID == guildId && x.TwitterUser == twitterUserName);

            context.Remove(monitorToRemove);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTweetLinkAsync(Tweet monitor, string newTweetLink)
        {
            using var context = new RPGContext(_options);

            monitor.LastTweetLink = newTweetLink;

            context.Update(monitor);
            await context.SaveChangesAsync();
        }

        public List<Tweet> GetMonitorGuildInfo(string twitterUserName)
        {
            using var context = new RPGContext(_options);

            return context.Tweets.Where(x => x.TwitterUser == twitterUserName).ToList();
        }

        public List<string> GetAllMonitoredAccounts()
        {
            using var context = new RPGContext(_options);

            return context.Tweets.Where(x => x.TwitterUser != null).Select(x => x.TwitterUser).Distinct().ToList();
        }

        public List<Tweet> GetGuildMonitors(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return context.Tweets.Where(x => x.GuildID == guildId).ToList();
        }
    }
}
