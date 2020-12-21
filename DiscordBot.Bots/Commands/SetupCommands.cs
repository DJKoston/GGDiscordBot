using DiscordBot.Bots.Handlers.Dialogue.Steps;
using DiscordBot.Core.Services.Configs;
using DiscordBot.DAL.Models.Configs;
using DiscordBot.Handlers.Dialogue;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]
    public class SetupCommands : BaseCommandModule
    {
        private readonly INitroBoosterRoleConfigService _nitroBoosterRoleService;
        private readonly IWelcomeMessageConfigService _welcomeMessageConfigService;
        private readonly IGameChannelConfigService _gameChannelConfigService;
        private readonly INowLiveRoleConfigService _nowLiveRoleConfigService;

        public SetupCommands(INitroBoosterRoleConfigService nitroBoosterRoleService, IWelcomeMessageConfigService welcomeMessageConfigService, IGameChannelConfigService gameChannelConfigService, INowLiveRoleConfigService nowLiveRoleConfigService)
        {
            _nitroBoosterRoleService = nitroBoosterRoleService;
            _welcomeMessageConfigService = welcomeMessageConfigService;
            _gameChannelConfigService = gameChannelConfigService;
            _nowLiveRoleConfigService = nowLiveRoleConfigService;
        }

        [Command("setup")]
        public async Task Setup(CommandContext ctx)
        {
            var setupStep8 = new SetupRoleStep("What Role would you like your users to be given when they go live?", null);
            var setupStep7 = new SetupChannelStep("What channel can your members play Discord Games in?", setupStep8);
            var setupStep6 = new SetupStep("What would you like your Leave Image to be? (Post a link to the image, DO NOT UPLOAD)", setupStep7);
            var setupStep5 = new SetupStep("What would you like your Leave Message to be? (Do not include Username etc here as the embed will already contain that information)", setupStep6);
            var setupStep4 = new SetupStep("What would you like your Welcome Image to be? (Post a link to the image, DO NOT UPLOAD)", setupStep5);
            var setupStep3 = new SetupStep("What would you like your Welcome Message to be? (Do not include Username etc here as the embed will already contain that information)", setupStep4);
            var setupStep2 = new SetupChannelStep("Which channel would you like your Welcome Channel to be? (This is used to announce when someone joins your server!)", setupStep3);
            var setupStep1 = new SetupRoleStep("Which role would you like your Double XP role to be? (This is used for 2x XP)", setupStep2);

            var nitroBoosterConfig = new NitroBoosterRoleConfig();
            nitroBoosterConfig.GuildId = ctx.Guild.Id;
            var welcomeChannelConfig = new WelcomeMessageConfig();
            welcomeChannelConfig.GuildId = ctx.Guild.Id;
            var gameChannelConfig = new GameChannelConfig();
            gameChannelConfig.GuildId = ctx.Guild.Id;
            var nowLiveRole = new NowLiveRoleConfig();
            nowLiveRole.GuildId = ctx.Guild.Id;

            setupStep1.OnValidResult += (DiscordRole result) => nitroBoosterConfig.RoleId = result.Id;
            setupStep2.OnValidResult += (DiscordChannel result) => welcomeChannelConfig.ChannelId = result.Id;
            setupStep3.OnValidResult += (result) => welcomeChannelConfig.WelcomeMessage = result;
            setupStep4.OnValidResult += (result) => welcomeChannelConfig.WelcomeImage = result;
            setupStep5.OnValidResult += (result) => welcomeChannelConfig.LeaveMessage = result;
            setupStep6.OnValidResult += (result) => welcomeChannelConfig.LeaveImage = result;
            setupStep7.OnValidResult += (DiscordChannel result) => gameChannelConfig.ChannelId = result.Id;
            setupStep8.OnValidResult += (DiscordRole result) => nowLiveRole.RoleId = result.Id;

            var inputDialogueHandler = new DialogueHandler(
                ctx.Client,
                ctx.Channel,
                ctx.User,
                setupStep1
                );

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }

            await _nitroBoosterRoleService.CreateNewNitroBoosterRoleConfig(nitroBoosterConfig);
            await _gameChannelConfigService.CreateGameChannelConfigService(gameChannelConfig);
            await _welcomeMessageConfigService.CreateNewWelcomeMessageConfig(welcomeChannelConfig);
            await _nowLiveRoleConfigService.CreateNowLiveRoleConfig(nowLiveRole);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Server Configuration Added",
                Description = $"To view your configuration run the commands below.",
                Color = DiscordColor.Purple,
            };

            embed.AddField("Double XP:", "`!config get2xprole`");
            embed.AddField("Welcome Channel Config", "`!config getwelcomechannel`");
            embed.AddField("Game Channel:", "`!config getgamechannel`");
            embed.AddField("Now Live Role:", "`!config getnowliverole`");

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            var suggestionChannel = await ctx.Guild.CreateChannelAsync("suggestions-log", DSharpPlus.ChannelType.Text, null, "This is where the Suggestions Live!");
            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = "Suggestion Channel Created!",
                Description = $"{suggestionChannel.Mention}\n\nPlease do not rename this channel as the bot will not know where to post suggestions for your admins to approve!\n\nYou may move the channel to another tab, just ensure {ctx.Client.CurrentUser.Mention} has permissions to access the channel and send messages to it!",
                Color = DiscordColor.Purple,
            };
            await ctx.Channel.SendMessageAsync(embed: suggestionEmbed);
            
            var streamerChannel = await ctx.Guild.CreateChannelAsync("streamers-to-approve", DSharpPlus.ChannelType.Text, null, "This is where the streamers to approve Live!");
            var streamerEmbed = new DiscordEmbedBuilder
            {
                Title = "Suggestion Channel Created!",
                Description = $"{streamerChannel.Mention}\n\nPlease do not rename this channel as the bot will not know where to post streamers for your admins to approve!\n\nYou may move the channel to another tab, just ensure {ctx.Client.CurrentUser.Mention} has permissions to access the channel and send messages to it!",
                Color = DiscordColor.Purple,
            };
            await ctx.Channel.SendMessageAsync(embed: streamerEmbed);

            var completedEmbed = new DiscordEmbedBuilder
            {
                Title = $"Base Configuration for {ctx.Guild.Name} Complete.",
                Description = $"Want to add streamers to be announced by the bot?\n\nThen go to: https://github.com/DJKoston/GGDiscordBot/wiki/Now-Live-Commands to get the commands!",
                Color = DiscordColor.Purple,
            };

            await ctx.Channel.SendMessageAsync(embed: completedEmbed);
        }
    }
}
