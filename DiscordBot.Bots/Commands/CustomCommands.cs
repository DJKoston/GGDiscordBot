namespace DiscordBot.Bots.Commands
{
    public class CustomCommands : BaseCommandModule
    {
        private readonly ICustomCommandService _customCommandService;

        public CustomCommands(ICustomCommandService customCommandService)
        {
            _customCommandService = customCommandService;
        }

        [Command("addcommand")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task AddCommand(CommandContext ctx, string trigger, [RemainingText] string action)
        {
            if (trigger.StartsWith('!')) { await ctx.RespondAsync("Do not start the commmand trigger with \"!\""); return; }

            var command = new CustomCommand
            {
                Trigger = $"!{trigger}",
                Action = $"{action}",
                GuildId = ctx.Guild.Id
            };

            await _customCommandService.CreateNewCommandAsync(command);

            var commandAddedEmbed = new DiscordEmbedBuilder
            {
                Title = $"Command Added: !{command.Trigger}",
                Description = $"Action: {command.Action}",
                Color = DiscordColor.Purple,
            };

            await ctx.Channel.SendMessageAsync(embed: commandAddedEmbed);
        }

        [Command("deletecommand")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task DeleteCommand(CommandContext ctx, string customCommand)
        {
            var command = await _customCommandService.GetCommandAsync(customCommand, ctx.Guild.Id);

            if (command == null)
            {
                var noCommandEmbed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Command called {customCommand}",
                    Description = "Try searching again!",
                    Color = DiscordColor.Red,
                };

                var messageBuilder1 = new DiscordMessageBuilder
                {
                    Embed = noCommandEmbed,
                };

                messageBuilder1.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder1);

                return;
            }

            await _customCommandService.DeleteCommandAsync(command);

            var messageBuilder = new DiscordMessageBuilder
            {
                Content = $"Command: `{command.Trigger}` deleted.",
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder);
        }
    }
}
