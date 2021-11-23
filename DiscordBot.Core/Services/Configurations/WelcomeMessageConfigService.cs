using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface IWelcomeMessageConfigService
    {
        Task CreateNewWelcomeMessageConfig(WelcomeMessageConfig config);
        Task RemoveWelcomeMessageConfig(WelcomeMessageConfig config);
        Task<WelcomeMessageConfig> GetWelcomeMessageConfig(ulong GuildId);
    }
    public class WelcomeMessageConfigService : IWelcomeMessageConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public WelcomeMessageConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewWelcomeMessageConfig(WelcomeMessageConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveWelcomeMessageConfig(WelcomeMessageConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<WelcomeMessageConfig> GetWelcomeMessageConfig(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.WelcomeMessageConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}