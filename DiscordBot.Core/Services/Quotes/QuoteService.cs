using DiscordBot.DAL;
using DiscordBot.DAL.Models.Quotes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Quotes
{
    public interface IQuoteService
    {
        Task CreateNewQuoteAsync(Quote quote);
        Task DeleteQuoteAsync(Quote quote);
        Task<Quote> GetQuoteAsync(int quoteId);
    }

    public class QuoteService : IQuoteService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public QuoteService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewQuoteAsync(Quote quote)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(quote).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteQuoteAsync(Quote quote)
        {
            using var context = new RPGContext(_options);

            context.Remove(quote);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Quote> GetQuoteAsync(int quoteId)
        {
            using var context = new RPGContext(_options);

            return await context.Quotes.FirstOrDefaultAsync(x => x.QuoteId == quoteId).ConfigureAwait(false);
        }
    }
}
