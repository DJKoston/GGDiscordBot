using DiscordBot.DAL;
using DiscordBot.DAL.Models.Egg;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Egg
{
    public interface IEggService
    {
        Task ChannelAddStore(EggChannel channel);
        Task RoleAddStore(EggRole role);
        Task NicknameAddStore(EggNickname nickname);
        Task<EggChannel> ChannelFind(ulong guildId, ulong channelId);
        Task<EggRole> RoleFind(ulong guildId, ulong roleId);
        Task<EggNickname> NicknameFind(ulong guildId, ulong userId);
        Task ChannelDelete(EggChannel channel);
        Task RoleDelete(EggRole role);
        Task NicknameDelete(EggNickname nickname);
    }

    public class EggService : IEggService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public EggService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task ChannelAddStore(EggChannel channel)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(channel);

            await context.SaveChangesAsync();
        }

        public async Task RoleAddStore(EggRole role)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(role);

            await context.SaveChangesAsync();
        }

        public async Task NicknameAddStore(EggNickname nickname)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(nickname);

            await context.SaveChangesAsync();
        }

        public async Task<EggChannel> ChannelFind(ulong guildId, ulong channelId)
        {
            using var context = new RPGContext(_options);

            return await context.EggChannels.FirstOrDefaultAsync(x => x.GuildId == guildId && x.ChannelId == channelId);
        }

        public async Task<EggRole> RoleFind(ulong guildId, ulong roleId)
        {
            using var context = new RPGContext(_options);

            return await context.EggRoles.FirstOrDefaultAsync(x => x.GuildId == guildId && x.RoleId == roleId);
        }

        public async Task<EggNickname> NicknameFind(ulong guildId, ulong userId)
        {
            using var context = new RPGContext(_options);

            return await context.EggNicknames.FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);
        }

        public async Task ChannelDelete(EggChannel channel)
        {
            using var context = new RPGContext(_options);

            context.Remove(channel);

            await context.SaveChangesAsync();
        }

        public async Task RoleDelete(EggRole role)
        {
            using var context = new RPGContext(_options);

            context.Remove(role);

            await context.SaveChangesAsync();
        }

        public async Task NicknameDelete(EggNickname nickname)
        {
            using var context = new RPGContext(_options);

            context.Remove(nickname);

            await context.SaveChangesAsync();
        }
    }
}