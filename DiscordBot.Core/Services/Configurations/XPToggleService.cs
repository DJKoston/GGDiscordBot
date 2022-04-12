using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface IXPToggleService
    {
        Task<string> ToggleXPForGuild(ulong guildId);
        Task<XPSystemConfig> GetGuildConfig(ulong guildId);
    }

    public class XPToggleService : IXPToggleService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public XPToggleService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task<string> ToggleXPForGuild(ulong guildId)
        {
            using var context = new RPGContext(_options);

            var config = await context.XPSystemConfigs.FirstOrDefaultAsync(x => x.GuildId == guildId);

            if(config == null)
            {
                var xpConfig = new XPSystemConfig
                {
                    GuildId = guildId,
                    Status = "disabled"
                };

                await context.AddAsync(xpConfig);

                await context.SaveChangesAsync();

                return "The XP System has been Disabled";
            }

            if(config.Status == "enabled")
            {
                config.Status = "disabled";

                context.Update(config);

                await context.SaveChangesAsync();

                return "The XP System has been Disabled";
            }

            if(config.Status == "disabled")
            {
                config.Status = "enabled";

                context.Update(config);

                await context.SaveChangesAsync();

                return "The XP System has been Enabled";
            }

            return "The Process Could not Complete the task.";
        }

        public async Task<XPSystemConfig> GetGuildConfig(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return await context.XPSystemConfigs.FirstOrDefaultAsync(x => x.GuildId == guildId);
        }
    }
}
