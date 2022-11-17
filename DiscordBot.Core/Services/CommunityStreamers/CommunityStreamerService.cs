using DiscordBot.DAL;
using DiscordBot.DAL.Models.CommunityStreamers;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.CommunityStreamers
{
    public interface ICommunityStreamerService
    {
        Task CreateNewStreamer(CommunityStreamer streamer);
        Task<CommunityStreamer> GetStreamer(ulong GuildId, int Id);
        Task DeleteStreamer(CommunityStreamer streamer);
        Task EditStreamer(CommunityStreamer streamer);
    }

    public class CommunityStreamerService : ICommunityStreamerService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public CommunityStreamerService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewStreamer(CommunityStreamer streamer)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(streamer);

            await context.SaveChangesAsync();
        }

        public async Task EditStreamer(CommunityStreamer streamer)
        {
            using var context = new RPGContext(_options);

            context.Update(streamer);

            await context.SaveChangesAsync();
        }

        public async Task DeleteStreamer(CommunityStreamer streamer)
        {
            using var context = new RPGContext(_options);

            context.Remove(streamer);

            await context.SaveChangesAsync();
        }

        public async Task<CommunityStreamer> GetStreamer(ulong GuildId, int Id)
        {
            using var context = new RPGContext(_options);

            var guildContext = context.CommunityStreamers.Where(x => x.GuildId == GuildId);

            return await guildContext.FirstOrDefaultAsync(x => x.Id == Id);
        }
    }
}