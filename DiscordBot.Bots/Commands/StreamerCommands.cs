using DiscordBot.Core.Services.CommunityStreamers;
using DiscordBot.Core.Services.Configs;
using DiscordBot.Core.Services.Suggestions;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
using DiscordBot.DAL.Models.ReactionRoles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [Group("streamer")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    public class StreamerCommands : BaseCommandModule
    {
        private readonly RPGContext _context;
        private readonly ICommunityStreamerService _communityStreamerService;
        private readonly IGuildStreamerConfigService _guildStreamerConfigService;

        public StreamerCommands(RPGContext context, ICommunityStreamerService communityStreamerService, IGuildStreamerConfigService guildStreamerConfigService)
        {
            _context = context;
            _communityStreamerService = communityStreamerService;
            _guildStreamerConfigService = guildStreamerConfigService;
        }

        [Command("approve")]
        public async Task ApproveStreamer(CommandContext ctx, int suggestionId, DiscordChannel announceChannel)
        {
            var suggestion = await _communityStreamerService.GetStreamer(ctx.Guild.Id, suggestionId);

            if (suggestion == null) { await ctx.Channel.SendMessageAsync("This ID does not exist. Please try again.").ConfigureAwait(false); return; }

            if (suggestion.DealtWith == "APPROVED") { await ctx.Channel.SendMessageAsync("This request has already been approved!").ConfigureAwait(false); return; };

            if (suggestion.DealtWith == "DENIED") { await ctx.Channel.SendMessageAsync("This request has already been rejected!").ConfigureAwait(false); return; };

            DiscordMember suggestor = await ctx.Guild.GetMemberAsync(suggestion.requestorId);

            if(suggestor != null) { await suggestor.SendMessageAsync($"Your Request to add `{suggestion.streamerName}` to the Now Live bot has been approved in the {ctx.Guild.Name} server!"); }

            var config = new GuildStreamerConfig
            {
                AnnounceChannelId = announceChannel.Id,
                GuildId = ctx.Guild.Id,
                StreamerId = suggestion.streamerName,
                AnnouncementMessage = "%USER% has gone live streaming %GAME%! You should check them out over at: %URL%",
            };

            await _guildStreamerConfigService.CreateNewGuildStreamerConfig(config);

            suggestion.DealtWith = "APPROVED";

            await _communityStreamerService.EditStreamer(suggestion);

            DiscordChannel channel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "streamers-to-approve");

            DiscordMessage message = await channel.GetMessageAsync(suggestion.RequestMessage);

            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = $"Request for streamer to be added to the bot by: {suggestor.DisplayName}",
                Description = suggestion.streamerName,
                Color = DiscordColor.Green,
            };

            suggestionEmbed.AddField("This request Was Approved by:", $"{ctx.Member.Mention}");

            suggestionEmbed.WithFooter($"Request: {suggestion.Id}");

            DiscordEmbed newEmbed = suggestionEmbed;

            await message.ModifyAsync(embed: newEmbed).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"Request {suggestion.Id} has been approved!").ConfigureAwait(false);
        }

        [Command("reject")]
        public async Task Reject(CommandContext ctx, int suggestionId, [RemainingText] string reason)
        {
            var suggestion = await _communityStreamerService.GetStreamer(ctx.Guild.Id, suggestionId);

            if (suggestion == null) { await ctx.Channel.SendMessageAsync("This ID does not exist. Please try again.").ConfigureAwait(false); return; }

            if (suggestion.DealtWith == "APPROVED") { await ctx.Channel.SendMessageAsync("This request has already been approved!").ConfigureAwait(false); return; };

            if (suggestion.DealtWith == "DENIED") { await ctx.Channel.SendMessageAsync("This request has already been rejected!").ConfigureAwait(false); return; };

            DiscordMember suggestor = await ctx.Guild.GetMemberAsync(suggestion.requestorId);

            if(reason == null) {}

            if (suggestor != null)
            {
                if(reason == null)
                {
                    await suggestor.SendMessageAsync($"Your Request to add `{suggestion.streamerName}` to the Now Live bot has been rejected in the {ctx.Guild.Name} server!");
                }
                else
                {
                    await suggestor.SendMessageAsync($"Your Request to add `{suggestion.streamerName}` to the Now Live bot has been approved in the {ctx.Guild.Name} server for the following reason:\n\n {reason}");
                }
                
            }

            suggestion.DealtWith = "DENIED";

            await _communityStreamerService.EditStreamer(suggestion);

            DiscordChannel channel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "streamers-to-approve");

            DiscordMessage message = await channel.GetMessageAsync(suggestion.RequestMessage);

            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = $"Request for streamer to be added to the bot by: {suggestor.DisplayName}",
                Description = suggestion.streamerName,
                Color = DiscordColor.Red,
            };

            suggestionEmbed.AddField("This request Was Rejected by:", $"{ctx.Member.Mention}");

            suggestionEmbed.WithFooter($"Request: {suggestion.Id}");


            DiscordEmbed newEmbed = suggestionEmbed;

            await message.ModifyAsync(embed: newEmbed).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"Suggestion {suggestion.Id} has been rejected!").ConfigureAwait(false);
        }

    }
}
