namespace DiscordBot.Bots.SlashCommands
{
    public class GameSlashCommands : ApplicationCommandModule
    {
        private readonly IProfileService _profileService;
        private readonly IGoldService _goldService;
        private readonly ICurrencyNameConfigService _currencyNameService;
        private readonly INumberGuessService _numberGuessService;
        public string CurrencyName;

        public GameSlashCommands(IProfileService profileService, IGoldService goldService, ICurrencyNameConfigService currencyNameService, INumberGuessService numberGuessService)
        {
            _profileService = profileService;
            _goldService = goldService;
            _currencyNameService = currencyNameService;
            _numberGuessService = numberGuessService;
        }

        [SlashCommand("spin", "Spin the wheel and get a random bonus!")]
        public async Task SlashSpin(InteractionContext ctx,
            [Option("bet", "Amount to bet.")] double betConvert)
        {
            int bet = Convert.ToInt32(betConvert);

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
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

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
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

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
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

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
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            if (winOrLose == 1)
            {
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                var loseEmbed = new DiscordEmbedBuilder() { Title = "You lost the spin!", Description = $"You just lost {bet:###,###,###,###,###} {CurrencyName}!", Color = DiscordColor.IndianRed };

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(loseEmbed);
                messageBuilder.AsEphemeral(true);

                await ctx.CreateResponseAsync(messageBuilder); ;

                return;
            }
        }

