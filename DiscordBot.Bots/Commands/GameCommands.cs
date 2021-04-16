using DiscordBot.Core.Services.Configs;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class GameCommands : BaseCommandModule
    {
        private readonly IProfileService _profileService;
        private readonly IGoldService _goldService;
        private readonly IGameChannelConfigService _gameChannelConfigService;
        private readonly ICurrencyNameConfigService _currencyNameService;
        public string currencyName;

        public GameCommands(IProfileService profileService, IGoldService goldService, IGameChannelConfigService gameChannelConfigService, ICurrencyNameConfigService currencyNameService)
        {
            _profileService = profileService;
            _goldService = goldService;
            _gameChannelConfigService = gameChannelConfigService;
            _currencyNameService = currencyNameService;
        }

        [Command("spin")]
        [Description("Spins a wheel and gets a random bonus!")]
        public async Task SpinTheWheel(CommandContext ctx)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to specify an amount to spin!",
                    Description = "For example: `!spin 2015` or `!spin all`",
                    Color = DiscordColor.IndianRed,
                };

                errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = errorEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }

            return;
        }

        [Command("spin")]
        [Description("Spins a wheel and gets a random bonus!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task SpinTheWheel(CommandContext ctx, int bet)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            var CNConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            if (ctx.Channel.Id != configChannel.ChannelId)
            {
                if (configChannel == null)
                {
                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Content = "There isn't a game channel setup for this server. The Admin should specify one using the config command."
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
                }

                else
                {
                    var channel = ctx.Guild.GetChannel(configChannel.ChannelId);

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Content = $"You cannot run this command in this channel. Please run this command in the {channel.Mention} channel!"
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
                }
            }

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var rnd = new Random();
                var winOrLose = rnd.Next(1, 3);

                Profile profileCheck = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                if(bet > 1000000)
                {
                    var maxLimitCheck = new DiscordEmbedBuilder
                    {
                        Title = $"You can't bet more than 1,000,000 {currencyName}!",
                        Description = "Please try again in 1 min",
                        Color = DiscordColor.HotPink,
                    };

                    maxLimitCheck.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = maxLimitCheck,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                    return;
                }

                if(bet < 0)
                {
                    var lessThanCheck = new DiscordEmbedBuilder
                    {
                        Title = "You cannot bet less than 0!",
                        Description = "Nice try suckka!",
                        Color = DiscordColor.HotPink,
                    };

                    lessThanCheck.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = lessThanCheck,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                    return;
                }

                if (profileCheck.Gold < bet)
                {
                    var profileCheckFail = new DiscordEmbedBuilder
                    {
                        Title = "You can't bet that much!",
                        Description = $"Seems like you're too poor to afford to bet {bet:###,###,###,###,###} {currencyName}! Try betting a smaller amount... We have shown you how much {currencyName} you have below!"
                    };

                    if (profileCheck.Gold == 0) { profileCheckFail.AddField(currencyName, profileCheck.Gold.ToString()); }
                    if (profileCheck.Gold >= 1) { profileCheckFail.AddField(currencyName, profileCheck.Gold.ToString("###,###,###,###,###")); }

                    profileCheckFail.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = profileCheckFail,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                    return;
                }

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
                        Description = $"You just won {winAmount:###,###,###,###,###} {currencyName}!",
                        Color = DiscordColor.SpringGreen
                    };

                    winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = winEmbed,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                    return;
                }

                if (winOrLose == 1)
                {
                    await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                    var loseEmbed = new DiscordEmbedBuilder
                    {
                        Title = "You lost the spin!",
                        Description = $"You just lost {bet:###,###,###,###,###} {currencyName}!",
                        Color = DiscordColor.IndianRed
                    };

                    loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = loseEmbed,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                    return;
                }
            }
        }

        [Command("spin")]
        [Description("Spins a wheel and gets a random bonus!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task SpinAllGold(CommandContext ctx, string commandString)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            var CNConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                if (commandString == "all")
                {
                    var rnd = new Random();
                    var winOrLose = rnd.Next(1, 3);

                    Profile profileCheck = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                    if (profileCheck.Gold > 1000000)
                    {
                        var maxLimitCheck = new DiscordEmbedBuilder
                        {
                            Title = $"You can't bet more than 1,000,000 {currencyName}!",
                            Description = "Please try again in 1 min",
                            Color = DiscordColor.HotPink,
                        };

                        maxLimitCheck.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = maxLimitCheck,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                        return;
                    }

                    if (profileCheck.Gold <= 0)
                    {
                        var lessThanCheck = new DiscordEmbedBuilder
                        {
                            Title = $"You cannot bet 0 {currencyName} or less!",
                            Description = "Nice try suckka!",
                            Color = DiscordColor.HotPink,
                        };

                        lessThanCheck.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = lessThanCheck,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

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
                            Description = $"You just won {winAmount:###,###,###,###,###} {currencyName}!",
                            Color = DiscordColor.SpringGreen
                        };

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = winEmbed,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                        return;
                    }

                    if (winOrLose == 1)
                    {
                        await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                        var loseEmbed = new DiscordEmbedBuilder
                        {
                            Title = "You lost the spin!",
                            Description = $"You just lost {bet:###,###,###,###,###} {currencyName}!",
                            Color = DiscordColor.IndianRed
                        };

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = loseEmbed,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                        return;
                    }

                    return;
                }
            }
        }

        [Command("steal")]
        [Description("Steal from a specific person a random amount")]
        public async Task Steal(CommandContext ctx)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to specify a user!",
                    Description = "For example: `!steal @DJKoston`",
                    Color = DiscordColor.IndianRed,
                };

                errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = errorEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }           
        }

        [Command("steal")]
        [Description("Steal from a specific person a random amount")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task Steal(CommandContext ctx, DiscordMember member)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            var CNConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var rnd = new Random();
                var winOrLose = rnd.Next(1, 3);

                Profile thiefProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                Profile victimProfile = await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

                if (ctx.User.Username == member.Username)
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = "You want to try and steal from yourself?",
                        Description = "Who do you think you are? Chuck Fucking Norris??",
                        Color = DiscordColor.IndianRed,
                    };

                    errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = errorEmbed,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                    return;
                }

                if (victimProfile.Gold <= 0)
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"You cannot steal from {member.DisplayName}",
                        Description = $"They don't have any {currencyName}! Talk about trying to kick a man while he's down!",
                        Color = DiscordColor.IndianRed,
                    };

                    errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = errorEmbed,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

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
                            Title = $"You successfully stole {rndSteal:###,###,###,###,###} {currencyName} from {member.DisplayName}!",
                            Color = DiscordColor.SpringGreen
                        };

                        Profile thiefWinProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                        Profile victimLoseProfile = await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

                        if (thiefProfile.Gold == 0) { winEmbed.AddField(currencyName, thiefWinProfile.Gold.ToString()); }
                        if (thiefProfile.Gold >= 1) { winEmbed.AddField(currencyName, thiefWinProfile.Gold.ToString("###,###,###,###,###")); }

                        if (victimLoseProfile.Gold == 0) { winEmbed.AddField($"Their {currencyName}", victimLoseProfile.Gold.ToString()); }
                        if (victimLoseProfile.Gold >= 1) { winEmbed.AddField($"Their {currencyName}", victimLoseProfile.Gold.ToString("###,###,###,###,###")); }

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = winEmbed,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

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

                        if (thiefProfile.Gold == 0) { loseEmbed.AddField(currencyName, thiefLoseProfile.Gold.ToString()); }
                        if (thiefProfile.Gold >= 1) { loseEmbed.AddField(currencyName, thiefLoseProfile.Gold.ToString("###,###,###,###,###")); }

                        if (victimWinProfile.Gold == 0) { loseEmbed.AddField($"Their {currencyName}", victimWinProfile.Gold.ToString()); }
                        if (victimWinProfile.Gold >= 1) { loseEmbed.AddField($"Their {currencyName}", victimWinProfile.Gold.ToString("###,###,###,###,###")); }

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder = new DiscordMessageBuilder
                        {
                            Embed = loseEmbed,
                        };

                        messageBuilder.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                        return;
                    }
                }
            }
        }

        [Command("guess")]
        [Description("Guess a number between 1 and 5, if you're right - Win 2500 Currency if you are right!!")]
        public async Task GuessTheNumber(CommandContext ctx)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick a number between 1 and 10 to make a guess.",
                    Description = "For example `!guess 5`",
                    Color = DiscordColor.CornflowerBlue,
                };

                errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = errorEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }
        }

        [Command("guess")]
        [Description("Guess a number between 1 and 10, if you're right - Win 2,500 Currency if you are right!!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task GuessTheNumber(CommandContext ctx, int guess)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            var CNConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var rnd = new Random();
                int maxNumber = 11;
                var Number = rnd.Next(1, maxNumber);

                if (guess > maxNumber)
                {
                    var maxEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"You can't guess higher than {maxNumber - 1}",
                        Description = $"Try guessing a number below {maxNumber - 1}",
                        Color = DiscordColor.IndianRed
                    };

                    maxEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder1 = new DiscordMessageBuilder
                    {
                        Embed = maxEmbed,
                    };

                    messageBuilder1.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder1).ConfigureAwait(false);

                    return;
                }

                if (guess == Number)
                {
                    await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                    await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, 2500, ctx.Member.Username);

                    Profile profile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                    var winEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"You correctly guessed the number {Number}",
                        Description = $"You just won 2,500 {currencyName}!",
                        Color = DiscordColor.SpringGreen,
                    };

                    winEmbed.AddField($"Your {currencyName}:", profile.Gold.ToString());
                    winEmbed.AddField("The number has now changed!", "Can you guess it now?");
                    winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    var messageBuilder2 = new DiscordMessageBuilder
                    {
                        Embed = winEmbed,
                    };

                    messageBuilder2.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder2).ConfigureAwait(false);

                    return;
                }

                var loseEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Nice try, but the number was {Number}",
                    Description = "The number has now changed! Can you guess it now?",
                    Color = DiscordColor.IndianRed
                };

                loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = loseEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }
        }

        [Command("guess")]
        [Description("Guess a number between 1 and 10, if you're right - Win 2500 Currency if you are right!!")]
        public async Task GuessTheNumber(CommandContext ctx, string text)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            text.Remove(0);

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick a number between 1 and 10 to make a guess.",
                    Description = "For example `!guess 5`",
                    Color = DiscordColor.CornflowerBlue,
                };

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = errorEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }
        }

        [Command("coinflip")]
        [Description("")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task FlipACoin(CommandContext ctx)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick Head's or Tails!",
                    Description = "For example `!coinflip heads`",
                    Color = DiscordColor.CornflowerBlue,
                };

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = errorEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }
        }

        [Command("coinflip")]
        [Description("Flip a coin! If you guess it, you get 500 Currency back!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task FlipACoin(CommandContext ctx, string guess)
        {
            var configChannel = await _gameChannelConfigService.GetGameChannelConfigService(ctx.Guild.Id);

            var CNConfig = await _currencyNameService.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            if (ctx.Channel.Id == configChannel.ChannelId)
            {
                await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                var rnd = new Random();
                var outcome = rnd.Next(1, 3).ToString();

                string heads = 1.ToString();
                string tails = 2.ToString();

                if (guess.ToLower() == "heads")
                {
                    if (outcome == heads)
                    {
                        await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, 500, ctx.Member.Username);

                        var winEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Heads!",
                            Description = $"You just won 500 {currencyName}!",
                            Color = DiscordColor.SpringGreen,
                        };

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder1 = new DiscordMessageBuilder
                        {
                            Embed = winEmbed,
                        };

                        messageBuilder1.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder1).ConfigureAwait(false);

                        return;
                    }

                    if (outcome == tails)
                    {

                        var loseEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Tails!",
                            Description = $"You just lost the toss!",
                            Color = DiscordColor.IndianRed,
                        };

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder2 = new DiscordMessageBuilder
                        {
                            Embed = loseEmbed,
                        };

                        messageBuilder2.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder2).ConfigureAwait(false);

                        return;
                    }
                }

                if (guess.ToLower() == "tails")
                {
                    if (outcome == tails)
                    {
                        await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, 500, ctx.Member.Username);

                        var winEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Tails!",
                            Description = $"You just won 500 {currencyName}!",
                            Color = DiscordColor.SpringGreen,
                        };

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder3 = new DiscordMessageBuilder
                        {
                            Embed = winEmbed,
                        };

                        messageBuilder3.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder3).ConfigureAwait(false);

                        return;
                    }

                    if (outcome == heads)
                    {
                        var loseEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Heads!",
                            Description = "You just lost the toss!",
                            Color = DiscordColor.IndianRed,
                        };

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        var messageBuilder4 = new DiscordMessageBuilder
                        {
                            Embed = loseEmbed,
                        };

                        messageBuilder4.WithReply(ctx.Message.Id, true);

                        await ctx.Channel.SendMessageAsync(messageBuilder4).ConfigureAwait(false);

                        return;
                    }
                }

                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick Head's or Tails!",
                    Description = "For example `!coinflip heads`",
                    Color = DiscordColor.CornflowerBlue,
                };

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = errorEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            }
        }
    }
}
