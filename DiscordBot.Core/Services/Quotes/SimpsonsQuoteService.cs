using DiscordBot.DAL;
using DiscordBot.DAL.Models.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Quotes
{
    public interface ISimpsonsQuoteService
    {
        Task<SimpsonsQuote> GetSimpsonsQuote();
    }

    public class SimpsonsQuoteService : ISimpsonsQuoteService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public SimpsonsQuoteService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task<SimpsonsQuote> GetSimpsonsQuote()
        {
            using var context = new RPGContext(_options);

            var allQuotesCount = await context.SimpsonsQuotes.CountAsync();

            var rndm = new Random();
            var RandomQuoteID = rndm.Next(1, allQuotesCount + 1);

            return await context.SimpsonsQuotes.FirstOrDefaultAsync(x => x.Id == RandomQuoteID);
        }
    }
}