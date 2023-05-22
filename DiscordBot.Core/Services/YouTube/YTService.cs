using DiscordBot.DAL;
using DiscordBot.DAL.Models.YouTube;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.YouTube
{
    public interface IYouTubeService
    {
        Task MarkAsPosted(YTVideos video);
        List<YTVideos> GetVideosToPost();
        Task AddNewYouTuber(YTAccounts account);
        Task RemoveYouTuber(YTAccounts account);
        List<YTAccounts> GetGuildYouTubers(ulong guildId);
    }

    public class YTService : IYouTubeService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public YTService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task MarkAsPosted(YTVideos video)
        {
            using var context = new RPGContext(_options);

            context.Remove(video);
            await context.SaveChangesAsync();
        }

        public  List<YTVideos> GetVideosToPost()
        {
            using var context = new RPGContext(_options);

            return context.YouTubeToAnnounce.ToList();
        }

        public async Task AddNewYouTuber(YTAccounts account)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(account);
            await context.SaveChangesAsync();
        }

        public async Task RemoveYouTuber(YTAccounts account)
        {
            using var context = new RPGContext(_options);

            context.Remove(account);
            await context.SaveChangesAsync();
        }

        public List<YTAccounts> GetGuildYouTubers(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return context.YouTubeAccounts.Where(x => x.GuildID == guildId).ToList();
        }
    }
}
