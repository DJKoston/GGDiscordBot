using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        [Group("connect")]
        public class RadioCommands : BaseCommandModule
        {
            [Command("simulatorradio")]
            public async Task SimulatorRadio(CommandContext ctx)
            {
                Uri simRadio = new Uri("https://simulatorradio.stream/stream.mp3");

                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await ctx.RespondAsync("You are not in a voice channel.");
                    return;
                }

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn == null)
                {
                    await ctx.RespondAsync("Lavalink is not connected.");
                    return;
                }

                var loadResult = await node.Rest.GetTracksAsync(simRadio);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                    || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    Console.WriteLine("Can't load Simulator Radio");
                    return;
                }

                var track = loadResult.Tracks.First();

                await conn.PlayAsync(track);

                await ctx.RespondAsync($"Now playing Simulator Radio!");
            }

            [Command("radio1")]
            public async Task RadioOne(CommandContext ctx)
            {
                Uri simRadio = new Uri("http://stream.live.vc.bbcmedia.co.uk/bbc_radio_one");

                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await ctx.RespondAsync("You are not in a voice channel.");
                    return;
                }

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn == null)
                {
                    await ctx.RespondAsync("Lavalink is not connected.");
                    return;
                }

                var loadResult = await node.Rest.GetTracksAsync(simRadio);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                    || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    Console.WriteLine("Can't load BBC Radio 1");
                    return;
                }

                var track = loadResult.Tracks.First();

                await conn.PlayAsync(track);

                await ctx.RespondAsync($"Now playing BBC Radio 1!");
            }

            [Command("lincsfm")]
            public async Task LincsFM(CommandContext ctx)
            {
                Uri simRadio = new Uri("https://stream-mz.planetradio.co.uk/net1lincoln.aac?direct=true");

                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await ctx.RespondAsync("You are not in a voice channel.");
                    return;
                }

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn == null)
                {
                    await ctx.RespondAsync("Lavalink is not connected.");
                    return;
                }

                var loadResult = await node.Rest.GetTracksAsync(simRadio);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                    || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    Console.WriteLine("Can't load Lincs FM");
                    return;
                }

                var track = loadResult.Tracks.First();

                await conn.PlayAsync(track);

                await ctx.RespondAsync($"Now playing Lincs FM!");
            }
        }
    }
}
