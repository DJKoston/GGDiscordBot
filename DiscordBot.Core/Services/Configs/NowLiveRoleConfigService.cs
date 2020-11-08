using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Configs
{
    public interface INowLiveRoleConfigService
    {
        Task CreateNowLiveRoleConfig(NowLiveRoleConfig config);
        Task RemoveNowLiveRoleConfig(NowLiveRoleConfig config);
        Task<NowLiveRoleConfig> GetNowLiveRoleConfig(ulong GuildId);
        List<NowLiveRoleConfig> GetAllConfigs();
    }
    public class NowLiveRoleConfigService : INowLiveRoleConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public NowLiveRoleConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNowLiveRoleConfig(NowLiveRoleConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveNowLiveRoleConfig(NowLiveRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<NowLiveRoleConfig> GetNowLiveRoleConfig(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.NowLiveRoleConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }

        public List<NowLiveRoleConfig> GetAllConfigs()
        {
            using var context = new RPGContext(_options);

            var configs = context.NowLiveRoleConfigs;

            var list = new List<NowLiveRoleConfig> { };

            foreach (NowLiveRoleConfig config in configs)
            {
                list.Add(config);
            }

            return list;
        }
    }
}
