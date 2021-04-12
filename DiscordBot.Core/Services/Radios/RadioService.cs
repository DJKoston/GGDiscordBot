using DiscordBot.DAL;
using DiscordBot.DAL.Models.Radios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Radios
{
    public interface IRadioService
    {
        Task CreateNewRadioAsync(Radio quote);
        Task DeleteRadioAsync(Radio quote);
        Task<Radio> GetRadioAsync(ulong guildId);
    }

    public class RadioService : IRadioService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public RadioService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewRadioAsync(Radio radio)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(radio).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteRadioAsync(Radio radio)
        {
            using var context = new RPGContext(_options);

            context.Remove(radio);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Radio> GetRadioAsync(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return await context.Radios.FirstOrDefaultAsync(x => x.GuildId == guildId).ConfigureAwait(false);
        }
    }
}
