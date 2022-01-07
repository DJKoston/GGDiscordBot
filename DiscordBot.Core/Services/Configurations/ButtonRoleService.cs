using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface IButtonRoleService
    {
        Task CreateButtonRole(ButtonRoleConfig config);
        Task DeleteButtonRole(ButtonRoleConfig config);
        Task<ButtonRoleConfig> GetButtonRole(ulong guildId, string buttonId);
    }
    public class ButtonRoleService : IButtonRoleService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public ButtonRoleService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateButtonRole(ButtonRoleConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteButtonRole(ButtonRoleConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<ButtonRoleConfig> GetButtonRole(ulong guildId, string buttonId)
        {
            using var context = new RPGContext(_options);

            return await context.ButtonRoleConfigs.FirstOrDefaultAsync(x => x.GuildId == guildId && x.ButtonId == buttonId);
        }
    }
}