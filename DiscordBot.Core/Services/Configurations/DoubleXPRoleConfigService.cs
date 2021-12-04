using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface IDoubleXPRoleConfigService
    {
        Task AddDoubleXPRole(DoubleXPRoleConfig config);
        Task EditDoubleXPRole(DoubleXPRoleConfig config);
        Task DeleteDoubleXPRole(DoubleXPRoleConfig config);
        Task<DoubleXPRoleConfig> GetDoubleXPRole(ulong GuildId);
    }
    public class DoubleXPRoleConfigService : IDoubleXPRoleConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public DoubleXPRoleConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddDoubleXPRole(DoubleXPRoleConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config);

            await context.SaveChangesAsync();
        }

        public async Task EditDoubleXPRole(DoubleXPRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Update(config);

            await context.SaveChangesAsync();
        }

        public async Task DeleteDoubleXPRole(DoubleXPRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync();
        }

        public async Task<DoubleXPRoleConfig> GetDoubleXPRole(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.DoubleXPRoleConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}