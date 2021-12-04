using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface IWelcomeMessageConfigService
    {
        Task AddWelcomeMessage(WelcomeConfig config);
        Task EditWelcomeMessage(WelcomeConfig config);
        Task RemoveWelcomeMessage(WelcomeConfig config);
        Task<WelcomeConfig> GetWelcomeMessage(ulong GuildId);
    }
    public class WelcomeMessageConfigService : IWelcomeMessageConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public WelcomeMessageConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddWelcomeMessage(WelcomeConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task EditWelcomeMessage(WelcomeConfig config)
        {
            using var context = new RPGContext(_options);

            context.Update(config);

            await context.SaveChangesAsync();
        }

        public async Task RemoveWelcomeMessage(WelcomeConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<WelcomeConfig> GetWelcomeMessage(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.WelcomeConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}