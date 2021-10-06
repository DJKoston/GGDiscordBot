using DiscordBot.DAL;
using DiscordBot.DAL.Models.Playlists;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Music
{
    public interface IMusicService
    {
        Task AddToPlaylist(Playlist playlist);
        Task ClearGuildPlaylist(ulong guildId);
        Task ClearAllPlaylists();
        Task RemoveLastSongFromPlaylist(ulong guildId);
        Task<Playlist> GetNextGuildSong(ulong guildId);
        Task<Playlist> RemoveSpecificSong(ulong guildId, int songNumber);
    }

    public class MusicService : IMusicService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public MusicService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddToPlaylist(Playlist playlist)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(playlist);

             context.SaveChanges();
        }

        public async Task ClearGuildPlaylist(ulong guildId)
        {
            using var context = new RPGContext(_options);

            var guildPlaylists = context.Playlists.Where(x => x.GuildId == guildId);

            foreach(Playlist playlist in guildPlaylists)
            {
                context.Remove(playlist);
            }

            await context.SaveChangesAsync();
        }

        public async Task ClearAllPlaylists()
        {
            using var context = new RPGContext(_options);

            var allPlaylists = context.Playlists.Where(x => x.Id > 0);

            foreach(Playlist playlist in allPlaylists)
            {
                context.Remove(playlist);
            }

            await context.SaveChangesAsync();
        }

        public async Task RemoveLastSongFromPlaylist(ulong guildId)
        {
            using var context = new RPGContext(_options);

            var first = await context.Playlists.FirstOrDefaultAsync(x => x.GuildId == guildId);

            context.Remove(first);

            await context.SaveChangesAsync();
        }

        public async Task<Playlist> GetNextGuildSong(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return await context.Playlists.FirstOrDefaultAsync(x => x.GuildId == guildId);
        }

        public async Task<Playlist> RemoveSpecificSong(ulong guildId, int songNumber)
        {
            using var context = new RPGContext(_options);

            var first = await context.Playlists.FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == songNumber);

            context.Remove(first);

            await context.SaveChangesAsync();

            return first;
        }
    }
}
