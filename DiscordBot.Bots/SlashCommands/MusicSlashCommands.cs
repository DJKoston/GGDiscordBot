using DiscordBot.Core.Services.Music;
using DSharpPlus.Lavalink;
using TwitchLib.Api.Helix;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace DiscordBot.Bots.SlashCommands
{
    public class MusicSlashCommands : ApplicationCommandModule
    {
        private readonly IMusicService _musicService;

        public MusicSlashCommands(IMusicService musicService)
        {
            _musicService = musicService;
        }

        [SlashCommand("play", "play a song")]
        public async Task PlaySong(InteractionContext ctx, [Option("Song", "Insert URL or Search Term")] string songSearch)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync("The Lavalink connection is not established", true);
                return;
            }

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync("You are not in a voice channel.", false);
                return;
            }

            await node.ConnectAsync(ctx.Member.VoiceState.Channel);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync("Lavalink is not connected.");
                return;
            }

            var nextSong = await _musicService.GetNextSongAsync(ctx.Guild.Id);

            if (nextSong == null && conn.CurrentState.CurrentTrack == null)
            {
                LavalinkLoadResult loadResult = null;

                if (songSearch.StartsWith("https://") || songSearch.StartsWith("http://"))
                {
                    var songUri = new Uri(songSearch);

                    loadResult = await node.Rest.GetTracksAsync(songUri);
                }

                else
                {
                    loadResult = await node.Rest.GetTracksAsync(songSearch);
                }

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    await ctx.CreateResponseAsync($"Track search failed for {songSearch}.");
                    return;
                }

                var track = loadResult.Tracks.First();

                await conn.SetVolumeAsync(10);
                await conn.PlayAsync(track);

                await ctx.CreateResponseAsync($"Now playing {track.Title}!");
            }

            else
            {
                LavalinkLoadResult loadResult = null;

                if (songSearch.StartsWith("https://") || songSearch.StartsWith("http://"))
                {
                    var songUri = new Uri(songSearch);

                    loadResult = await node.Rest.GetTracksAsync(songUri);
                }

                else
                {
                    loadResult = await node.Rest.GetTracksAsync(songSearch);
                }

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    await ctx.CreateResponseAsync($"Track search failed for {songSearch}.");
                    return;
                }

                var track = loadResult.Tracks.First();

                await _musicService.AddToDatabaseAsync(ctx.Guild.Id, track.Uri.ToString(), track.Title);

                await ctx.CreateResponseAsync($"Added {track.Title} to Queue");
                return;
            }
        }

        [SlashCommand("skip", "skip this song")]
        public async Task SkipSong(InteractionContext ctx)
        {
            var nextSong = await _musicService.GetNextSongAsync(ctx.Guild.Id);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (nextSong == null) { await ctx.CreateResponseAsync("There are no other songs in the queue, I will now disconnect."); await conn.DisconnectAsync(); return; }

            await conn.StopAsync();

            await ctx.CreateResponseAsync($"Skipping Song, Now Playing: {nextSong.SongTitle}");
        }

        [SlashCommand("stop", "stops playing the current queue and clear it.")]
        public async Task StopQueue (InteractionContext ctx)
        {
            await _musicService.ClearGuildSongsAsync(ctx.Guild.Id);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            await conn.StopAsync();

            await ctx.CreateResponseAsync($"Stopping Player, See yah!");
        }

        [SlashCommand("pause", "pauses the music player.")]
        public async Task PauseSong(InteractionContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
            await ctx.CreateResponseAsync("Player is now Paused, run /resume to resume playback.");
        }

        [SlashCommand("resume", "resumes the music player.")]
        public async Task ResumeSong(InteractionContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync("There are no tracks loaded.");
                return;
            }

            await conn.ResumeAsync();
            await ctx.CreateResponseAsync($"Player has now resumed playing: {conn.CurrentState.CurrentTrack.Title}");
        }

        [SlashCommand("queue", "pull up the current song queue")]
        public async Task QueueList(InteractionContext ctx)
        {
            var queueList = await _musicService.GetQueueAsync(ctx.Guild.Id);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Yellow,
                Title = "GG-Bot Music Queue",
                Description = $"**Now Playing:** [{conn.CurrentState.CurrentTrack.Title}]({conn.CurrentState.CurrentTrack.Uri})\n\n" + queueList
            }.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

            var builder = new DiscordInteractionResponseBuilder();
            builder.AddEmbed(embed);

            await ctx.CreateResponseAsync(builder);
        }

        [SlashCommand("remove", "remove a song by its place in the queue")]
        public async Task RemoveSong(InteractionContext ctx, [Option("Position", "Position in the Queue for the song to Remove.")] double position)
        {
            int positionInt = Convert.ToInt32(position);

            int positionToRemove = positionInt - 1;

            var songToRemove = _musicService.GetQueueList(ctx.Guild.Id)[positionToRemove];

            await _musicService.RemoveSpecificSong(songToRemove);

            await ctx.CreateResponseAsync($"Removed Song: {songToRemove.SongTitle} from Queue.");
        }

        [SlashCommand("volume", "set the volume")]
        public async Task SetVolume(InteractionContext ctx, [Option("volume", "Volume out of 100.")] double volume)
        {
            int volumeInt = Convert.ToInt32(volume);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            await conn.SetVolumeAsync(volumeInt);

            await ctx.CreateResponseAsync($"Player Volume set to: {volume}%");
        }
    }
}
