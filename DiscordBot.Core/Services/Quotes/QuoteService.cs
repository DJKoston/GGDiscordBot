using DiscordBot.DAL;
using DiscordBot.DAL.Models.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Quotes
{
    public interface IQuoteService
    {
        Task CreateNewQuoteAsync(Quote quote);
        Task DeleteQuoteAsync(Quote quote);
        Task<Quote> GetQuoteAsync(int quoteId, ulong discordId);
        List<Quote> GetGuildQuotes(ulong guildId);
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

        public async Task<Quote> GetQuoteAsync(int quoteId, ulong discordId)
        {
            using var context = new RPGContext(_options);

            var serverQuotes = context.Quotes.Where(x => x.GuildId == discordId);

            return await serverQuotes.FirstOrDefaultAsync(x => x.QuoteId == quoteId).ConfigureAwait(false);
        }

        public List<Quote> GetGuildQuotes(ulong guildId)
        {
            using var context = new RPGContext(_options);

            var list = new List<Quote>();

            var guildQuotes = context.Quotes.Where(x => x.GuildId == guildId);

            foreach (Quote quote in guildQuotes)
            {
                list.Add(quote);
            }

            return list;
        }
    }
}