        [SlashCommand("spinall", "Spin the wheel and get a random bonus!")]
        public async Task SlashSpinAll(InteractionContext ctx)
        {
            var currencyNameConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (currencyNameConfig == null) { CurrencyName = "Gold"; }
            else { CurrencyName = currencyNameConfig.CurrencyName; }

            var rnd = new Random();
            var winOrLose = rnd.Next(1, 3);

            var profileCheck = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

            if (profileCheck.Gold > 1000000)
            {
                var maxLimitCheck = new DiscordEmbedBuilder
                {
                    Title = $"You can't bet more than 1,000,000 {CurrencyName}!",
                    Description = "Please try again in 1 min",
                    Color = DiscordColor.HotPink,
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();
                messageBuilder.AddEmbed(maxLimitCheck);
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            if (profileCheck.Gold <= 0)
            {
                var lessThanCheck = new DiscordEmbedBuilder
                {
                    Title = $"You cannot bet 0 {CurrencyName} or less!",
                    Description = "Nice try suckka!",
                    Color = DiscordColor.HotPink,
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();
                messageBuilder.AddEmbed(lessThanCheck);
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            int bet = profileCheck.Gold;

            if (winOrLose == 2)
            {
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                var rndWinX = new Random();
                var maxWin = bet * 3;
                int winAmount = rndWinX.Next(bet, maxWin);

                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, winAmount, ctx.Member.Username);

                var winEmbed = new DiscordEmbedBuilder
                {
                    Title = $"You won the spin!",
                    Description = $"You just won {winAmount:###,###,###,###,###} {CurrencyName}!",
                    Color = DiscordColor.SpringGreen
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();
                messageBuilder.AddEmbed(winEmbed);
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            if (winOrLose == 1)
            {
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                var loseEmbed = new DiscordEmbedBuilder
                {
                    Title = "You lost the spin!",
                    Description = $"You just lost {bet:###,###,###,###,###} {CurrencyName}!",
                    Color = DiscordColor.IndianRed
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();
                messageBuilder.AddEmbed(loseEmbed);
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }
            return;
        }

        [SlashCommand("steal", "Steal from someone!")]
        public async Task SlashSteal(InteractionContext ctx,
            [Option("user", "User to attempt to steal from!")] DiscordUser user)
        {
            DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);

            var currencyNameConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (currencyNameConfig == null) { CurrencyName = "Gold"; }
            else { CurrencyName = currencyNameConfig.CurrencyName; }

            var rnd = new Random();
            var winOrLose = rnd.Next(1, 3);

            Profile thiefProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
            Profile victimProfile = await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

            if (ctx.User.Username == member.Username)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You want to try and steal from yourself?",
                    Description = "Who do you think you are? Chuck Norris??",
                    Color = DiscordColor.IndianRed,
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();
                messageBuilder.AddEmbed(errorEmbed);
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            if (victimProfile.Gold <= 0)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = $"You cannot steal from {member.DisplayName}",
                    Description = $"They don't have any {CurrencyName}! Talk about trying to kick a man while he's down!",
                    Color = DiscordColor.IndianRed,
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();
                messageBuilder.AddEmbed(errorEmbed);
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            if (victimProfile.Gold > 0)
            {
                if (winOrLose == 2)
                {
                    var rndSteal = rnd.Next(0, victimProfile.Gold);

                    await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, rndSteal, ctx.Member.Username);
                    await _goldService.GrantGoldAsync(member.Id, ctx.Guild.Id, -rndSteal, member.Username);

                    var winEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"You successfully stole {rndSteal:###,###,###,###,###} {CurrencyName} from {member.DisplayName}!",
                        Color = DiscordColor.SpringGreen
                    };

                    Profile thiefWinProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                    Profile victimLoseProfile = await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

                    if (thiefProfile.Gold == 0) { winEmbed.AddField(CurrencyName, thiefWinProfile.Gold.ToString()); }
                    if (thiefProfile.Gold >= 1) { winEmbed.AddField(CurrencyName, thiefWinProfile.Gold.ToString("###,###,###,###,###")); }

                    if (victimLoseProfile.Gold == 0) { winEmbed.AddField($"Their {CurrencyName}", victimLoseProfile.Gold.ToString()); }
                    if (victimLoseProfile.Gold >= 1) { winEmbed.AddField($"Their {CurrencyName}", victimLoseProfile.Gold.ToString("###,###,###,###,###")); }

                    var messageBuilder = new DiscordInteractionResponseBuilder();
                    messageBuilder.AddEmbed(winEmbed);
                    messageBuilder.AsEphemeral(true);
                    await ctx.CreateResponseAsync(messageBuilder);

                    await ctx.Channel.SendMessageAsync($"{user.Mention} - {ctx.Member.Mention} just stole {rndSteal:###,###,###,###,###} {CurrencyName} from you!\n\nYou can get them back by using `/steal`!");

                    return;
                }

                if (winOrLose == 1)
                {
                    var loseEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"You have unsucessfully stolen from {member.DisplayName}",
                        Color = DiscordColor.IndianRed
                    };

                    Profile thiefLoseProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                    Profile victimWinProfile = await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

                    if (thiefProfile.Gold == 0) { loseEmbed.AddField(CurrencyName, thiefLoseProfile.Gold.ToString()); }
                    if (thiefProfile.Gold >= 1) { loseEmbed.AddField(CurrencyName, thiefLoseProfile.Gold.ToString("###,###,###,###,###")); }

                    if (victimWinProfile.Gold == 0) { loseEmbed.AddField($"Their {CurrencyName}", victimWinProfile.Gold.ToString()); }
                    if (victimWinProfile.Gold >= 1) { loseEmbed.AddField($"Their {CurrencyName}", victimWinProfile.Gold.ToString("###,###,###,###,###")); }

                    var messageBuilder = new DiscordInteractionResponseBuilder();
                    messageBuilder.AddEmbed(loseEmbed);
                    messageBuilder.AsEphemeral(true);
                    await ctx.CreateResponseAsync(messageBuilder);

                    return;
                }
            }
        }

        [SlashCommand("guess", "Guess a number between 1 and 10, if you're right - Win 2,500 Currency if you are right!!")]
        public async Task SlashGuessTheNumber(InteractionContext ctx,
            [Option("number", "Type a number between 1 and 10!")] double guessconvert)
        {
            int guess = Convert.ToInt32(guessconvert);

            var currencyNameConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);
            var numberGuess = await _numberGuessService.GetNumberGuess(ctx.Guild.Id);

            if(numberGuess == null) 
            { 
                await _numberGuessService.CreateNumberGuess(ctx.Guild.Id);

                numberGuess = await _numberGuessService.GetNumberGuess(ctx.Guild.Id);
            }

            int number = numberGuess.Number;

            if (currencyNameConfig == null) { CurrencyName = "Gold"; }
            else { CurrencyName = currencyNameConfig.CurrencyName; }

            if (guess > 10)
            {
                var maxEmbed = new DiscordEmbedBuilder
                {
                    Title = $"You can't guess higher than 10",
                    Description = $"Try guessing a number between 1-10",
                    Color = DiscordColor.IndianRed
                };

                var messageBuilder = new DiscordInteractionResponseBuilder();
                messageBuilder.AddEmbed(maxEmbed);
                messageBuilder.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            if (guess == number)
            {
                await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, 2500, ctx.Member.Username);

                Profile profile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                var winEmbed = new DiscordEmbedBuilder
                {
                    Title = $"You correctly guessed the number {number}",
                    Description = $"You just won 2,500 {CurrencyName}!",
                    Color = DiscordColor.SpringGreen,
                };

                winEmbed.AddField($"Your {CurrencyName}:", $"{profile.Gold:###,###,###,###,###}");
                winEmbed.AddField("The number has now changed!", "Can you guess it now?");

                var messageBuilder2 = new DiscordInteractionResponseBuilder();
                messageBuilder2.AddEmbed(winEmbed);
                messageBuilder2.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder2);

                await _numberGuessService.UpdateNumberGuess(ctx.Guild.Id);

                await ctx.Channel.SendMessageAsync("The Number has just been guessed and has now changed! Can you get it before someone else does? Do `/guess` to give it a go!");

                return;
            }

            else
            {
                var loseEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Nice try, Try again!",
                    Description = "Can you guess it now?",
                    Color = DiscordColor.IndianRed
                };

                var messageBuilder3 = new DiscordInteractionResponseBuilder();
                messageBuilder3.AddEmbed(loseEmbed);
                messageBuilder3.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder3);
            }
            
        }

        [SlashCommand("coinflip", "Flip a coin and guess the outcome!")]
        public async Task SlashCoinFlip(InteractionContext ctx,
            [Choice("heads", 1)]
            [Choice("tails", 2)]
            [Option("guess", "Heads or Tails?")] double guessConvert)
        {
            int guess = Convert.ToInt32(guessConvert);

            var currencyNameConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (currencyNameConfig == null) { CurrencyName = "Gold"; }
            else { CurrencyName = currencyNameConfig.CurrencyName; }

            await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

            var rnd = new Random();
            var outcome = rnd.Next(1, 3);

            var coinSide = "";

            if (outcome == 1) { coinSide = "Heads"; }
            else if (outcome == 2) { coinSide = "Tails"; }

            if (guess == outcome)
            {
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, 500, ctx.Member.Username);

                var winEmbed = new DiscordEmbedBuilder
                {
                    Title = $"The coin landed on {coinSide}!",
                    Description = $"You just won 500 {CurrencyName}!",
                    Color = DiscordColor.SpringGreen,
                };

                var messageBuilder1 = new DiscordInteractionResponseBuilder();
                messageBuilder1.AddEmbed(winEmbed);
                messageBuilder1.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder1);

                return;
            }

            else
            {
                var loseEmbed = new DiscordEmbedBuilder
                {
                    Title = "The coin landed on Tails!",
                    Description = $"You just lost the toss!",
                    Color = DiscordColor.IndianRed,
                };

                var messageBuilder2 = new DiscordInteractionResponseBuilder();
                messageBuilder2.AddEmbed(loseEmbed);
                messageBuilder2.AsEphemeral(true);
                await ctx.CreateResponseAsync(messageBuilder2);

                return;
            }
        }

    }
}
