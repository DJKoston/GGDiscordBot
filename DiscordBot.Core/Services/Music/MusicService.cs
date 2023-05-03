using DiscordBot.DAL;
using DiscordBot.DAL.Models.Games;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.Music
{
    public interface IMusicService
    {
        Task AddToDatabaseAsync(ulong guildId, string url, string songName);
        Task<string> GetQueueAsync(ulong guildId);
        Task ClearGuildSongsAsync(ulong guildId);
        Task<MusicPlaylist> GetNextSongAsync(ulong guildId);
        Task RemoveNextSongAsync(ulong guildId);
        Task RemoveAllSongsAsync();
        Task RemoveSpecificSong(MusicPlaylist song);
        List<MusicPlaylist> GetQueueList(ulong guildId);
    }

    public class MusicService : IMusicService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public MusicService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task AddToDatabaseAsync(ulong guildId, string url, string songName)
        {
            using var context = new RPGContext(_options);

            var songToAdd = new MusicPlaylist
            {
                GuildId = guildId,
                SongURI = url,
                SongTitle = songName
            };

            await context.AddAsync(songToAdd);

            await context.SaveChangesAsync();
        }

        public async Task ClearGuildSongsAsync(ulong guildId)
        {
            using var context = new RPGContext(_options);

            var guildSongs = context.MusicPlaylists.Where(x => x.GuildId == guildId);

            foreach (var song in guildSongs)
            {
                context.Remove(song);
            }

            await context.SaveChangesAsync();
        }

        public async Task<MusicPlaylist> GetNextSongAsync(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return await context.MusicPlaylists.FirstOrDefaultAsync(x => x.GuildId == guildId);
        }

        public async Task RemoveNextSongAsync(ulong guildId)
        {
            using var context = new RPGContext(_options);

            var songToDelete = await context.MusicPlaylists.FirstOrDefaultAsync(x => x.GuildId == guildId);

            context.Remove(songToDelete);

            await context.SaveChangesAsync();
        }

        public async Task RemoveAllSongsAsync()
        {
            using var context = new RPGContext(_options);

            var allSongs = context.MusicPlaylists.Where(x => x.GuildId != 0);

            foreach(var song in allSongs)
            {
                context.Remove(song);
            }

            await context.SaveChangesAsync();
        }


        public async Task<string> GetQueueAsync(ulong guildId)
        {
            using var context = new RPGContext(_options);

            var allSongs = context.MusicPlaylists.Where(x => x.GuildId == guildId);

            int count = 1;
            
            string queueListAsString = string.Empty;

            if (allSongs.Any())
            {
                foreach (var song in allSongs)
                {
                    queueListAsString += $"**#{count}** - [{song.SongTitle}]({song.SongURI})\n";
                    count++;
                }
            }

            else
            {
                queueListAsString = "No Songs were found in the Queue.";
            }

            return queueListAsString;
        }

        public async Task RemoveSpecificSong(MusicPlaylist song)
        {
            using var context = new RPGContext(_options);

            context.Remove(song);

            await context.SaveChangesAsync();
        }

        public List<MusicPlaylist> GetQueueList(ulong guildId)
        {
            using var context = new RPGContext(_options);

            return context.MusicPlaylists.Where(x => x.GuildId == guildId).ToList();
        }
    }
}
