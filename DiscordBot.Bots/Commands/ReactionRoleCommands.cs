using DiscordBot.Core.Services.ReactionRoles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.ReactionRoles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [Group("rr")]
    [Aliases("ReactionRole")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    public class ReactionRoleCommands : BaseCommandModule
    {
        private readonly IReactionRoleService _reactionRoleService;
        private readonly RPGContext _context;

        public ReactionRoleCommands(RPGContext context, IReactionRoleService reactionRoleService)
        {
            _reactionRoleService = reactionRoleService;
            _context = context;
        }

        [Command("add")]
        public async Task AddReactionRole(CommandContext ctx, DiscordChannel Channel, ulong MessageId, DiscordRole Role, DiscordEmoji Emoji)
        {
            DiscordChannel channel = ctx.Guild.GetChannel(Channel.Id);
            DiscordMessage message = await channel.GetMessageAsync(MessageId);

            var ReactionRole = new ReactionRole()
            {
                GuildId = ctx.Guild.Id,
                ChannelId = channel.Id,
                MessageId = message.Id,
                RoleId = Role.Id,
                EmoteId = Emoji.Id,
                UnicodeEmote = Emoji.Name
            };

            await _reactionRoleService.CreateNewReactionRole(ReactionRole);

            await message.CreateReactionAsync(Emoji);

            await ctx.Channel.SendMessageAsync("Reaction Role added!");
        }

        [Command("delete")]
        public async Task DeletedReactionRole(CommandContext ctx, DiscordChannel Channel, ulong MessageId, DiscordRole Role, DiscordEmoji Emoji)
        {
            DiscordChannel channel = ctx.Guild.GetChannel(Channel.Id);
            DiscordMessage message = await channel.GetMessageAsync(MessageId);

            var reactionRole = _reactionRoleService.GetReactionRole(ctx.Guild.Id, Channel.Id, MessageId, Emoji.Id, Emoji.Name).Result;

            await _reactionRoleService.DeleteReactionRole(reactionRole);

            await message.DeleteReactionsEmojiAsync(Emoji);

            await ctx.Channel.SendMessageAsync("Reaction Role Deleted.");
        }

        [Command("list")]
        public async Task ListReactionRoles(CommandContext ctx)
        {
            var reactionRoles = _context.ReactionRoles.Where(x => x.GuildId == ctx.Guild.Id);

            var ReactionRoleList = new DiscordEmbedBuilder
            {
                Title = $"List of current Reaction Roles in {ctx.Guild.Name}",
                Color = DiscordColor.MidnightBlue
            };

            foreach (ReactionRole reactionRole in reactionRoles)
            {
                DiscordChannel channel = ctx.Guild.GetChannel(reactionRole.ChannelId);
                DiscordRole role = ctx.Guild.GetRole(reactionRole.RoleId);

                if(reactionRole.EmoteId == 0)
                {
                    ReactionRoleList.AddField($"MessageID: {reactionRole.MessageId}", $"Channel: {channel.Name}, Role: {role.Mention}, Emote: {reactionRole.UnicodeEmote}");
                }

                else
                {
                    DiscordEmoji emoji = await ctx.Guild.GetEmojiAsync(reactionRole.EmoteId);

                    ReactionRoleList.AddField($"MessageID: {reactionRole.MessageId}", $"Channel: {channel.Name}, Role: {role.Mention}, Emote: {emoji}");
                }
            }

            await ctx.Channel.SendMessageAsync(embed: ReactionRoleList);

        }

        [Command("help")]
        public async Task ReactionRoleHelp(CommandContext ctx)
        {
            var HelpEmbed = new DiscordEmbedBuilder
            {
                Title = "How to add a Reaction Role",
                Description = "`!rr add #channel-name MessageId @Role :Emote:`",
                Color = DiscordColor.Purple
            };

            HelpEmbed.AddField("To get your Message ID", "Right click the message and click 'Copy ID'");

            await ctx.Channel.SendMessageAsync(embed: HelpEmbed).ConfigureAwait(false);
        }
    }
}
