using DiscordBot.Core.Services.Configs;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configs;
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

        public ConfigCommands(INitroBoosterRoleConfigService nitroBoosterRoleService, IWelcomeMessageConfigService welcomeMessageConfigService)
        {
            _nitroBoosterRoleService = nitroBoosterRoleService;
            _welcomeMessageConfigService = welcomeMessageConfigService;
        }

        [Command("SetNitroRole")]
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
                    Title = $"New Discord Nitro Booster Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Nitro Booster Role for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                DiscordRole NitroRole = ctx.Guild.GetRole(config.RoleId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is already a Nitro Booster Role Set!",
                    Description = $"{NitroRole.Mention} is the Nitro Booster Role for the Server!"
                };

                embed.AddField("To Clear the Config for the Nitro Booster Role", "Do `!config resetnitrorole`");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }


        }

        [Command("GetNitroRole")]
        public async Task ViewCurrentNitroBoosterRole(CommandContext ctx)
        {
            var config = _nitroBoosterRoleService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            if(config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Nitro Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Nitro Role Config, do `!config setnitrorole @Role`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }

            else
            {
                DiscordRole NitroBooster = ctx.Guild.GetRole(config.RoleId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Nitro Booster Role Configured\nfor {ctx.Guild.Name}",
                    Description = $"{NitroBooster.Mention} is the Nitro Booster Role for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
        }

        [Command("ResetNitroRole")]
        public async Task ResetNitroBoosterRole(CommandContext ctx)
        {
            var config = _nitroBoosterRoleService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            if (config == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"There is no Nitro Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"To add a Nitro Role Config, do `!config setnitrorole @Role`"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
            }

            else
            {
                await _nitroBoosterRoleService.RemoveNitroBoosterConfig(config);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"The Nitro Booster Role Config has been reset!",
                };

                embed.AddField("To Add the Config for the Nitro Booster Role", "Do `!config setnitrorole @Role`");

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
                var WMConfig = new WelcomeMessageConfig()
                {
                    GuildId = ctx.Guild.Id,
                    ChannelId = channel.Id,
                };

                await _welcomeMessageConfigService.CreateNewWelcomeMessageConfig(WMConfig);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"New Welcome Channel Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{channel.Mention} is the Welcome Channel for the Server!"
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                return;
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
    }
}
