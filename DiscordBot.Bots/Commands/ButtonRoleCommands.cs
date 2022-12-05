using DSharpPlus.Entities;

namespace DiscordBot.Bots.Commands
{
    [Group("buttonrole")]
    [RequirePermissions(Permissions.Administrator)]
    public class ButtonRoleCommands : BaseCommandModule
    {
        private readonly IButtonRoleService _buttonRoleService;

        public ButtonRoleCommands(IButtonRoleService buttonRoleService)
        {
            _buttonRoleService = buttonRoleService;
        }

        [Command("add1")]
        public async Task AddButtonRole1(CommandContext ctx, DiscordChannel channel,
            string giveRemove, string title1, DiscordRole role1, [RemainingText] string message)
        {
            DiscordMessageBuilder messageBuilder = new();

            messageBuilder.Content = message;

            messageBuilder.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title1}", $"{title1}")
            });

            var buttonRole = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title1}",
                RoleId = role1.Id,
                GiveRemove = giveRemove.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole);

            await channel.SendMessageAsync(messageBuilder);
        }

        [Command("add2")]
        public async Task AddButtonRole2(CommandContext ctx, DiscordChannel channel,
            string giveRemove1, string title1, DiscordRole role1,
            string giveRemove2, string title2, DiscordRole role2, [RemainingText] string message)
        {
            DiscordMessageBuilder messageBuilder = new();

            messageBuilder.Content = message;

            messageBuilder.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title1}", $"{title1}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title2}", $"{title2}")
            });

            var buttonRole1 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title1}",
                RoleId = role1.Id,
                GiveRemove = giveRemove1.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole1);

            var buttonRole2 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title2}",
                RoleId = role2.Id,
                GiveRemove = giveRemove2.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole2);

            await channel.SendMessageAsync(messageBuilder);
        }

        [Command("add3")]
        public async Task AddButtonRole3(CommandContext ctx, DiscordChannel channel,
            string giveRemove1, string title1, DiscordRole role1,
            string giveRemove2, string title2, DiscordRole role2,
            string giveRemove3, string title3, DiscordRole role3, [RemainingText] string message)
        {
            DiscordMessageBuilder messageBuilder = new();

            messageBuilder.Content = message;

            messageBuilder.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title1}", $"{title1}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title2}", $"{title2}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title3}", $"{title3}")
            });

            var buttonRole1 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title1}",
                RoleId = role1.Id,
                GiveRemove = giveRemove1.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole1);

            var buttonRole2 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title2}",
                RoleId = role2.Id,
                GiveRemove = giveRemove2.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole2);

            var buttonRole3 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title3}",
                RoleId = role3.Id,
                GiveRemove = giveRemove3.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole3);

            await channel.SendMessageAsync(messageBuilder);
        }

        [Command("add4")]
        public async Task AddButtonRole4(CommandContext ctx, DiscordChannel channel,
            string giveRemove1, string title1, DiscordRole role1,
            string giveRemove2, string title2, DiscordRole role2,
            string giveRemove3, string title3, DiscordRole role3,
            string giveRemove4, string title4, DiscordRole role4, [RemainingText] string message)
        {
            DiscordMessageBuilder messageBuilder = new();

            messageBuilder.Content = message;

            messageBuilder.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title1}", $"{title1}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title2}", $"{title2}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title3}", $"{title3}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title4}", $"{title4}")
            });

            var buttonRole1 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title1}",
                RoleId = role1.Id,
                GiveRemove = giveRemove1.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole1);

            var buttonRole2 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title2}",
                RoleId = role2.Id,
                GiveRemove = giveRemove2.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole2);

            var buttonRole3 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title3}",
                RoleId = role3.Id,
                GiveRemove = giveRemove3.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole3);

            var buttonRole4 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title4}",
                RoleId = role4.Id,
                GiveRemove = giveRemove4.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole4);

            await channel.SendMessageAsync(messageBuilder);
        }

        [Command("add5")]
        public async Task AddButtonRole5(CommandContext ctx, DiscordChannel channel,
            string giveRemove1, string title1, DiscordRole role1,
            string giveRemove2, string title2, DiscordRole role2,
            string giveRemove3, string title3, DiscordRole role3,
            string giveRemove4, string title4, DiscordRole role4,
            string giveRemove5, string title5, DiscordRole role5, [RemainingText] string message)
        {
            DiscordMessageBuilder messageBuilder = new();

            messageBuilder.Content = message;

            messageBuilder.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title1}", $"{title1}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title2}", $"{title2}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title3}", $"{title3}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title4}", $"{title4}"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ctx.Guild.Id}{title5}", $"{title5}")
            });

            var buttonRole1 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title1}",
                RoleId = role1.Id,
                GiveRemove = giveRemove1.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole1);

            var buttonRole2 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title2}",
                RoleId = role2.Id,
                GiveRemove = giveRemove2.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole2);

            var buttonRole3 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title3}",
                RoleId = role3.Id,
                GiveRemove = giveRemove3.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole3);

            var buttonRole4 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title4}",
                RoleId = role4.Id,
                GiveRemove = giveRemove4.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole4);

            var buttonRole5 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title5}",
                RoleId = role5.Id,
                GiveRemove = giveRemove5.ToLower(),
            };

            await _buttonRoleService.CreateButtonRole(buttonRole5);

            await channel.SendMessageAsync(messageBuilder);
        }

        [Command("enbygames")]
        public async Task AddEnbyGamesButton(CommandContext ctx, DiscordChannel channel)
        {
            DiscordRole role1 = ctx.Guild.GetRole(1028278198091395112); //Overwatch
            DiscordRole role2 = ctx.Guild.GetRole(986048836176846849); //Fortnite
            DiscordRole role3 = ctx.Guild.GetRole(986048856674402354); //Genshin
            DiscordRole role4 = ctx.Guild.GetRole(986048871895531592); //PCG
            DiscordRole role5 = ctx.Guild.GetRole(1045437857176752279);//Pokemon

            string title1 = "Overwatch";
            string title2 = "Fortnite";
            string title3 = "Genshin";
            string title4 = "Pokemon Community Game";
            string title5 = "Pokemon (Scarlet/Violet)";

            DiscordEmoji guildOverwatchEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 1028281250953306162);
            DiscordEmoji guildGenshinEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 1028282371138658405);
            DiscordEmoji guildFortniteEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 1028281791406161941);
            DiscordEmoji guildPCGEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 1028281966027620412);
            DiscordEmoji guildPokemonEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 1045441274938916916);

            DiscordComponentEmoji OverwatchEmoji = new() { Id = guildOverwatchEmoji.Id, Name = guildOverwatchEmoji.Name };
            DiscordComponentEmoji GenshinEmoji = new() { Id = guildGenshinEmoji.Id, Name = guildGenshinEmoji.Name };
            DiscordComponentEmoji FortniteEmoji = new() { Id = guildFortniteEmoji.Id, Name = guildFortniteEmoji.Name };
            DiscordComponentEmoji PCGEmoji = new() { Id = guildPCGEmoji.Id, Name = guildPCGEmoji.Name };
            DiscordComponentEmoji PokemonEmoji = new() { Id = guildPokemonEmoji.Id, Name = guildPokemonEmoji.Name };

            DiscordMessageBuilder messageBuilder = new();

            messageBuilder.Content = "Play any of these games and want a ping when someone wants to play? Click below to get the role!";

            messageBuilder.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, $"{ctx.Guild.Id}{title1}", title1, false, OverwatchEmoji),
                new DiscordButtonComponent(ButtonStyle.Secondary, $"{ctx.Guild.Id}{title2}", title2, false, FortniteEmoji),
                new DiscordButtonComponent(ButtonStyle.Secondary, $"{ctx.Guild.Id}{title3}", title3, false, GenshinEmoji),
                new DiscordButtonComponent(ButtonStyle.Secondary, $"{ctx.Guild.Id}{title4}", title4, false, PCGEmoji),
                new DiscordButtonComponent(ButtonStyle.Secondary, $"{ctx.Guild.Id}{title5}", title5, false, PokemonEmoji),
            });

            var buttonRole1 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title1}",
                RoleId = role1.Id,
                GiveRemove = "give",
            };

            await _buttonRoleService.CreateButtonRole(buttonRole1);

            var buttonRole2 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title2}",
                RoleId = role2.Id,
                GiveRemove = "give",
            };

            await _buttonRoleService.CreateButtonRole(buttonRole2);

            var buttonRole3 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title3}",
                RoleId = role3.Id,
                GiveRemove = "give",
            };

            await _buttonRoleService.CreateButtonRole(buttonRole3);

            var buttonRole4 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title4}",
                RoleId = role4.Id,
                GiveRemove = "give",
            };

            await _buttonRoleService.CreateButtonRole(buttonRole4);

            var buttonRole5 = new ButtonRoleConfig
            {
                GuildId = ctx.Guild.Id,
                ButtonId = $"{ctx.Guild.Id}{title5}",
                RoleId = role5.Id,
                GiveRemove = "give",
            };

            await _buttonRoleService.CreateButtonRole(buttonRole5);

            await channel.SendMessageAsync(messageBuilder);
        }
    }
}
