using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Configs
{
    public interface IGuildStreamerConfigService
    {
        Task CreateNewGuildStreamerConfig(GuildStreamerConfig config);
        Task RemoveGuildStreamerConfig(GuildStreamerConfig config);
        List<string> GetGuildStreamerList();
        List<GuildStreamerConfig> GetGuildStreamerConfig(string streamerName);
        Task<GuildStreamerConfig> GetConfigToDelete(ulong GuildId, string twitchName);
        List<GuildStreamerConfig> GetAllStreamers();
        Task EditUser(GuildStreamerConfig config);
    }
    public class GuildStreamerConfigService : IGuildStreamerConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public GuildStreamerConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewGuildStreamerConfig(GuildStreamerConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveGuildStreamerConfig(GuildStreamerConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public List<string> GetGuildStreamerList()
        {
            using var context = new RPGContext(_options);

            var streamers = context.GuildStreamerConfigs.Where(x => x.StreamerId != null);

            var list = new List<string> { };

            foreach (GuildStreamerConfig streamer in streamers)
            {
                list.Add(streamer.StreamerId);
            }

            return list;
        }

        public List<GuildStreamerConfig> GetGuildStreamerConfig(string streamerName)
        {
            using var context = new RPGContext(_options);

            var streamers = context.GuildStreamerConfigs.Where(x => x.StreamerId == streamerName);

            var list = new List<GuildStreamerConfig> { };

            foreach (GuildStreamerConfig streamer in streamers)
            {
                list.Add(streamer);
            }

            return list;
        }

        public async Task<GuildStreamerConfig> GetConfigToDelete(ulong GuildId, string twitchName)
        {
            using var context = new RPGContext(_options);

            return await context.GuildStreamerConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId && x.StreamerId == twitchName);
        }

        public List<GuildStreamerConfig> GetAllStreamers()
        {
            using var context = new RPGContext(_options);

            var streamers = context.GuildStreamerConfigs.Where(x => x.StreamerId != null);

            var list = new List<GuildStreamerConfig> { };

            foreach (GuildStreamerConfig streamer in streamers)
            {
                list.Add(streamer);
            }

            return list;
        }

        public async Task EditUser(GuildStreamerConfig config)
        {
            using var context = new RPGContext(_options);

            context.Update(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
