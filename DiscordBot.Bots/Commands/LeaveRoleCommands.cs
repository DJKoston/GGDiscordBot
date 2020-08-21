using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{

    [Group("leaverole")]
    [Description("These commands allow you to leave a role.")]
    [RequireRoles(RoleCheckMode.Any, "18+")]
    public class LeaveRoleCommands : BaseCommandModule
    {
        [Command("18+")]
        [Description("Allows you to leave the 18+ Role")]
        [RequireRoles(RoleCheckMode.Any, "18+")]
        public async Task LeaveNSFW(CommandContext ctx)
        {
            var joinEmbed = new DiscordEmbedBuilder
            {
                Title = "Do you want to leave the 18+ Role?",
                Color = DiscordColor.Orange
            };

            joinEmbed.WithThumbnail(ctx.User.AvatarUrl);

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);

            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":thumbsup:");
            var thumbsDownEmoji = DiscordEmoji.FromName(ctx.Client, ":thumbsdown:");

            await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
            await joinMessage.CreateReactionAsync(thumbsDownEmoji).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == joinMessage && x.User.Id == ctx.User.Id && (x.Emoji == thumbsUpEmoji || x.Emoji == thumbsDownEmoji)).ConfigureAwait(false);

            if (reactionResult.Result.Emoji == thumbsUpEmoji)
            {

                var joinedEmbed = new DiscordEmbedBuilder
                {
                    Title = $" {ctx.Member.Username} left the 18+ Role!",
                    Description = "If you want to join the 18+ role again, just type `!role 18+`",
                    Color = DiscordColor.Orange
                };

                joinedEmbed.WithThumbnail(ctx.User.AvatarUrl);

                var role = ctx.Guild.GetRole(512707274418946059);
                await ctx.Member.RevokeRoleAsync(role).ConfigureAwait(false);
                await joinMessage.DeleteAsync().ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync(embed: joinedEmbed).ConfigureAwait(false);
            }
            else if (reactionResult.Result.Emoji == thumbsDownEmoji)
            {
                var notJoinedEmbed = new DiscordEmbedBuilder
                {
                    Title = "The Action has been cancelled",
                    Description = "If you wish to leave the 18+ role, type !18+leave again!",
                    Color = DiscordColor.Orange
                };

                await joinMessage.DeleteAsync().ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync(embed: notJoinedEmbed).ConfigureAwait(false);
            }


        }
    }
}
