using DiscordBot.DAL;
using DiscordBot.DAL.Models.Profiles;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Profiles
{

    public interface IProfileService
    {
        Task<Profile> GetOrCreateProfileAsync(ulong discordId, ulong guildId, string discordName);
        Task<Profile> DeleteProfileAsync(ulong discordId, ulong guildId, string discordName);
    }
    public class ProfileService : IProfileService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public ProfileService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task<Profile> GetOrCreateProfileAsync(ulong discordId, ulong guildId, string discordName)
        {
            using var context = new RPGContext(_options);

            var profile = await context.Profiles.Where(x => x.GuildId == guildId).Include(x => x.Items).Include(x => x.Items).ThenInclude(x => x.Item).FirstOrDefaultAsync(x => x.DiscordId == discordId).ConfigureAwait(false);

            if (profile != null) { return profile; }

            profile = new Profile
            {
                DiscordId = discordId,
                GuildId = guildId,
                DiscordName = discordName,
                XP = 1,
                Gold = 100
            };

            context.Add(profile);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return profile;
        }

        public async Task<Profile> DeleteProfileAsync(ulong discordId, ulong guildId, string discordName)
        {
            using var context = new RPGContext(_options);

            var profile = await context.Profiles.Where(x => x.GuildId == guildId).Include(x => x.Items).Include(x => x.Items).ThenInclude(x => x.Item).FirstOrDefaultAsync(x => x.DiscordId == discordId).ConfigureAwait(false);

            context.Remove(profile);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return profile;
        }
    }
}