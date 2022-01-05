namespace DiscordBot.Bots.Commands
{
    [Group("config")]
    [Description("Configure the bot for this server.")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]
    public class ConfigCommands : BaseCommandModule
    {
        private readonly IDoubleXPRoleConfigService _doubleXPRoleConfigService;
        private readonly ILeaveMessageConfigService _leaveMessageConfigService;
        private readonly IWelcomeMessageConfigService _welcomeMessageConfigService;
        private readonly INowLiveRoleConfigService _nowLiveRoleConfigService;
        private readonly ICurrencyNameConfigService _currencyNameConfigService;

        public ConfigCommands(IDoubleXPRoleConfigService doubleXPRoleConfigService, ILeaveMessageConfigService leaveMessageConfigService, IWelcomeMessageConfigService welcomeMessageConfigService, INowLiveRoleConfigService nowLiveRoleConfigService, ICurrencyNameConfigService currencyNameConfigService)
        {
            _doubleXPRoleConfigService = doubleXPRoleConfigService;
            _leaveMessageConfigService = leaveMessageConfigService;
            _welcomeMessageConfigService = welcomeMessageConfigService;
            _nowLiveRoleConfigService = nowLiveRoleConfigService;
            _currencyNameConfigService = currencyNameConfigService;
        }

        [Command("view")]
        [Description("View the config of this Server.")]
        public async Task ViewConfig(CommandContext ctx)
        {
            var doubleXPRoleConfig = await _doubleXPRoleConfigService.GetDoubleXPRole(ctx.Guild.Id);
            var welcomeMessageConfig = await _welcomeMessageConfigService.GetWelcomeMessage(ctx.Guild.Id);
            var leaveMessageConfig = await _leaveMessageConfigService.GetLeaveMessageConfig(ctx.Guild.Id);
            var nowLiveRoleConfig = await _nowLiveRoleConfigService.GetNowLiveRole(ctx.Guild.Id);
            var currencyNameConfig = await _currencyNameConfigService.GetCurrencyNameConfig(ctx.Guild.Id);

            var embed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Blurple,
                Title = $"{ctx.Guild.Name} Configuration:",
                Description = "Please see the configuration for your server below."
            };

            if (doubleXPRoleConfig != null)
            {
                var guildDoubleXPRole = ctx.Guild.GetRole(doubleXPRoleConfig.RoleId);

                embed.AddField("Double XP Role:", $"{guildDoubleXPRole.Mention}");
            }

            else if (doubleXPRoleConfig == null)
            {
                embed.AddField("Double XP Role:", "Not Configured.");
            }

            if (welcomeMessageConfig != null)
            {
                var guildWelcomeMessageChannel = ctx.Guild.GetChannel(welcomeMessageConfig.ChannelId);

                embed.AddField("Welcome Message Channel:", $"{guildWelcomeMessageChannel.Mention}");
                embed.AddField("Welcome Message:", $"{welcomeMessageConfig.WelcomeMessage}", true);
                embed.AddField("Welcome Image:", $"[Click here for Image]({welcomeMessageConfig.WelcomeImage})", true);
            }

            else if (welcomeMessageConfig == null)
            {
                embed.AddField("Welcome Message:", "Not Configured.");
            }

            if (leaveMessageConfig != null)
            {
                var guildLeaveMessageChannel = ctx.Guild.GetChannel(leaveMessageConfig.ChannelId);

                embed.AddField("Leave Message Channel:", $"{guildLeaveMessageChannel.Mention}");
                embed.AddField("Leave Message:", $"{leaveMessageConfig.LeaveMessage}", true);
                embed.AddField("Leave Image:", $"[Click here for Image]({leaveMessageConfig.LeaveImage})", true);
            }

            else if (leaveMessageConfig == null)
            {
                embed.AddField("Leave Message:", "Not Configured.");
            }

            if (nowLiveRoleConfig != null)
            {
                var guildNowLiveRole = ctx.Guild.GetRole(nowLiveRoleConfig.RoleId);

                embed.AddField("Now Live Role:", $"{guildNowLiveRole.Mention}");
            }

            else if (nowLiveRoleConfig == null)
            {
                embed.AddField("Now Live Role:", "Not Configured.");
            }

            if (currencyNameConfig != null)
            {
                embed.AddField("Currency Name:", $"{currencyNameConfig.CurrencyName}");
            }

            else if (currencyNameConfig == null)
            {
                embed.AddField("Custom Currency Name:", "Not Configured.");
            }

            var responseBuilder = new DiscordMessageBuilder();

            responseBuilder.AddEmbed(embed);

            await ctx.RespondAsync(responseBuilder);
        }

        [Command("reset")]
        [Description("Reset the Config for the Server.")]
        public async Task ResetConfig(CommandContext ctx, string configType)
        {
            if (configType == "xprole")
            {
                var doubleXPRoleConfig = await _doubleXPRoleConfigService.GetDoubleXPRole(ctx.Guild.Id);

                if (doubleXPRoleConfig != null)
                {
                    await _doubleXPRoleConfigService.DeleteDoubleXPRole(doubleXPRoleConfig);

                    await ctx.RespondAsync($"Deleted Double XP Role Configuration for {ctx.Guild.Name}");
                }

                else
                {
                    await ctx.RespondAsync($"There is no Double XP Role Configuration for {ctx.Guild.Name}");
                }
            }

            else if (configType == "welcome")
            {
                var welcomeMessageConfig = await _welcomeMessageConfigService.GetWelcomeMessage(ctx.Guild.Id);

                if (welcomeMessageConfig != null)
                {
                    await _welcomeMessageConfigService.RemoveWelcomeMessage(welcomeMessageConfig);

                    await ctx.RespondAsync($"Deleted Welcome Message Configuration for {ctx.Guild.Name}");
                }

                else
                {
                    await ctx.RespondAsync($"There is no Welcome Message Configuration for {ctx.Guild.Name}");
                }
            }

            else if (configType == "leave")
            {
                var leaveMessageConfig = await _leaveMessageConfigService.GetLeaveMessageConfig(ctx.Guild.Id);

                if (leaveMessageConfig != null)
                {
                    await _leaveMessageConfigService.RemoveLeaveMessage(leaveMessageConfig);

                    await ctx.RespondAsync($"Deleted Leave Message Configuration for {ctx.Guild.Name}");
                }

                else
                {
                    await ctx.RespondAsync($"There is no Leave Message Configuration for {ctx.Guild.Name}");
                }
            }

            else if (configType == "nowlive")
            {
                var nowLiveRoleConfig = await _nowLiveRoleConfigService.GetNowLiveRole(ctx.Guild.Id);

                if (nowLiveRoleConfig != null)
                {
                    await _nowLiveRoleConfigService.RemoveNowLiveRole(nowLiveRoleConfig);

                    await ctx.RespondAsync($"Deleted Now Live Role Configuration for {ctx.Guild.Name}");
                }

                else
                {
                    await ctx.RespondAsync($"There is no Now Live Role Configuration for {ctx.Guild.Name}");
                }
            }

            else if (configType == "currency")
            {
                var customCurrencyConfig = await _currencyNameConfigService.GetCurrencyNameConfig(ctx.Guild.Id);

                if (customCurrencyConfig != null)
                {
                    await _currencyNameConfigService.RemoveCurrencyName(customCurrencyConfig);

                    await ctx.RespondAsync($"Deleted Custom Currency Name Configuration for {ctx.Guild.Name}");
                }

                else
                {
                    await ctx.RespondAsync($"There is no Custom Currency Name Configuration for {ctx.Guild.Name}");
                }
            }
        }

        [Command("doublexprole")]
        [Description("Set the Double XP Role for the Server.")]
        public async Task DoubleXPRole(CommandContext ctx, DiscordRole role)
        {
            var config = await _doubleXPRoleConfigService.GetDoubleXPRole(ctx.Guild.Id);

            if (config == null)
            {
                DoubleXPRoleConfig newDoubleXPRole = new()
                {
                    GuildId = ctx.Guild.Id,
                    RoleId = role.Id,
                };

                await _doubleXPRoleConfigService.AddDoubleXPRole(newDoubleXPRole);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Double XP Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Double XP Role for the Server!"
                };

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }

            else
            {
                config.RoleId = role.Id;

                await _doubleXPRoleConfigService.EditDoubleXPRole(config);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Double XP Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Double XP Role for the Server!"
                };

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }
        }

        [Command("welcomemessage")]
        [Description("Set the Welcome Message for the Server.")]
        public async Task WelcomeMessage(CommandContext ctx, DiscordChannel channel, string imageurl, [RemainingText] string message)
        {
            var config = await _welcomeMessageConfigService.GetWelcomeMessage(ctx.Guild.Id);

            if (config == null)
            {
                var newWelcomeConfig = new WelcomeConfig()
                {
                    GuildId = ctx.Guild.Id,
                    ChannelId = channel.Id,
                    WelcomeMessage = message,
                    WelcomeImage = imageurl,
                };

                await _welcomeMessageConfigService.AddWelcomeMessage(newWelcomeConfig);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Welcome Message Config\nAdded for {ctx.Guild.Name}",
                };

                embed.AddField("Welcome Channel:", $"{channel.Mention}");
                embed.AddField("Welcome Message:", message);
                embed.WithImageUrl(imageurl);

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }

            else
            {
                config.ChannelId = channel.Id;
                config.WelcomeMessage = message;
                config.WelcomeImage = imageurl;

                await _welcomeMessageConfigService.EditWelcomeMessage(config);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Welcome Message Config\nAdded for {ctx.Guild.Name}",
                };

                embed.AddField("Welcome Channel:", $"{channel.Mention}");
                embed.AddField("Welcome Message:", message);
                embed.WithImageUrl(imageurl);

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }
        }

        [Command("leavemessage")]
        [Description("Set the Leave Message for the Server.")]
        public async Task LeaveMessage(CommandContext ctx, DiscordChannel channel, string imageurl, [RemainingText] string message)
        {
            var config = await _leaveMessageConfigService.GetLeaveMessageConfig(ctx.Guild.Id);

            if (config == null)
            {
                var newLeaveConfig = new LeaveConfig()
                {
                    GuildId = ctx.Guild.Id,
                    ChannelId = channel.Id,
                    LeaveMessage = message,
                    LeaveImage = imageurl,
                };

                await _leaveMessageConfigService.AddLeaveMessage(newLeaveConfig);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Leave Message Config\nAdded for {ctx.Guild.Name}",
                };

                embed.AddField("Leave Channel:", $"{channel.Mention}");
                embed.AddField("Leave Message:", message);
                embed.WithImageUrl(imageurl);

                var  responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }

            else
            {
                config.ChannelId = channel.Id;
                config.LeaveMessage = message;
                config.LeaveImage = imageurl;

                await _leaveMessageConfigService.EditLeaveMessage(config);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Leave Message Config\nAdded for {ctx.Guild.Name}",
                };

                embed.AddField("Leave Channel:", $"{channel.Mention}");
                embed.AddField("Leave Message:", message);
                embed.WithImageUrl(imageurl);

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }
        }

        [Command("nowliverole")]
        [Description("Set the Now Live Role for the Server.")]
        public async Task NowLiveRole(CommandContext ctx,  DiscordRole role)
        {
            var config = await _nowLiveRoleConfigService.GetNowLiveRole(ctx.Guild.Id);

            if (config == null)
            {
                var newNowLiveRole = new NowLiveRoleConfig()
                {
                    GuildId = ctx.Guild.Id,
                    RoleId = role.Id,
                };

                await _nowLiveRoleConfigService.AddNowLiveRole(newNowLiveRole);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Now Live Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Now Live Role for the Server!"
                };

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }

            else
            {
                config.RoleId = role.Id;

                await _nowLiveRoleConfigService.EditNowLiveRole(config);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Now Live Role Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{role.Mention} is the Now Live Role for the Server!"
                };

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }
        }

        [Command("customcurrency")]
        [Description("Set the Custom Currency Name for the Server.")]
        public async Task CustomCurrency(CommandContext ctx, string currencyName)
        {
            var config = await _currencyNameConfigService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (config == null)
            {
                var newCurrencyName = new CurrencyNameConfig()
                {
                    GuildId = ctx.Guild.Id,
                    CurrencyName = currencyName,
                };

                await _currencyNameConfigService.NewCurrencyName(newCurrencyName);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Custom Currency Name Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{currencyName} is now the Custom Currency for the Server!"
                };

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }

            else
            {
                config.CurrencyName = currencyName;

                await _currencyNameConfigService.EditCurrencyName(config);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"New Custom Currency Name Config\nAdded for {ctx.Guild.Name}",
                    Description = $"{currencyName} is now the Custom Currency for the Server!"
                };

                var responseBuilder = new DiscordMessageBuilder();

                responseBuilder.AddEmbed(embed);

                await ctx.RespondAsync(responseBuilder);
            }
        }
    }
}
