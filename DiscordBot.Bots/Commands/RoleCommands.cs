using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [Group("Role")]
    [Description("These commands allow you to be assigned a role.")]
    [RequireRoles(RoleCheckMode.None, "18+")]
    public class RoleCommands : BaseCommandModule
    {

        [Command("18+")]
        [Description("Allows you to join the 18+ Role")]
        [RequireRoles(RoleCheckMode.None, "18+")]
        public async Task NSFW(CommandContext ctx)
        {
            var joinEmbed = new DiscordEmbedBuilder
            {
                Title = "Are you over 18?",
                Color = DiscordColor.Orange
            };

            joinEmbed.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

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
                    Title = $" {ctx.Member.Username} has been added to the 18+ Role!",
                    Description = "If you want to leave the 18+ role, just type `!leaverole 18+`",
                    Color = DiscordColor.Orange
                };

                joinedEmbed.WithThumbnail(ctx.User.AvatarUrl);

                var role = ctx.Guild.GetRole(512707274418946059);
                await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
                await joinMessage.DeleteAsync().ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync(embed: joinedEmbed).ConfigureAwait(false);
            }
            else if (reactionResult.Result.Emoji == thumbsDownEmoji)
            {
                var notJoinedEmbed = new DiscordEmbedBuilder
                {
                    Title = "The Action has been cancelled",
                    Description = "If you wish to join the 18+ role, type !role 18+ again!",
                    Color = DiscordColor.Orange
                };

                await joinMessage.DeleteAsync().ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync(embed: notJoinedEmbed).ConfigureAwait(false);
            }
        }
    }

}
