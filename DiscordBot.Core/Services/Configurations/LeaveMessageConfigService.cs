using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface ILeaveMessageConfigService
    {
        Task AddLeaveMessage(LeaveConfig config);
        Task EditLeaveMessage(LeaveConfig config);
        Task RemoveLeaveMessage(LeaveConfig config);
        Task<LeaveConfig> GetLeaveMessageConfig(ulong GuildId);
    }
    public class LeaveMessageConfigService : ILeaveMessageConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public LeaveMessageConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddLeaveMessage(LeaveConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task EditLeaveMessage(LeaveConfig config)
        {
            using var context = new RPGContext(_options);

            context.Update(config);

            await context.SaveChangesAsync();
        }

        public async Task RemoveLeaveMessage(LeaveConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<LeaveConfig> GetLeaveMessageConfig(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.LeaveConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}