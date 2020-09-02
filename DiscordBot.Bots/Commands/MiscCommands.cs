using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class MiscCommands : BaseCommandModule
    {
        [Command("ping")]
        [Description("Play Ping-Pong with the Bot")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("mathadd")]
        [Description("Add two numbers together")]
        public async Task MathAdd(CommandContext ctx, [Description("First Number")] int numberOne, [Description("Second Number")] int numberTwo)
        {
            await ctx.Channel.SendMessageAsync((numberOne + numberTwo).ToString()).ConfigureAwait(false);
        }

        [Command("d12")]
        [Description("Rolls a 12 sided Dice")]
        public async Task RollTwelveDie(CommandContext ctx)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"🎲 The D12 has been rolled and the result is: {rnd.Next(1, 12)}").ConfigureAwait(false);
        }

        [Command("roles")]
        [Description("See what role commands you can use!")]
        public async Task Roles(CommandContext ctx)
        {
            var rolesEmbed = new DiscordEmbedBuilder
            {
                Title = "Role's You Can Self Assign!",
                Description = "Check the list below to find out what commands you can use to get your roles!",
                Color = DiscordColor.Blurple
            };
            rolesEmbed.AddField("`!role 18+`", "Gets the 18+ Role");
            rolesEmbed.WithFooter($"Command run by {ctx.User.Mention}");

            await ctx.Channel.SendMessageAsync(embed: rolesEmbed).ConfigureAwait(false);
        }

        [Command("cmcs")]
        [Description("Alex only Command")]
        [RequireRoles(RoleCheckMode.Any, "Dotty <3")]
        public async Task ChristianMinecraftServer(CommandContext ctx, DiscordMember member)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"You have been banned from {ctx.Member.Mention}'s Christian Minecraft Server {member.Mention}! HOW DARE!").ConfigureAwait(false);
        }
    }
}
