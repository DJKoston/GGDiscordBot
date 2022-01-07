using DiscordBot.Core.ViewModels;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Profiles;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Profiles
{
    public interface IXPService
    {
        Task<GrantXpViewModel> GrantXpAsync(ulong discordId, ulong guildId, int xpAmount, string discordName);
    }

    public class XPService : IXPService
    {
        private readonly DbContextOptions<RPGContext> _options;
        private readonly IProfileService _profileService;
        private readonly IGoldService _goldService;

        public XPService(DbContextOptions<RPGContext> options, IProfileService profileService, IGoldService goldService)
        {
            _options = options;
            _profileService = profileService;
            _goldService = goldService;
        }

        public async Task<GrantXpViewModel> GrantXpAsync(ulong discordId, ulong guildId, int xpAmount, string discordName)
        {
            using var context = new RPGContext(_options);

            Profile profile = await _profileService.GetOrCreateProfileAsync(discordId, guildId, discordName).ConfigureAwait(false);

            int levelBefore = profile.Level;

            profile.XP += xpAmount;

            context.Profiles.Update(profile);

            await context.SaveChangesAsync().ConfigureAwait(false);

            int levelAfter = profile.Level;

            int goldGrant = profile.Level * 100;

            if (levelAfter > levelBefore)
            {
                await _goldService.GrantGoldAsync(discordId, guildId, goldGrant, discordName);
            }

            return new GrantXpViewModel
            {
                Profile = profile,
                LevelledUp = levelAfter > levelBefore
            };
        }
    }
}