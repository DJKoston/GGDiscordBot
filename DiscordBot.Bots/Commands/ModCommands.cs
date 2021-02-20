using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
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

        [Command("getlog")]
        public async Task GetLog(CommandContext ctx, string date)
        {
            if(date.ToLower() == "today")
            {
                var parsedTimeDate = DateTime.Now;

                await ctx.Channel.SendFileAsync($"\\Logs\\{parsedTimeDate.Year}\\{parsedTimeDate.Month}\\{DateTime.Today.ToLongDateString()}.txt", $"Here is the log file for {parsedTimeDate.ToLongDateString()} {ctx.Member.Mention}").ConfigureAwait(false);
            }
            else
            {
                var parsedTimeDate = DateTime.Parse(date);

                await ctx.Channel.SendFileAsync($"\\Logs\\{parsedTimeDate.Year}\\{parsedTimeDate.Month}\\{DateTime.Today.ToLongDateString()}.txt", $"Here is the log file for {parsedTimeDate.ToLongDateString()} {ctx.Member.Mention}").ConfigureAwait(false);
            }
        }
    }
}
