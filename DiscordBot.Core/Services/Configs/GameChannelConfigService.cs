﻿using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Configs
{
    public interface IGameChannelConfigService
    {
        Task CreateGameChannelConfigService(GameChannelConfig config);
        Task RemoveGameChannelConfigService(GameChannelConfig config);
        Task<GameChannelConfig> GetGameChannelConfigService(ulong GuildId);
        List<GameChannelConfig> GetGameChannelConfigs();
    }
    public class GameChannelConfigService : IGameChannelConfigService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public GameChannelConfigService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateGameChannelConfigService(GameChannelConfig config)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(config).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveGameChannelConfigService(GameChannelConfig config)
        {
            using var context = new RPGContext(_options);

            context.Remove(config);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<GameChannelConfig> GetGameChannelConfigService(ulong GuildId)
        {
            using var context = new RPGContext(_options);

            return await context.GameChannelConfigs.FirstOrDefaultAsync(x => x.GuildId == GuildId);
        }

        public List<GameChannelConfig> GetGameChannelConfigs()
        {
            using var context = new RPGContext(_options);

            var gameChannels = context.GameChannelConfigs.Where(x => x.Id > 0);

            var list = new List<GameChannelConfig> {  };

            foreach (GameChannelConfig config in gameChannels)
            {
                list.Add(config);
            }

            return list;
        }
    }
}
