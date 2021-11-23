using DiscordBot.DAL;
using DiscordBot.DAL.Models.Suggestions;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Suggestions
{
    public interface ISuggestionService
    {
        Task CreateNewSuggestion(Suggestion suggestion);
        Task<Suggestion> GetSuggestion(ulong GuildId, int Id);
        Task DeleteSuggestion(Suggestion suggestion);
        Task EditSuggestion(Suggestion suggestion);
    }

    public class SuggestionService : ISuggestionService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public SuggestionService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewSuggestion(Suggestion suggestion)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(suggestion);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task EditSuggestion(Suggestion suggestion)
        {
            using var context = new RPGContext(_options);

            context.Update(suggestion);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteSuggestion(Suggestion suggestion)
        {
            using var context = new RPGContext(_options);

            context.Remove(suggestion);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Suggestion> GetSuggestion(ulong GuildId, int Id)
        {
            using var context = new RPGContext(_options);

            var guildContext = context.Suggestions.Where(x => x.GuildId == GuildId);

            return await guildContext.FirstOrDefaultAsync(x => x.Id == Id).ConfigureAwait(false);
        }
    }
}