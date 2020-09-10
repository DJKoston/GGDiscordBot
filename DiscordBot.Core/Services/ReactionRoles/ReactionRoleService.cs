using DiscordBot.DAL;
using DiscordBot.DAL.Models.ReactionRoles;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.ReactionRoles
{
    public interface IReactionRoleService
    {
        Task CreateNewReactionRole(ReactionRole reactionRole);
        Task<ReactionRole> GetReactionRole(ulong GuildId, ulong ChannelId, ulong MessageId, ulong EmoteId, string EmoteName);
        Task DeleteReactionRole(ReactionRole reactionRole);
    }

    public class ReactionRoleService : IReactionRoleService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public ReactionRoleService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewReactionRole(ReactionRole reactionRole)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(reactionRole);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteReactionRole(ReactionRole reactionRole)
        {
            using var context = new RPGContext(_options);

            context.Remove(reactionRole);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<ReactionRole> GetReactionRole(ulong GuildId, ulong ChannelId, ulong MessageId, ulong EmoteId, string EmoteName)
        {
            using var context = new RPGContext(_options);

            var GuildReactionRoles = context.ReactionRoles.Where(x => x.GuildId == GuildId);

            var ChannelReactionRoles = GuildReactionRoles.Where(x => x.ChannelId == ChannelId);

            var MessageReactionRoles = ChannelReactionRoles.Where(x => x.MessageId == MessageId);

            return await MessageReactionRoles.FirstOrDefaultAsync(x => x.UnicodeEmote == EmoteName).ConfigureAwait(false);              
        }
    }
}
