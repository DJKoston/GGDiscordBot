using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Configs
{
    public interface ICurrencyNameConfigService
    {
        Task CreateCurrencyNameConfig(CurrencyNameConfig config);
        Task RemoveCurrencyNameConfig(CurrencyNameConfig config);
        Task<CurrencyNameConfig> GetCurrencyNameConfig(ulong GuildId);
    }
    public class CurrencyNameConfigService : ICurrencyNameConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public CurrencyNameConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateCurrencyNameConfig(CurrencyNameConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveCurrencyNameConfig(CurrencyNameConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<CurrencyNameConfig> GetCurrencyNameConfig(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.CurrencyNameConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}
