using DiscordBot.DAL;
using DiscordBot.DAL.Models.Counters;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Counters
{
    public interface IGoodBotBadBotService
    {
        Task AddGoodBot();
        Task AddBadBot();
        Task<GoodBotBadBot> GetGoodBot();
        Task<GoodBotBadBot> GetBadBot();
    }
    public class GoodBotBadBotService : IGoodBotBadBotService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public GoodBotBadBotService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddGoodBot()
        {
            using var context = new RPGContext(_options);

            var goodBotBadBot = await context.GoodBotBadBotCounters.FirstOrDefaultAsync(x => x.BotName == "GGBot");

            goodBotBadBot.GoodBot += 1;

            context.Update(goodBotBadBot);

            await context.SaveChangesAsync();
        }

        public async Task AddBadBot()
        {
            using var context = new RPGContext(_options);

            var goodBotBadBot = await context.GoodBotBadBotCounters.FirstOrDefaultAsync(x => x.BotName == "GGBot");

            goodBotBadBot.BadBot += 1;

            context.Update(goodBotBadBot);

            await context.SaveChangesAsync();
        }

        public async Task<GoodBotBadBot> GetGoodBot()
        {
            using var context = new RPGContext(_options);

            return await context.GoodBotBadBotCounters.FirstOrDefaultAsync(x => x.BotName == "GGBot");
        }

        public async Task<GoodBotBadBot> GetBadBot()
        {
            using var context = new RPGContext(_options);

            return await context.GoodBotBadBotCounters.FirstOrDefaultAsync(x => x.BotName == "GGBot");
        }
    }
}