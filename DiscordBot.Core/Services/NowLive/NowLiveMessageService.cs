using DiscordBot.DAL;
using DiscordBot.DAL.Models.NowLive;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Configs
{
    public interface INowLiveMessageService
    {
        Task CreateNewMessageStore(NowLiveMessage config);
        Task RemoveMessageStore(NowLiveMessage config);
        Task<NowLiveMessage> GetMessageStore(ulong GuildId, string streamerId);
    }
    public class MessageStoreService : INowLiveMessageService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public MessageStoreService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewMessageStore(NowLiveMessage config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveMessageStore(NowLiveMessage config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<NowLiveMessage> GetMessageStore(ulong GuildId, string streamerId)
        {
            using var context = new RPGContext(_options);

            var streamer = context.NowLiveMessages.Where(x => x.GuildId == GuildId);

            return await streamer.FirstOrDefaultAsync(x => x.StreamerId == streamerId);
        }
    }
}
