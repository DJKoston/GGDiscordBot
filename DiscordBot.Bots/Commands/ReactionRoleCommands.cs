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
        public async Task AddReactionRole(CommandContext ctx)
        {
            var messageBuilder = new DiscordMessageBuilder
            {
                Content = $"You need to specify some information before you can add a reaction role. \n\nUSAGE: !rr add #channel-name MessageIDNumber @Role Emoji (add/remove)"
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder);
        }

        [Command("add")]
        public async Task AddReactionRole(CommandContext ctx, DiscordChannel Channel, ulong MessageId, DiscordRole Role, DiscordEmoji Emoji)
        {
            var messageBuilder = new DiscordMessageBuilder
            {
                Content = $"You need to specify whether to add or remove the role when the emote is clicked. \n\nUSAGE: !rr add {Channel.Mention} {MessageId} {Role.Mention} {Emoji} (add/remove)"
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder);
        }

        [Command("add")]
        public async Task AddReactionRole(CommandContext ctx, DiscordChannel Channel, ulong MessageId, DiscordRole Role, DiscordEmoji Emoji, string RemoveRoleAddRole)
        {
            if (RemoveRoleAddRole.ToLower() == "remove" || RemoveRoleAddRole.ToLower() == "add")
            {
                DiscordChannel channel = ctx.Guild.GetChannel(Channel.Id);
                DiscordMessage message = await channel.GetMessageAsync(MessageId);

                if (message == null) { await ctx.Channel.SendMessageAsync("There is no message with that ID. Please check the ID again."); }

                var ReactionRole = new ReactionRole();

                if (RemoveRoleAddRole.ToLower() == "remove")
                {
                    ReactionRole.GuildId = ctx.Guild.Id;
                    ReactionRole.ChannelId = channel.Id;
                    ReactionRole.MessageId = message.Id;
                    ReactionRole.RoleId = Role.Id;
                    ReactionRole.EmoteId = Emoji.Id;
                    ReactionRole.UnicodeEmote = Emoji.Name;
                    ReactionRole.RemoveAddRole = "remove";
                }
                else if (RemoveRoleAddRole.ToLower() == "add")
                {
                    ReactionRole.GuildId = ctx.Guild.Id;
                    ReactionRole.ChannelId = channel.Id;
                    ReactionRole.MessageId = message.Id;
                    ReactionRole.RoleId = Role.Id;
                    ReactionRole.EmoteId = Emoji.Id;
                    ReactionRole.UnicodeEmote = Emoji.Name;
                    ReactionRole.RemoveAddRole = "add";
                }

                await _reactionRoleService.CreateNewReactionRole(ReactionRole);

                await message.CreateReactionAsync(Emoji);

                await ctx.Channel.SendMessageAsync("Reaction Role added!");
            }
            else
            {
                await ctx.Channel.SendMessageAsync("You need to specify `add` or `remove` in your final argument.");
            }
        }

        [Command("delete")]
        public async Task DeletedReactionRole(CommandContext ctx, DiscordChannel Channel, ulong MessageId, DiscordEmoji Emoji)
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

                if (reactionRole.EmoteId == 0)
                {
                    ReactionRoleList.AddField($"MessageID: {reactionRole.MessageId}", $"Channel: {channel.Name}, Role: {role.Mention}, Emote: {reactionRole.UnicodeEmote}, Add/Remove on React: {reactionRole.RemoveAddRole}");
                }

                else
                {
                    DiscordEmoji emoji = await ctx.Guild.GetEmojiAsync(reactionRole.EmoteId);

                    ReactionRoleList.AddField($"MessageID: {reactionRole.MessageId}", $"Channel: {channel.Name}, Role: {role.Mention}, Emote: {emoji}, Add/Remove on React: {reactionRole.RemoveAddRole}");
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
                Description = "`!rr add #channel-name MessageId @Role :Emote: add/remove`",
                Color = DiscordColor.Purple
            };

            HelpEmbed.AddField("To get your Message ID", "Right click the message and click 'Copy ID'");

            await ctx.Channel.SendMessageAsync(embed: HelpEmbed);
        }
    }
}
