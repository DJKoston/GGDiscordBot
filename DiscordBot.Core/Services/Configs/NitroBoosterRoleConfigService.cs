using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Configs
{
    public interface INitroBoosterRoleConfigService
    {
        Task CreateNewNitroBoosterRoleConfig(NitroBoosterRoleConfig config);
        Task RemoveNitroBoosterConfig(NitroBoosterRoleConfig config);
        Task<NitroBoosterRoleConfig> GetNitroBoosterConfig(ulong GuildId);
    }
    public class NitroBoosterRoleConfigService : INitroBoosterRoleConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public NitroBoosterRoleConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewNitroBoosterRoleConfig(NitroBoosterRoleConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveNitroBoosterConfig(NitroBoosterRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<NitroBoosterRoleConfig> GetNitroBoosterConfig(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.NitroBoosterConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}
