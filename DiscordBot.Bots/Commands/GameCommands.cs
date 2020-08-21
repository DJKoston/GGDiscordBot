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

        public GameCommands(IProfileService profileService, IGoldService goldService)
        {
            _profileService = profileService;
            _goldService = goldService;
        }

        [Command("spin")]
        [Description("Spins a wheel and gets a random bonus!")]
        public async Task SpinTheWheel(CommandContext ctx)
        {
            if(ctx.Channel.Name == "discord-games")
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to specify an amount to spin!",
                    Description = "For example: `!spin 2015` or `!spin all`",
                    Color = DiscordColor.IndianRed,
                };

                errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);
            }

            return;
        }

        [Command("spin")]
        [Description("Spins a wheel and gets a random bonus!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        
        public async Task SpinTheWheel(CommandContext ctx, int bet)
        {
            if (ctx.Channel.Name == "discord-games")
            {
                var rnd = new Random();
                var winOrLose = rnd.Next(1, 3);

                Profile profileCheck = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                if(bet > 1000000)
                {
                    var maxLimitCheck = new DiscordEmbedBuilder
                    {
                        Title = "You can't bet more than 1,000,000 Gold!",
                        Description = "Please try again in 1 min",
                        Color = DiscordColor.HotPink,
                    };

                    maxLimitCheck.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    await ctx.Channel.SendMessageAsync(embed: maxLimitCheck).ConfigureAwait(false);

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

                    await ctx.Channel.SendMessageAsync(embed: lessThanCheck).ConfigureAwait(false);

                    return;
                }

                if (profileCheck.Gold < bet)
                {
                    var profileCheckFail = new DiscordEmbedBuilder
                    {
                        Title = "You can't bet that much!",
                        Description = $"Seems like you're too poor to afford to bet {bet:###,###,###,###,###} Gold! Try betting a smaller amount... We have shown you how much Gold you have below!"
                    };

                    if (profileCheck.Gold == 0) { profileCheckFail.AddField("Gold", profileCheck.Gold.ToString()); }
                    if (profileCheck.Gold >= 1) { profileCheckFail.AddField("Gold", profileCheck.Gold.ToString("###,###,###,###,###")); }

                    profileCheckFail.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    await ctx.Channel.SendMessageAsync(embed: profileCheckFail).ConfigureAwait(false);

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
                        Description = $"You just won {winAmount:###,###,###,###,###} Gold!",
                        Color = DiscordColor.SpringGreen
                    };

                    winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    await ctx.Channel.SendMessageAsync(embed: winEmbed).ConfigureAwait(false);

                    return;
                }

                if (winOrLose == 1)
                {

                    await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                    var loseEmbed = new DiscordEmbedBuilder
                    {
                        Title = "You lost the spin!",
                        Description = $"You just lost {bet:###,###,###,###,###} Gold!",
                        Color = DiscordColor.IndianRed
                    };

                    loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    await ctx.Channel.SendMessageAsync(embed: loseEmbed).ConfigureAwait(false);

                    return;

                
                }
            }
        }

        [Command("spin")]
        [Description("Spins a wheel and gets a random bonus!")]
        [Cooldown(1, 60, CooldownBucketType.User)]

        public async Task SpinAllGold(CommandContext ctx, string commandString)
        {
            if (ctx.Channel.Name == "discord-games")
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
                            Title = "You can't bet more than 1,000,000 Gold!",
                            Description = "Please try again in 1 min",
                            Color = DiscordColor.HotPink,
                        };

                        maxLimitCheck.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: maxLimitCheck).ConfigureAwait(false);

                        return;
                    }


                    if (profileCheck.Gold <= 0)
                    {
                        var lessThanCheck = new DiscordEmbedBuilder
                        {
                            Title = "You cannot bet 0 Gold or less!",
                            Description = "Nice try suckka!",
                            Color = DiscordColor.HotPink,
                        };

                        lessThanCheck.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: lessThanCheck).ConfigureAwait(false);

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
                            Description = $"You just won {winAmount:###,###,###,###,###} Gold!",
                            Color = DiscordColor.SpringGreen
                        };

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: winEmbed).ConfigureAwait(false);

                        return;
                    }

                    if (winOrLose == 1)
                    {

                        await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -bet, ctx.Member.Username);

                        var loseEmbed = new DiscordEmbedBuilder
                        {
                            Title = "You lost the spin!",
                            Description = $"You just lost {bet:###,###,###,###,###} Gold!",
                            Color = DiscordColor.IndianRed
                        };

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: loseEmbed).ConfigureAwait(false);

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
            if (ctx.Channel.Name == "discord-games")
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to specify a user!",
                    Description = "For example: `!steal @DJKoston`",
                    Color = DiscordColor.IndianRed,
                };

                errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);
            }           
        }

        [Command("steal")]
        [Description("Steal from a specific person a random amount")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task Steal(CommandContext ctx, DiscordMember member)
        {
            if (ctx.Channel.Name == "discord-games")
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

                    await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);

                    return;
                }

                if (victimProfile.Gold <= 0)
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"You cannot steal from {member.DisplayName}",
                        Description = "They don't have any gold! Talk about trying to kick a man while he's down!",
                        Color = DiscordColor.IndianRed,
                    };

                    errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);

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
                            Title = $"You successfully stole {rndSteal:###,###,###,###,###} Gold from {member.DisplayName}!",
                            Color = DiscordColor.SpringGreen
                        };

                        Profile thiefWinProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                        Profile victimLoseProfile = await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

                        if (thiefProfile.Gold == 0) { winEmbed.AddField("Gold", thiefWinProfile.Gold.ToString()); }
                        if (thiefProfile.Gold >= 1) { winEmbed.AddField("Gold", thiefWinProfile.Gold.ToString("###,###,###,###,###")); }

                        if (victimLoseProfile.Gold == 0) { winEmbed.AddField("Their Gold", victimLoseProfile.Gold.ToString()); }
                        if (victimLoseProfile.Gold >= 1) { winEmbed.AddField("Their Gold", victimLoseProfile.Gold.ToString("###,###,###,###,###")); }

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: winEmbed).ConfigureAwait(false);

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

                        if (thiefProfile.Gold == 0) { loseEmbed.AddField("Gold", thiefLoseProfile.Gold.ToString()); }
                        if (thiefProfile.Gold >= 1) { loseEmbed.AddField("Gold", thiefLoseProfile.Gold.ToString("###,###,###,###,###")); }

                        if (victimWinProfile.Gold == 0) { loseEmbed.AddField("Their Gold", victimWinProfile.Gold.ToString()); }
                        if (victimWinProfile.Gold >= 1) { loseEmbed.AddField("Their Gold", victimWinProfile.Gold.ToString("###,###,###,###,###")); }

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: loseEmbed).ConfigureAwait(false);

                        return;
                    }
                }
            }
        }

        [Command("guess")]
        [Description("Guess a number between 1 and 5, if you're right - Win 2500 Gold if you are right!!")]
        public async Task GuessTheNumber(CommandContext ctx)
        {
            if (ctx.Channel.Name == "discord-games")
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick a number between 1 and 10 to make a guess.",
                    Description = "For example `!guess 5`",
                    Color = DiscordColor.CornflowerBlue,
                };
                errorEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);
            }
        }

        [Command("guess")]
        [Description("Guess a number between 1 and 5, if you're right - Win 2,500 Gold if you are right!!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task GuessTheNumber(CommandContext ctx, int guess)
        {
            if (ctx.Channel.Name == "discord-games")
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

                    await ctx.Channel.SendMessageAsync(embed: maxEmbed).ConfigureAwait(false);

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
                        Description = "You just won 2,500 Gold!",
                        Color = DiscordColor.SpringGreen,
                    };

                    winEmbed.AddField("Your Gold:", profile.Gold.ToString());
                    winEmbed.AddField("The number has now changed!", "Can you guess it now?");
                    winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                    await ctx.Channel.SendMessageAsync(embed: winEmbed).ConfigureAwait(false);

                    return;
                }

                var loseEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Nice try, but the number was {Number}",
                    Description = "The number has now changed! Can you guess it now?",
                    Color = DiscordColor.IndianRed
                };

                loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                await ctx.Channel.SendMessageAsync(embed: loseEmbed).ConfigureAwait(false);
            }
        }

        [Command("guess")]
        [Description("Guess a number between 1 and 10, if you're right - Win 2500 Gold if you are right!!")]
        public async Task GuessTheNumber(CommandContext ctx, string text)
        {
            if (ctx.Channel.Name == "discord-games")
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick a number between 1 and 10 to make a guess.",
                    Description = "For example `!guess 5`",
                    Color = DiscordColor.CornflowerBlue,
                };

                await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);
            }
        }

        [Command("coinflip")]
        [Description("Flip a coin, auto bet 50 gold! If you guess it, you get 100 back!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task FlipACoin(CommandContext ctx)
        {
            if (ctx.Channel.Name == "discord-games")
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick Head's or Tails!",
                    Description = "For example `!coinflip heads`",
                    Color = DiscordColor.CornflowerBlue,
                };

                await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);
            }

        }

        [Command("coinflip")]
        [Description("Flip a coin, auto bet 50 gold! If you guess it, you get 100 back!")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task FlipACoin(CommandContext ctx, string guess)
        {
            if (ctx.Channel.Name == "discord-games")
            {
                await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -50, ctx.Member.Username);

                var rnd = new Random();
                var outcome = rnd.Next(1, 3).ToString();

                string heads = 1.ToString();
                string tails = 2.ToString();

                if (guess.ToLower() == "heads")
                {
                    if (outcome == heads)
                    {
                        await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, 100, ctx.Member.Username);

                        var winEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Heads!",
                            Description = "You just won 50 Gold!",
                            Color = DiscordColor.SpringGreen,
                        };

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: winEmbed).ConfigureAwait(false);

                        return;
                    }

                    if (outcome == tails)
                    {

                        var loseEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Tails!",
                            Description = "You just lost 50 Gold!",
                            Color = DiscordColor.IndianRed,
                        };

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: loseEmbed).ConfigureAwait(false);

                        return;
                    }
                }

                if (guess.ToLower() == "tails")
                {
                    if (outcome == tails)
                    {
                        await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, 100, ctx.Member.Username);

                        var winEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Tails!",
                            Description = "You just won 50 Gold!",
                            Color = DiscordColor.SpringGreen,
                        };

                        winEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: winEmbed).ConfigureAwait(false);

                        return;
                    }

                    if (outcome == heads)
                    {
                        var loseEmbed = new DiscordEmbedBuilder
                        {
                            Title = "The coin landed on Heads!",
                            Description = "You just lost 50 Gold!",
                            Color = DiscordColor.IndianRed,
                        };

                        loseEmbed.AddField("There is a cooldown for this command!", "You can run it again in 1 min.");

                        await ctx.Channel.SendMessageAsync(embed: loseEmbed).ConfigureAwait(false);

                        return;
                    }

                }

                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "You need to pick Head's or Tails!",
                    Description = "For example `!coinflip heads`",
                    Color = DiscordColor.CornflowerBlue,
                };

                await ctx.Channel.SendMessageAsync(embed: errorEmbed).ConfigureAwait(false);
            }
        }
            
    }
}
