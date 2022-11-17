using DiscordBot.DAL;
using DiscordBot.DAL.Models.NowLive;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.NowLive
{
    public interface INowLiveMessageService
    {
        Task CreateNewMessageStore(NowLiveMessage config);
        Task RemoveMessageStore(NowLiveMessage config);
        Task<NowLiveMessage> GetMessageStore(ulong GuildId, string streamerId);
        List<NowLiveMessage> GetAllMessages();
    }
    public class NowLiveMessageService : INowLiveMessageService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public NowLiveMessageService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewMessageStore(NowLiveMessage config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config);

            await context.SaveChangesAsync();
        }

        public async Task RemoveMessageStore(NowLiveMessage config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync();
        }

        public async Task<NowLiveMessage> GetMessageStore(ulong GuildId, string streamerId)
        {
            using var context = new RPGContext(_options);

            var streamer = context.NowLiveMessages.Where(x => x.GuildId == GuildId);

            return await streamer.FirstOrDefaultAsync(x => x.StreamerId == streamerId);
        }

        public List<NowLiveMessage> GetAllMessages()
        {
            using var context = new RPGContext(_options);

            var messages = context.NowLiveMessages.Where(x => x.StreamerId != null);

            var list = new List<NowLiveMessage> { };

            foreach (NowLiveMessage message in messages)
            {
                list.Add(message);
            }

            return list;
        }
    }
}
