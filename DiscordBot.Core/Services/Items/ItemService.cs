using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Items;
using DiscordBot.DAL.Models.Profiles;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Items
{
    public interface IItemService
    {
        Task CreateNewItemAsync(Item item);
        Task<Item> GetItemByName(string itemName);
        Task DeleteItemAsync(Item item);
        Task<bool> PurchaseItemAsync(ulong discordId, ulong guildId, string itemName, string discordName);
    }
    public class ItemService : IItemService
    {

        private readonly DbContextOptions<RPGContext> _options;
        private readonly IProfileService _profileService;

        public ItemService(DbContextOptions<RPGContext> options, IProfileService profileService)
        {
            _options = options;
            _profileService = profileService;
        }

        public async Task CreateNewItemAsync(Item item)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(item).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteItemAsync(Item item)
        {
            using var context = new RPGContext(_options);

            context.Remove(item);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Item> GetItemByName(string itemName)
        {
            using var context = new RPGContext(_options);

            itemName = itemName.ToLower();

            return await context.Items.FirstOrDefaultAsync(x => x.Name.ToLower() == itemName).ConfigureAwait(false);
        }

        public async Task<bool> PurchaseItemAsync(ulong discordId, ulong guildId, string itemName, string discordName)
        {
            using var context = new RPGContext(_options);

            Item item = await GetItemByName(itemName).ConfigureAwait(false);

            if (item == null) { return false; }

            Profile profile = await _profileService.GetOrCreateProfileAsync(discordId, guildId, discordName).ConfigureAwait(false);

            if (profile.Gold < item.Price) { return false; }

            profile.Gold -= item.Price;
            profile.Items.Add(new ProfileItem
            {
                ItemId = item.Id,
                ProfileId = profile.Id
            });

            context.Profiles.Update(profile);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }
    }
}
