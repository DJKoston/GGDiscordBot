using DiscordBot.DAL;
using DiscordBot.DAL.Models.NowLive;
using Microsoft.EntityFrameworkCore;


namespace DiscordBot.Core.Services.NowLive
{
    public interface INowLiveStreamerService
    {
        Task CreateNewNowLiveStreamer(NowLiveStreamer config);
        Task RemoveNowLiveStreamer(NowLiveStreamer config);
        List<string> GetNowLiveStreamerList();
        List<NowLiveStreamer> GetNowLiveStreamer(string streamerName);
        Task<NowLiveStreamer> GetStreamerToDelete(ulong GuildId, string twitchName);
        List<NowLiveStreamer> GetAllStreamers();
        Task EditNowLiveStreamer(NowLiveStreamer config);
    }
    public class NowLiveStreamerService : INowLiveStreamerService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public NowLiveStreamerService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewNowLiveStreamer(NowLiveStreamer config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveNowLiveStreamer(NowLiveStreamer config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public List<string> GetNowLiveStreamerList()
        {
            using var context = new RPGContext(_options);

            var streamers = context.NowLiveStreamers.Where(x => x.StreamerId != null);

            var list = new List<string> { };

            foreach (NowLiveStreamer streamer in streamers)
            {
                if (list.Contains(streamer.StreamerId)) { continue; }

                list.Add(streamer.StreamerId);
            }

            return list;
        }

        public List<NowLiveStreamer> GetNowLiveStreamer(string streamerName)
        {
            using var context = new RPGContext(_options);

            var streamers = context.NowLiveStreamers.Where(x => x.StreamerId == streamerName);

            var list = new List<NowLiveStreamer> { };

            foreach (NowLiveStreamer streamer in streamers)
            {
                list.Add(streamer);
            }

            return list;
        }

        public async Task<NowLiveStreamer> GetStreamerToDelete(ulong GuildId, string twitchName)
        {
            using var context = new RPGContext(_options);

            return await context.NowLiveStreamers.FirstOrDefaultAsync(x => x.GuildId == GuildId && x.StreamerId == twitchName);
        }

        public List<NowLiveStreamer> GetAllStreamers()
        {
            using var context = new RPGContext(_options);

            var streamers = context.NowLiveStreamers.Where(x => x.StreamerId != null);

            var list = new List<NowLiveStreamer> { };

            foreach (NowLiveStreamer streamer in streamers)
            {
                list.Add(streamer);
            }

            return list;
        }

        public async Task EditNowLiveStreamer(NowLiveStreamer config)
        {
            using var context = new RPGContext(_options);

            context.Update(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}