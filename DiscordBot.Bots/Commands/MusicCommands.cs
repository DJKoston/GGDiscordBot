using DiscordBot.Core.Services.Music;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Playlists;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        private readonly RPGContext _context;
        private readonly IMusicService _musicService;

        public MusicCommands(RPGContext context, IMusicService musicService)
        {
            _context = context;
            _musicService = musicService;
        }

        [Command("skip")]
        public async Task SkipSong (CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var con = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            con.PlaybackFinished -= this.LavalinkNode_TrackFinished;

            var currentTrack = con.CurrentState.CurrentTrack;

            await _musicService.RemoveLastSongFromPlaylist(con.Guild.Id);

            var playlist = await _musicService.GetNextGuildSong(con.Guild.Id);

            if (playlist == null)
            {
                await con.DisconnectAsync();

                await ctx.RespondAsync("There was no more songs in the server playlist, so i have disconnected from the VC.");

                return;
            };

            var loadResult = await con.GetTracksAsync(playlist.TrackURL);

            var track = loadResult.Tracks.First();

            await con.PlayAsync(track);

            await ctx.RespondAsync($"Skipped {currentTrack.Title}\nNow Playing {track.Title}");

            con.PlaybackFinished += this.LavalinkNode_TrackFinished;
        }

        [Command("removesong")]
        public async Task RemoveSong(CommandContext ctx, int songNumber)
        {
            var song = await _musicService.RemoveSpecificSong(ctx.Guild.Id, songNumber);

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var loadResult = await node.Rest.GetTracksAsync(song.TrackURL);

            var track = loadResult.Tracks.First();

            await ctx.RespondAsync($"Removed {track.Title} from queue.");
        }

        [Command("resume")]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.ResumeAsync();
        }

        [Command("simulatorradio")]
        public async Task SimulatorRadio(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (ctx.Member.VoiceState == null)
            {
                await ctx.RespondAsync("You are not in a VC. Please connect to one before running this command again.");
                return;
            }

            var channel = ctx.Member.VoiceState.Channel;

            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (botMember.VoiceState == null || botMember.VoiceState.Channel == null)
            {
                await node.ConnectAsync(channel);
            }

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            Uri srURL = new Uri("https://simulatorradio.stream/stream.mp3");

            var loadResult = await node.Rest.GetTracksAsync(srURL);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Simulator Radio could not be played at this time.");
                return;
            }

            var track = loadResult.Tracks.First();

            var playlist = await _musicService.GetNextGuildSong(ctx.Guild.Id);

            if (playlist == null)
            {
                await conn.PlayAsync(track);

                await ctx.RespondAsync($"Now playing Simulator Radio!");

                conn.PlaybackFinished += this.LavalinkNode_TrackFinished;
                conn.TrackException += this.LavalinkNode_TrackError;
            }
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, Uri url)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (ctx.Member.VoiceState == null)
            {
                await ctx.RespondAsync("You are not in a VC. Please connect to one before running this command again.");
                return;
            }

            var channel = ctx.Member.VoiceState.Channel;

            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (botMember.VoiceState == null || botMember.VoiceState.Channel == null)
            {
                await node.ConnectAsync(channel);
            }

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var loadResult = await node.Rest.GetTracksAsync(url);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {url}.");
                return;
            }

            var track = loadResult.Tracks.First();

            var playlist = await _musicService.GetNextGuildSong(ctx.Guild.Id);

            if(playlist == null)
            {
                var newSong = new Playlist();

                newSong.GuildId = ctx.Guild.Id;
                newSong.RequestorId = ctx.Member.Id;
                newSong.TrackURL = track.Uri;

                await _musicService.AddToPlaylist(newSong);

                await conn.PlayAsync(track);

                await ctx.RespondAsync($"Now playing {track.Title}!");

                conn.PlaybackFinished += this.LavalinkNode_TrackFinished;
                conn.TrackException += this.LavalinkNode_TrackError;
            }

            else if(playlist != null)
            {
                var newSong = new Playlist();

                newSong.GuildId = ctx.Guild.Id;
                newSong.RequestorId = ctx.Member.Id;
                newSong.TrackURL = track.Uri;

                await _musicService.AddToPlaylist(newSong);

                await ctx.RespondAsync($"Added: {track.Title} to the server playlist. To remove the song type `!removesong {newSong.Id}`");
            }
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (ctx.Member.VoiceState == null)
            {
                await ctx.RespondAsync("You are not in a VC. Please connect to one before running this command again.");
                return;
            }

            var channel = ctx.Member.VoiceState.Channel;

            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (botMember.VoiceState == null || botMember.VoiceState.Channel == null)
            {
                await node.ConnectAsync(channel);
            }

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            var playlist = await _musicService.GetNextGuildSong(ctx.Guild.Id);

            if (conn.CurrentState.CurrentTrack != null)
            {
                var newSong = new Playlist();

                newSong.GuildId = ctx.Guild.Id;
                newSong.RequestorId = ctx.Member.Id;
                newSong.TrackURL = track.Uri;

                await _musicService.AddToPlaylist(newSong);

                await ctx.RespondAsync($"Added: {track.Title} to the server playlist. To remove the song type `!removesong {newSong.Id}`");

                return;
            }

            if (playlist == null)
            {
                var newSong = new Playlist();

                newSong.GuildId = ctx.Guild.Id;
                newSong.RequestorId = ctx.Member.Id;
                newSong.TrackURL = track.Uri;

                await _musicService.AddToPlaylist(newSong);

                await conn.PlayAsync(track);

                await ctx.RespondAsync($"Now playing {track.Title}!");

                conn.PlaybackFinished += this.LavalinkNode_TrackFinished;
                conn.TrackException += this.LavalinkNode_TrackError;

                return;
            }

            else if (playlist != null)
            {
                var newSong = new Playlist();

                newSong.GuildId = ctx.Guild.Id;
                newSong.RequestorId = ctx.Member.Id;
                newSong.TrackURL = track.Uri;

                await _musicService.AddToPlaylist(newSong);

                await ctx.RespondAsync($"Added: {track.Title} to the server playlist. To remove the song type `!removesong {newSong.Id}`");

                return;
            }
        }

        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            var node = lava.ConnectedNodes.Values.First();

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            conn.PlaybackFinished -= this.LavalinkNode_TrackFinished;
            conn.TrackException -= this.LavalinkNode_TrackError;

            await conn.StopAsync();
            await conn.DisconnectAsync();

            await _musicService.ClearGuildPlaylist(ctx.Guild.Id);

            await ctx.RespondAsync("I have stopped all music, disconnected and cleaned up the server playlist!");
        }

        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
        }

        private async Task LavalinkNode_TrackError(LavalinkGuildConnection con, TrackExceptionEventArgs e)
        {
            con.TrackException -= this.LavalinkNode_TrackError;

            await _musicService.RemoveLastSongFromPlaylist(con.Guild.Id);

            var playlist = await _musicService.GetNextGuildSong(con.Guild.Id);

            if (playlist == null)
            {
                await con.DisconnectAsync();

                return;
            };

            var loadResult = await con.GetTracksAsync(playlist.TrackURL);

            var track = loadResult.Tracks.First();

            await con.PlayAsync(track);

            con.TrackException += this.LavalinkNode_TrackError;
        }

        private async Task LavalinkNode_TrackFinished(LavalinkGuildConnection con, TrackFinishEventArgs e)
        {
            con.PlaybackFinished -= this.LavalinkNode_TrackFinished;

            await _musicService.RemoveLastSongFromPlaylist(con.Guild.Id);

            var playlist = await _musicService.GetNextGuildSong(con.Guild.Id);

            if(playlist == null)
            {
                await con.DisconnectAsync();

                return;
            };

            var loadResult = await con.GetTracksAsync(playlist.TrackURL);

            var track = loadResult.Tracks.First();

            await con.PlayAsync(track);

            con.PlaybackFinished += this.LavalinkNode_TrackFinished;
        }

    }
}
