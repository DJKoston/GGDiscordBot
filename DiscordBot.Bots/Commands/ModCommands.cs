﻿namespace DiscordBot.Bots.Commands
{
    [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
    public class ModCommands : BaseCommandModule
    {
        [Command("purge")]
        [Description("Deletes a certain amount of messages from the server. Warning, Max 100.")]
        public async Task Purge(CommandContext ctx, int deleteString)
        {
            if (deleteString > 100)
            {
                var errorMessage = new DiscordEmbedBuilder
                {
                    Title = "WARNING, MAX MESSAGES 100!",
                    Description = "You can only delete up to 100 messages using this command.",
                    Color = DiscordColor.Red,
                };

                await ctx.Channel.SendMessageAsync(embed: errorMessage);

                return;
            }

            var beforeTwoWeek = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, deleteString);

            var twoWeekMarker = beforeTwoWeek.Where(x => x.CreationTimestamp < DateTime.Now.AddDays(-13));

            var messagesToDelete = beforeTwoWeek.Except(twoWeekMarker);

            if (!messagesToDelete.Any()) { await ctx.Channel.SendMessageAsync("I'm sorry, I can only purge messages that are within 2 weeks of today's date."); return; }

            await ctx.Channel.DeleteMessagesAsync(messagesToDelete);

            await ctx.Message.DeleteAsync();

            var deletedConfirm = await ctx.Channel.SendMessageAsync($"{messagesToDelete.Count()} messages deleted!, This message will self distruct in 3 seconds.");

            await Task.Delay(1000);

            await deletedConfirm.ModifyAsync($"{messagesToDelete.Count()} messages deleted!, This message will self distruct in 2 seconds.");

            await Task.Delay(1000);

            await deletedConfirm.ModifyAsync($"{messagesToDelete.Count()} messages deleted!, This message will self distruct in 1 second.");

            await Task.Delay(1000);

            await deletedConfirm.DeleteAsync();
        }
    }
}
