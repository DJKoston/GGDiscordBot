using DiscordBot.Core.Services.Configurations;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace DiscordBot.Bots.SlashCommands
{
    public class GameSlashCommands : ApplicationCommandModule
    {
        private readonly IProfileService _profileService;
        private readonly IGoldService _goldService;
        private readonly ICurrencyNameConfigService _currencyNameService;
        public string CurrencyName;

        public GameSlashCommands(IProfileService profileService, IGoldService goldService, ICurrencyNameConfigService currencyNameService)
        {
            _profileService = profileService;
            _goldService = goldService;
            _currencyNameService = currencyNameService;
        }

        [SlashCommand("spin", "Spin's a wheel and gets a random bonus!")]
        public async Task Spin(InteractionContext ctx,
            [Option("amount", "Amount to spin.")] double betToConvert)
        {
            var bet = Convert.ToInt32(betToConvert);

            var currencyNameConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (currencyNameConfig == null) { CurrencyName = "Gold"; }
            else { CurrencyName = currencyNameConfig.CurrencyName; }

            var rnd = new Random();
            var winOrLose = rnd.Next(1, 3);

            var profileCheck = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

            if (bet > 1000000)
            {
                var maxLimitCheck = new DiscordEmbedBuilder()
                {
                    Title = $"You can't bet more than 1,000,000 {CurrencyName}!",
                    Color = DiscordColor.HotPink,
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(maxLimitCheck);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder);

                return;
            }

            if (bet < 0)
            {
                var lessThanCheck = new DiscordEmbedBuilder()
                {
                    Title = "You cannot bet less than 0!",
                    Description = "Nice try suckka!",
                    Color = DiscordColor.HotPink,
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(lessThanCheck);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder);

                return;
            }

            if (profileCheck.Gold < bet)
            {
                var profileCheckFail = new DiscordEmbedBuilder()
                {
                    Title = "You can't bet that much!",
                    Description = $"Seems like you're too poor to afford to bet {bet:###,###,###,###,###} {CurrencyName}! Try betting a smaller amount... We have shown you how much {CurrencyName} you have below!"
                };

                if (profileCheck.Gold == 0) { profileCheckFail.AddField(CurrencyName, profileCheck.Gold.ToString()); }
                if (profileCheck.Gold >= 1) { profileCheckFail.AddField(CurrencyName, profileCheck.Gold.ToString("###,###,###,###,###")); }

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(profileCheckFail);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder);

                return;
            }

            if (winOrLose == 2)
            {
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                var rndWinX = new Random();
                var maxWin = bet * 3;
                var winAmount = rndWinX.Next(bet, maxWin);

                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, winAmount, ctx.Member.Username);

                var winEmbed = new DiscordEmbedBuilder()
                {
                    Title = $"You won the spin!",
                    Description = $"You just won {winAmount:###,###,###,###,###} {CurrencyName}!",
                    Color = DiscordColor.SpringGreen
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(winEmbed);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder);

                return;
            }

            if (winOrLose == 1)
            {
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                var loseEmbed = new DiscordEmbedBuilder() { Title = "You lost the spin!", Description = $"You just lost {bet:###,###,###,###,###} {CurrencyName}!", Color = DiscordColor.IndianRed };

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(loseEmbed);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder);

                return;
            }
        }
    }
}
