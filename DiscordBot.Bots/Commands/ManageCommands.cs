using DiscordBot.Bots.Handlers.Dialogue.Steps;
using DiscordBot.Core.Services.CustomCommands;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.CustomCommands;
using DiscordBot.Handlers.Dialogue;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class ManageCommands : BaseCommandModule
    {
        private readonly ICustomCommandService _customCommandService;
        private readonly RPGContext _context;

        public ManageCommands(RPGContext context, ICustomCommandService customCommandService)
        {
            _context = context;
            _customCommandService = customCommandService;
        }
        
        [Command("addcommand")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task AddCommand(CommandContext ctx)
        {
            var commandActionStep = new commandAddStep("What will the output of this command be?", null);
            var commandTriggerStep = new commandAddStep("What will the command trigger be? (Do not include the '!' prefix!)", commandActionStep);

            var command = new CustomCommand();

            commandTriggerStep.OnValidResult += (result) => command.Trigger = $"!{result}";
            commandActionStep.OnValidResult += (result) => command.Action = $"{result}";

            command.GuildId = ctx.Guild.Id;

            var inputDialogueHandler = new DialogueHandler(
                ctx.Client,
                ctx.Channel,
                ctx.User,
                commandTriggerStep
                );

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }

            await _customCommandService.CreateNewCommandAsync(command).ConfigureAwait(false);

            var commandAddedEmbed = new DiscordEmbedBuilder
            {
                Title = $"Command Added: {command.Trigger}",
                Description = $"Action: {command.Action}",
                Color = DiscordColor.Purple,
            };

            await ctx.Channel.SendMessageAsync(embed: commandAddedEmbed).ConfigureAwait(false);
        }

        [Command("deletecommand")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task Profile(CommandContext ctx)
        {
            var messageBuilder = new DiscordMessageBuilder
            {
                Content = "You have not specified what command is to be deleted! **USAGE: `!deletecommand <!command>`**",
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
        }

        [Command("deletecommand")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task Profile(CommandContext ctx, string command)
        {
            await CommandDelete(ctx, command);
        }

        private async Task CommandDelete(CommandContext ctx, string customCommand)
        {
            var command = await _customCommandService.GetCommandAsync(customCommand, ctx.Guild.Id).ConfigureAwait(false);

            if(command == null)
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

                await ctx.Channel.SendMessageAsync(messageBuilder1).ConfigureAwait(false);

                return;
            }

            await _customCommandService.DeleteCommandAsync(command);

            var messageBuilder = new DiscordMessageBuilder
            {
                Content = $"Command: `{command.Trigger}` deleted.",
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
        }
    }
}
