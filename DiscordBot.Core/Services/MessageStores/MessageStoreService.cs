using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using DiscordBot.DAL.Models.MessageStores;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Configs
{
    public interface IMessageStoreService
    {
        Task CreateNewMessageStore(NowLiveMessages config);
        Task RemoveMessageStore(NowLiveMessages config);
        Task<NowLiveMessages> GetMessageStore(ulong GuildId, string streamerId);
    }
    public class MessageStoreService : IMessageStoreService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public MessageStoreService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewMessageStore(NowLiveMessages config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveMessageStore(NowLiveMessages config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<NowLiveMessages> GetMessageStore(ulong GuildId, string streamerId)
        {
            using var context = new RPGContext(_options);

            var streamer = context.NowLiveMessages.Where(x => x.GuildId == GuildId);

            return await streamer.FirstOrDefaultAsync(x => x.StreamerId == streamerId);
        }
    }
}
