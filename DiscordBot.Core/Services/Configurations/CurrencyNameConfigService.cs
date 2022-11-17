﻿using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configurations
{
    public interface ICurrencyNameConfigService
    {
        Task NewCurrencyName(CurrencyNameConfig config);
        Task EditCurrencyName(CurrencyNameConfig config);
        Task RemoveCurrencyName(CurrencyNameConfig config);
        Task<CurrencyNameConfig> GetCurrencyNameConfig(ulong GuildId);
    }
    public class CurrencyNameConfigService : ICurrencyNameConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public CurrencyNameConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task NewCurrencyName(CurrencyNameConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config);

            await context.SaveChangesAsync();
        }

        public async Task EditCurrencyName(CurrencyNameConfig config)
        {
            using var context = new RPGContext(_options);

            context.Update(config);

            await context.SaveChangesAsync();
        }

        public async Task RemoveCurrencyName(CurrencyNameConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync();
        }

        public async Task<CurrencyNameConfig> GetCurrencyNameConfig(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.CurrencyNameConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }
    }
}