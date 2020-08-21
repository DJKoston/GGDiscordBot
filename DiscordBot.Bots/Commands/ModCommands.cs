using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [RequireRoles(RoleCheckMode.Any, "Admin", "Discord Mods")]
    public class ModCommands : BaseCommandModule
    {
        [Command("purge")]
        [Description("Deletes a certain amount of messages from the server. Warning, Max 100.")]
        public async Task Test(CommandContext ctx, int deleteString)
        {
           if(deleteString > 100)
            {
                var errorMessage = new DiscordEmbedBuilder
                {
                    Title = "WARNING, MAX MESSAGES 100!",
                    Description = "You can only delete up to 100 messages using this command.",
                    Color = DiscordColor.Red,
                };

                await ctx.Channel.SendMessageAsync(embed: errorMessage).ConfigureAwait(false);

                return;
            }

            var messagesToDelete = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, deleteString);

            await ctx.Channel.DeleteMessagesAsync(messagesToDelete).ConfigureAwait(false);

            await ctx.Message.DeleteAsync();

            var deletedConfirm = await ctx.Channel.SendMessageAsync($"{deleteString} messages deleted!, This message will self distruct in 3 seconds.");

            Thread.Sleep(1000);

            await deletedConfirm.ModifyAsync($"{deleteString} messages deleted!, This message will self distruct in 2 seconds.");

            Thread.Sleep(1000);

            await deletedConfirm.ModifyAsync($"{deleteString} messages deleted!, This message will self distruct in 1 second.");

            Thread.Sleep(1000);

            await deletedConfirm.DeleteAsync();
        }
    }
}
