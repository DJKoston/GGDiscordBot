using DiscordBot.Bots.Handlers.Dialogue.Steps;
using DiscordBot.Core.Services.Configs;
using DiscordBot.DAL.Models.Configs;
using DiscordBot.Handlers.Dialogue;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [Group("Config")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    public class ConfigCommands : BaseCommandModule
    {
        private readonly INitroBoosterRoleConfigService _nitroBoosterRoleService;
        private readonly IWelcomeMessageConfigService _welcomeMessageConfigService;
        private readonly IGameChannelConfigService _gameChannelConfigService;
        private readonly INowLiveRoleConfigService _nowLiveRoleConfigService;

        public ConfigCommands(INitroBoosterRoleConfigService nitroBoosterRoleService, IWelcomeMessageConfigService welcomeMessageConfigService, IGameChannelConfigService gameChannelConfigService, INowLiveRoleConfigService nowLiveRoleConfigService)
        {
            _nitroBoosterRoleService = nitroBoosterRoleService;
            _welcomeMessageConfigService = welcomeMessageConfigService;
            _gameChannelConfigService = gameChannelConfigService;
            _nowLiveRoleConfigService = nowLiveRoleConfigService;
        }

        [Command("Set2xpRole")]
        public async Task CreateNewNitroBoosterConfig(CommandContext ctx, DiscordRole role)
        {
            var config = _nitroBoosterRoleService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var NitroBoosterConfig = new NitroBoosterRoleConfig()
                {
                    GuildId = ctx.Guild.Id,
                    RoleId = role.Id,
                };

                await _nitroBoosterRoleService.CreateNewNitroBoosterRoleConfig(NitroBoosterConfig);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"New Double XP Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Double XP Role for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                DiscordRole NitroRole = ctx.Guild.GetRole(config.RoleId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is already a Double XP Role Set!",
                    Description = $"{NitroRole.Mention} is the Double XP Role for the Server!"
                };

                embed.AddField("To Clear the Config for the Double XP Role", "Do `!config reset2xprole`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }


        }

        [Command("Get2xpRole")]
        public async Task ViewCurrentNitroBoosterRole(CommandContext ctx)
        {
            var config = _nitroBoosterRoleService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            if(config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Double XP Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Double XP Role Config, do `!config set2xprole @Role`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }

            else
            {
                DiscordRole NitroBooster = ctx.Guild.GetRole(config.RoleId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Double XP Role Configured\nfor {ctx.Guild.Name}",
                    Description = $"{NitroBooster.Mention} is the Double XP Role for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
        }

        [Command("Reset2xpRole")]
        public async Task ResetNitroBoosterRole(CommandContext ctx)
        {
            var config = _nitroBoosterRoleService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Double XP Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Double XP Role Config, do `!config set2xprole @Role`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                await _nitroBoosterRoleService.RemoveNitroBoosterConfig(config);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Double XP Role Config has been reset!",
                };

                embed.AddField("To Add the Config for the Double XP Role", "Do `!config set2xprole @Role`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }
        }

        [Command("SetWelcomeChannel")]
        public async Task CreateWelcomeMessageConfig(CommandContext ctx, DiscordChannel channel)
        {
            var config = _welcomeMessageConfigService.GetWelcomeMessageConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var LeaveImageStep = new welcomeChannelAddStep("What image would you like to display on the leave message?", null);
                var LeaveMessageStep = new welcomeChannelAddStep("What will the Leave Message be?", LeaveImageStep);
                var WelcomeImageStep = new welcomeChannelAddStep("What Image would you like to display on the welcome message?", LeaveMessageStep);
                var WelcomeMessageStep = new welcomeChannelAddStep("What will the Welcome Message be?", WelcomeImageStep);

                var WMConfig = new WelcomeMessageConfig();

                WelcomeMessageStep.OnValidResult += (result) => WMConfig.WelcomeMessage = $"{result}";
                WelcomeImageStep.OnValidResult += (result) => WMConfig.WelcomeImage = $"{result}";
                LeaveMessageStep.OnValidResult += (result) => WMConfig.LeaveMessage = $"{result}";
                LeaveImageStep.OnValidResult += (result) => WMConfig.LeaveImage = $"{result}";

                WMConfig.GuildId = ctx.Guild.Id;
                WMConfig.ChannelId = channel.Id;

                var inputDialogueHandler = new DialogueHandler(
                    ctx.Client,
                    ctx.Channel,
                    ctx.User,
                    WelcomeMessageStep
                    );

                bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

                if (!succeeded) { return; }

                await _welcomeMessageConfigService.CreateNewWelcomeMessageConfig(WMConfig).ConfigureAwait(false);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"New Welcome Channel Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{channel.Mention} is the Welcome Channel for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                var WMConfig2 = _welcomeMessageConfigService.GetWelcomeMessageConfig(ctx.Guild.Id).Result;

                await ctx.Channel.SendMessageAsync("Here is what your Welcome Message will look like!").ConfigureAwait(false);
                
                var joinEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Welcome to the Server {ctx.Member.DisplayName}",
                    Description = $"{WMConfig2.WelcomeMessage}",
                    ImageUrl = $"{WMConfig2.WelcomeImage}",
                    Color = DiscordColor.Purple,
                };

                var totalMembers = ctx.Guild.MemberCount;
                var otherMembers = totalMembers - 1;

                joinEmbed.WithThumbnail(ctx.Member.AvatarUrl);
                joinEmbed.AddField($"Once again welcome to the server!", $"Thanks for joining the other {otherMembers:###,###,###,###,###} of us!");

                await ctx.Channel.SendMessageAsync(ctx.Member.Mention, embed: joinEmbed);

                await ctx.Channel.SendMessageAsync("Here is what your Leave Message will look like!").ConfigureAwait(false);

                var leaveEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Big Oof! {ctx.Member.DisplayName} has just left the server!",
                    Description = $"{WMConfig2.LeaveMessage}",
                    ImageUrl = $"{WMConfig2.LeaveImage}",
                    Color = DiscordColor.Yellow,
                };

                leaveEmbed.WithThumbnail(ctx.Member.AvatarUrl);

                await ctx.Channel.SendMessageAsync(embed: leaveEmbed);
            }

            else
            {
                DiscordChannel WelcomeChannel = ctx.Guild.GetChannel(config.ChannelId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is already a Welcome Channel Set!",
                    Description = $"{WelcomeChannel.Mention} is the Welcome Channel for the Server!"
                };

                embed.AddField("To Clear the Config for the Welcome Channel", "Do `!config resetwelcomechannel`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }


        }

        [Command("GetWelcomeChannel")]
        public async Task ViewCurrentWelcomeChannel(CommandContext ctx)
        {
            var config = _welcomeMessageConfigService.GetWelcomeMessageConfig(ctx.Guild.Id).Result;

            if(config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Welcome Channel Configured\nfor {ctx.Guild.Name}",
                    Description = $"To add a Welcome Channel Config, do `!config setwelcomechannel #channel-name`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            else
            {
                DiscordChannel WelcomeChannel = ctx.Guild.GetChannel(config.ChannelId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Welcome Channel Configured\nfor {ctx.Guild.Name}",
                    Description = $"{WelcomeChannel.Mention} is the Welcome Channel for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
        }

        [Command("ResetWelcomeChannel")]
        public async Task ResetWelcomeChannel(CommandContext ctx)
        {
            var config = _welcomeMessageConfigService.GetWelcomeMessageConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Welcome Channel Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Welcome Channel Config, do `!config setwelcomechannel #channel-name`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                await _welcomeMessageConfigService.RemoveWelcomeMessageConfig(config);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Welcome Channel Config has been reset!",
                };

                embed.AddField("To Add the Config for the Welcome Channel", "Do `!config setwelcomechannel #channel-name`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }
        }

        [Command("SetGameChannel")]
        public async Task CreateGameChannelConfig(CommandContext ctx, DiscordChannel channel)
        {
            var config = _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id).Result;

            if (config == null)
            {
                var gameChannelConfig = new GameChannelConfig()
                {
                    GuildId = ctx.Guild.Id,
                    ChannelId = channel.Id,
                };

                await _gameChannelConfigService.CreateGameChannelConfigService(gameChannelConfig);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"New Game Channel Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{channel.Mention} is the Game Channel for the Server! No other channel will accept commands for games."
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                DiscordChannel gameChannel = ctx.Guild.GetChannel(config.ChannelId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is already a Game Channel Set!",
                    Description = $"{gameChannel.Mention} is the Game Channel for the Server!"
                };

                embed.AddField("To Clear the Config for the Game Channel", "Do `!config resetgamechannel`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }
        }

        [Command("GetGameChannel")]
        public async Task GetGameChannelConfig(CommandContext ctx)
        {
            var config = _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id).Result;

            if (config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Game Channel Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Game Channel Config, do `!config setgamechannel #channel-name`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }

            else
            {
                DiscordChannel channel = ctx.Guild.GetChannel(config.ChannelId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Game Channel Configured\nfor {ctx.Guild.Name}",
                    Description = $"{channel.Mention} is the Game Channel for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
        }

        [Command("ResetGameChannel")]
        public async Task ResetGameChannelConfig(CommandContext ctx)
        {
            var config = _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id).Result;

            if (config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Game Channel Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Game Channel Config, do `!config setgamechannel #channel-name`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                await _gameChannelConfigService.RemoveGameChannelConfigService(config);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Game Channel Config has been reset!",
                };

                embed.AddField("To Add the Config for the Game Channel", "Do `!config setgamechannel #channel-name`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }
        }

        [Command("SetUpSuggestionChannel")]
        public async Task SetUpSuggestionChannel(CommandContext ctx)
        {
            var suggestionChannel = await ctx.Guild.CreateChannelAsync("suggestions-log", DSharpPlus.ChannelType.Text, null, "This is where the Suggestions Live!");

            await ctx.Channel.SendMessageAsync($"Suggestion Channel Created! {suggestionChannel.Mention}\n\nPlease do not rename this channel as the bot will not know where to post suggestions for your admins to approve!\n\nYou may move the channel to another tab, just ensure {ctx.Client.CurrentUser.Mention} has permissions to access the channel and send messages to it!");
        }

        [Command("SetUpStreamerChannel")]
        public async Task SetUpStreamerLogChannel(CommandContext ctx)
        {
            var suggestionChannel = await ctx.Guild.CreateChannelAsync("streamers-to-approve", DSharpPlus.ChannelType.Text, null, "This is where the streamers to approve Live!");

            await ctx.Channel.SendMessageAsync($"Streamer Log Channel Created! {suggestionChannel.Mention}\n\nPlease do not rename this channel as the bot will not know where to post streamers for your admins to approve!\n\nYou may move the channel to another tab, just ensure {ctx.Client.CurrentUser.Mention} has permissions to access the channel and send messages to it!");
        }

        [Command("SetNowLiveRole")]
        public async Task CreateNowLiveRoleConfig(CommandContext ctx, DiscordRole role)
        {
            var config = _nowLiveRoleConfigService.GetNowLiveRoleConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var nowLiveRoleConfig = new NowLiveRoleConfig()
                {
                    GuildId = ctx.Guild.Id,
                    RoleId = role.Id,
                };

                await _nowLiveRoleConfigService.CreateNowLiveRoleConfig(nowLiveRoleConfig);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"New Now Live Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Now Live Role for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                DiscordRole role2 = ctx.Guild.GetRole(config.RoleId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is already a Now Live Role Set!",
                    Description = $"{role2.Mention} is the Now Live Role for the Server!"
                };

                embed.AddField("To Clear the Config for the Now Live Role", "Do `!config resetnowliverole`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }
        }

        [Command("GetNowLiveRole")]
        public async Task GetNowLiveRoleConfig(CommandContext ctx)
        {
            var config = _nowLiveRoleConfigService.GetNowLiveRoleConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Now Live Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Now Live Role Config, do `!config setnowliverole @role`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }

            else
            {
                DiscordRole role = ctx.Guild.GetRole(config.RoleId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Now Live Role Configured\nfor {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Now Live Role for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
        }

        [Command("ResetNowLiveRole")]
        public async Task ResetNowLiveRoleConfig(CommandContext ctx)
        {
            var config = _nowLiveRoleConfigService.GetNowLiveRoleConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Now Live Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Game Channel Config, do `!config setnowliverole @role`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                await _nowLiveRoleConfigService.RemoveNowLiveRoleConfig(config);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Now Live Role Config has been reset!",
                };

                embed.AddField("To Add the Config for the Now Live Role", "Do `!config setnowliverole @role`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }
        }
    }
}
