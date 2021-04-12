using DiscordBot.Core.Services.Radios;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Radios;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        private readonly IRadioService _radioService;
        private readonly RPGContext _context;

        public MusicCommands(RPGContext context, IRadioService radioService)
        {
            _context = context;
            _radioService = radioService;
        }

        [Command("volume")]
        [Aliases("v")]
        public async Task Volume(CommandContext ctx, int volume)
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

            await conn.SetVolumeAsync(volume);

            await ctx.RespondAsync($"Volume set to {volume}");
        }

        [Command("connect")]
        [Aliases("radio")]
        public async Task RadioConnect(CommandContext ctx)
        {
            var currentStore = await _radioService.GetRadioAsync(ctx.Guild.Id);

            if (currentStore != null) { await _radioService.DeleteRadioAsync(currentStore); }

            var emoteOne = DiscordEmoji.FromName(ctx.Client, ":one:");
            var emoteTwo = DiscordEmoji.FromName(ctx.Client, ":two:");
            var emoteThree = DiscordEmoji.FromName(ctx.Client, ":three:");

            var connectionEmbed = new DiscordEmbedBuilder
            {
                Title = "React below to select a Radio Station!",
                Description = $"{emoteOne}: BBC Radio One\n\n" +
                $"{emoteTwo}: Simulator Radio\n\n" +
                $"{emoteThree}: Lincs FM",
                Color = DiscordColor.SpringGreen
            };

            var connectMessage = await ctx.Channel.SendMessageAsync(embed: connectionEmbed).ConfigureAwait(false);

            await connectMessage.CreateReactionAsync(emoteOne).ConfigureAwait(false);
            await connectMessage.CreateReactionAsync(emoteTwo).ConfigureAwait(false);
            await connectMessage.CreateReactionAsync(emoteThree).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == connectMessage && x.User == ctx.User && (x.Emoji == emoteOne || x.Emoji == emoteTwo || x.Emoji == emoteThree));

            Uri radioURL = null;
            string radioName = null;

            if (reactionResult.Result.Emoji == emoteOne)
            {
                radioURL = new Uri("http://stream.live.vc.bbcmedia.co.uk/bbc_radio_one");
                radioName = "BBC Radio One";
            }

            if (reactionResult.Result.Emoji == emoteTwo)
            {
                radioURL = new Uri("https://simulatorradio.stream/stream.mp3");
                radioName = "Simulator Radio";
            }

            if (reactionResult.Result.Emoji == emoteThree)
            {
                radioURL = new Uri("https://stream-mz.planetradio.co.uk/net1lincoln.aac?direct=true");
                radioName = "Lincs FM";
            }

            if (reactionResult.Result.Emoji == emoteOne || reactionResult.Result.Emoji == emoteTwo || reactionResult.Result.Emoji == emoteThree)
            {
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

                var loadResult = await node.Rest.GetTracksAsync(radioURL);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                    || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    await ctx.RespondAsync($"Can't load {radioName}");
                    return;
                }

                var track = loadResult.Tracks.First();

                await conn.SetVolumeAsync(25);
                await conn.PlayAsync(track);

                await ctx.Message.DeleteAsync().ConfigureAwait(false);
                await connectMessage.DeleteAsync().ConfigureAwait(false);

                await ctx.RespondAsync($"Now playing {radioName}!");

                var newRadioStore = new Radio
                {
                    GuildId = conn.Guild.Id,
                    ChannelId = conn.Channel.Id,
                    RadioURL = radioURL.ToString(),
                };

                await _radioService.CreateNewRadioAsync(newRadioStore);
            }
        }
    }
}
