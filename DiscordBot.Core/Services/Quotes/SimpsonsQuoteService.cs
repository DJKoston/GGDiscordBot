using DiscordBot.DAL;
using DiscordBot.DAL.Models.Quotes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Quotes
{
    public interface ISimpsonsQuoteService
    {
        Task<SimpsonsQuotes> GetSimpsonsQuote();
    }

    public class SimpsonsQuoteService : ISimpsonsQuoteService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public SimpsonsQuoteService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task<SimpsonsQuotes> GetSimpsonsQuote()
        {
            using var context = new RPGContext(_options);

            var allQuotesCount = await context.SimpsonsQuotes.CountAsync();

            var rndm = new Random();
            var RandomQuoteID = rndm.Next(1, allQuotesCount + 1);

            return await context.SimpsonsQuotes.FirstOrDefaultAsync(x => x.Id == RandomQuoteID);
        }
    }
}
