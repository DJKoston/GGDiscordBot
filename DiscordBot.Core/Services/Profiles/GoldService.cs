using DiscordBot.DAL;
using DiscordBot.DAL.Models.Profiles;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Profiles
{
    public interface IGoldService
    {
        Task GrantGoldAsync(ulong discordId, ulong guildId, int goldAmount, string discordName);
    }

    public class GoldService : IGoldService
    {
        private readonly DbContextOptions<RPGContext> _options;
        private readonly IProfileService _profileService;

        public GoldService(DbContextOptions<RPGContext> options, IProfileService profileService)
        {
            _options = options;
            _profileService = profileService;
        }

        public async Task GrantGoldAsync(ulong discordId, ulong guildId, int goldAmount, string discordName)
        {
            using var context = new RPGContext(_options);

            Profile profile = await _profileService.GetOrCreateProfileAsync(discordId, guildId, discordName).ConfigureAwait(false);

            profile.Gold += goldAmount;

            context.Profiles.Update(profile);

            await context.SaveChangesAsync().ConfigureAwait(false);

        }
    }
}