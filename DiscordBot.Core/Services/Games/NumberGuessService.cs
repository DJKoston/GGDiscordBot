using DiscordBot.DAL;
using DiscordBot.DAL.Models.Games;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Games
{
    public interface INumberGuessService
    {
        Task CreateNumberGuess(ulong guildId);
        Task UpdateNumberGuess(ulong guildId);
        Task<NumberGuess> GetNumberGuess(ulong guildId);
    }

    public class NumberGuessService : INumberGuessService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public NumberGuessService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNumberGuess(ulong guildId)
        {
            using var context = new RPGContext(_options);

            NumberGuess numberGuess = new();

            numberGuess.GuildId = guildId;

            Random rnd = new();
            numberGuess.Number = rnd.Next(0, 11);

            await context.AddAsync(numberGuess);

            await context.SaveChangesAsync();
        }

        public async Task UpdateNumberGuess(ulong guildId)
        {
            using var context = new RPGContext(_options);

            NumberGuess numberGuess = await context.NumberGuesses.FirstOrDefaultAsync(x => x.GuildId == guildId);

            Random rnd = new();
            numberGuess.Number = rnd.Next(0, 11);

            context.Update(numberGuess);

            await context.SaveChangesAsync();
        }

        public async Task<NumberGuess> GetNumberGuess(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return await context.NumberGuesses.FirstOrDefaultAsync(x => x.GuildId == guildId);
        }
    }
}
