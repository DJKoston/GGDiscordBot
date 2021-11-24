using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface IDoubleXPRoleConfigService
    {
        Task CreateNewNitroBoosterRoleConfig(DoubleXPRoleConfig config);
        Task RemoveNitroBoosterConfig(DoubleXPRoleConfig config);
        Task<DoubleXPRoleConfig> GetNitroBoosterConfig(ulong GuildId);
    }
    public class DoubleXPRoleConfigService : IDoubleXPRoleConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public DoubleXPRoleConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewNitroBoosterRoleConfig(DoubleXPRoleConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveNitroBoosterConfig(DoubleXPRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<DoubleXPRoleConfig> GetNitroBoosterConfig(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.DoubleXPRoleConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}