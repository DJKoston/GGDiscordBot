using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface INowLiveRoleConfigService
    {
        Task AddNowLiveRole(NowLiveRoleConfig config);
        Task EditNowLiveRole(NowLiveRoleConfig config);
        Task RemoveNowLiveRole(NowLiveRoleConfig config);
        Task<NowLiveRoleConfig> GetNowLiveRole(ulong GuildId);
        List<NowLiveRoleConfig> GetAllNowLiveRoles();
    }
    public class NowLiveRoleConfigService : INowLiveRoleConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public NowLiveRoleConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddNowLiveRole(NowLiveRoleConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config);

            await context.SaveChangesAsync();
        }

        public async Task EditNowLiveRole(NowLiveRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Update(config);

            await context.SaveChangesAsync();
        }

        public async Task RemoveNowLiveRole(NowLiveRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync();
        }

        public async Task<NowLiveRoleConfig> GetNowLiveRole(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.NowLiveRoleConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }

        public List<NowLiveRoleConfig> GetAllNowLiveRoles()
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