using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
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

            var beforeTwoWeek = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, deleteString);

            var twoWeekMarker = beforeTwoWeek.Where(x => x.CreationTimestamp < DateTime.Now.AddDays(-13));

            var messagesToDelete = beforeTwoWeek.Except(twoWeekMarker);

            if (messagesToDelete.Count() == 0) { await ctx.Channel.SendMessageAsync("I'm sorry, I can only purge messages that are within 2 weeks of today's date."); return; }

            await ctx.Channel.DeleteMessagesAsync(messagesToDelete).ConfigureAwait(false);

            await ctx.Message.DeleteAsync();

            var deletedConfirm = await ctx.Channel.SendMessageAsync($"{messagesToDelete.Count()} messages deleted!, This message will self distruct in 3 seconds.");

            Thread.Sleep(1000);

            await deletedConfirm.ModifyAsync($"{messagesToDelete.Count()} messages deleted!, This message will self distruct in 2 seconds.");

            Thread.Sleep(1000);

            await deletedConfirm.ModifyAsync($"{messagesToDelete.Count()} messages deleted!, This message will self distruct in 1 second.");

            Thread.Sleep(1000);

            await deletedConfirm.DeleteAsync();
        }

        [Command("getlog")]
        public async Task GetLog(CommandContext ctx, string date)
        {
            if(date.ToLower() == "today")
            {
                var parsedTimeDate = DateTime.Now;

                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = $"Here is the log file for {parsedTimeDate.ToLongDateString()} {ctx.Member.Mention}",
                };

                messageBuilder.WithFile($"\\Logs\\{parsedTimeDate.Year}\\{parsedTimeDate.Month}\\{DateTime.Today.ToLongDateString()}.txt");

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }
            else
            {
                var parsedTimeDate = DateTime.Parse(date);

                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = $"Here is the log file for {parsedTimeDate.ToLongDateString()} {ctx.Member.Mention}",
                };

                messageBuilder.WithFile($"\\Logs\\{parsedTimeDate.Year}\\{parsedTimeDate.Month}\\{parsedTimeDate.ToLongDateString()}.txt");

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }
        }
    }
}
