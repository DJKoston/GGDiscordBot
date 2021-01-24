using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Configs
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

            var goodBotBadBot = await context.GoodBotBadBots.FirstOrDefaultAsync(x => x.BotName == "GGBot").ConfigureAwait(false);

            goodBotBadBot.GoodBot += 1;

            context.Update(goodBotBadBot);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddBadBot()
        {
            using var context = new RPGContext(_options);

            var goodBotBadBot = await context.GoodBotBadBots.FirstOrDefaultAsync(x => x.BotName == "GGBot").ConfigureAwait(false);

            goodBotBadBot.BadBot += 1;

            context.Update(goodBotBadBot);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<GoodBotBadBot>GetGoodBot()
        {
            using var context = new RPGContext(_options);

            return await context.GoodBotBadBots.FirstOrDefaultAsync(x => x.BotName == "GGBot").ConfigureAwait(false);
        }

        public async Task<GoodBotBadBot> GetBadBot()
        {
            using var context = new RPGContext(_options);

            return await context.GoodBotBadBots.FirstOrDefaultAsync(x => x.BotName == "GGBot").ConfigureAwait(false);
        }
    }
}
