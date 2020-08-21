using DiscordBot.Core.Services.Profiles;
using DiscordBot.Core.ViewModels;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class ProfileCommands : BaseCommandModule
    {
        private readonly RPGContext _context;
        private readonly IProfileService _profileService;
        private readonly IExperienceService _experienceService;
        private readonly IGoldService _goldService;

        public ProfileCommands(RPGContext context, IProfileService profileService, IExperienceService experienceService, IGoldService goldService)
        {
            _context = context;
            _profileService = profileService;
            _experienceService = experienceService;
            _goldService = goldService;
        }

        [Command("myinfo")]
        public async Task Profile(CommandContext ctx)
        {
            await GetProfileToDisplayAsync(ctx, ctx.Member.Id);
        }

        [Command("myinfo")]
        public async Task Profile(CommandContext ctx, DiscordMember member)
        {
            await GetProfileToDisplayAsync(ctx, member.Id);
        }

        private async Task GetProfileToDisplayAsync(CommandContext ctx, ulong memberId)
        {
            var memberUsername = ctx.Guild.Members[memberId].Username;

            var quotes = _context.Quotes.Count(x => x.DiscordUserQuotedId == memberId);
            var quotesby = _context.Quotes.Count(x => x.AddedById == memberId);

            Profile profile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

            var member = ctx.Guild.Members[profile.DiscordId];

            var profileEmbed = new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName}'s Profile",
                Color = DiscordColor.Gold
            };

            profileEmbed.WithThumbnail(member.AvatarUrl);

            if(profile.Gold == 0) { profileEmbed.AddField("Gold", profile.Gold.ToString()); }
            if(profile.Gold >= 1) { profileEmbed.AddField("Gold", profile.Gold.ToString("###,###,###,###,###")); };

            profileEmbed.AddField("XP", profile.XP.ToString("###,###,###,###,###"));
            profileEmbed.AddField("Level", profile.Level.ToString("###,###,###,###,###"));

            if(quotes == 0) { profileEmbed.AddField("You have been Quoted:", $"{quotes.ToString()} Times"); }
            if (quotes == 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes.ToString("###,###,###,###,###")} Time"); }
            if (quotes > 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes.ToString("###,###,###,###,###")} Times"); }

            if(quotesby == 0) { profileEmbed.AddField("You have Quoted:", $"{quotesby.ToString()} Quotes"); }
            if (quotesby == 1) { profileEmbed.AddField("You have Quoted:", $"{quotesby.ToString("###,###,###,###,###")} Quote"); }
            if (quotesby > 1) { profileEmbed.AddField("You have Quoted:", $"{quotesby.ToString("###,###,###,###,###")} Quotes"); }

            await ctx.Channel.SendMessageAsync(embed: profileEmbed).ConfigureAwait(false);
        }

        [Command("grantxp")]
        [RequireRoles(RoleCheckMode.Any,"Admin")]
        public async Task GrantXp(CommandContext ctx, DiscordMember member, int number)
        {
            await GiveXp(ctx, member.Id, number);
        }

        private async Task GiveXp(CommandContext ctx, ulong memberId, int xpGranted)
        {
            var member = ctx.Guild.Members[memberId];
            var memberUsername = ctx.Guild.Members[memberId].Username;

            GrantXpViewModel viewModel = await _experienceService.GrantXpAsync(memberId, ctx.Guild.Id, xpGranted, memberUsername);

            var XPAddedEmbed = new DiscordEmbedBuilder
            {
                Title = $"{xpGranted.ToString("###,###,###,###,###")} XP given to {member.DisplayName}!",
                Color = DiscordColor.Blurple
            };

            XPAddedEmbed.AddField("XP", viewModel.Profile.XP.ToString("###,###,###,###,###"));
            XPAddedEmbed.AddField("Level", viewModel.Profile.Level.ToString("###,###,###,###,###"));

            await ctx.Channel.SendMessageAsync(embed: XPAddedEmbed).ConfigureAwait(false);

            if (!viewModel.LevelledUp) { return; }

            var leveledUpEmbed = new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level.ToString("###,###,###,###,###")}!",
                Color = DiscordColor.Gold,
            };

            leveledUpEmbed.WithThumbnail(member.AvatarUrl);

            await ctx.Channel.SendMessageAsync(embed: leveledUpEmbed).ConfigureAwait(false);
        }

        [Command("givegold")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task GiveGold(CommandContext ctx, DiscordMember member, int number)
        {
            await GiveGold(ctx, member.Id, number);
        }

        private async Task GiveGold(CommandContext ctx, ulong memberId, int goldGranted)
        {
            var member = ctx.Guild.Members[memberId];
            var memberUsername = ctx.Guild.Members[memberId].Username;

            Profile oldProfile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

            await _goldService.GrantGoldAsync(memberId, ctx.Guild.Id, goldGranted, memberUsername);

            var GoldAddedEmbed = new DiscordEmbedBuilder
            {
                Title = $"{goldGranted.ToString("###,###,###,###,###")} Gold given to {member.DisplayName}!",
                Color = DiscordColor.Blurple
            };

            Profile newProfile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

            if (oldProfile.Gold == 0) { GoldAddedEmbed.AddField("Gold Before", oldProfile.Gold.ToString()); }
            if (oldProfile.Gold >= 1) { GoldAddedEmbed.AddField("Gold Before", oldProfile.Gold.ToString("###,###,###,###,###")); }

            if (newProfile.Gold == 0) { GoldAddedEmbed.AddField("Gold Now", newProfile.Gold.ToString()); }
            if (newProfile.Gold >= 1) { GoldAddedEmbed.AddField("Gold Now", newProfile.Gold.ToString("###,###,###,###,###")); }

            if (oldProfile.Gold == newProfile.Gold)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "An Error has occurred!",
                    Description = $"It appears an error has occurred while trying to add {goldGranted.ToString("###,###,###,###,###")} gold to {member.DisplayName}",
                    Color = DiscordColor.Red
                };

                errorEmbed.AddField("🤔", "Please try again or if it persists, please contact DJKoston");

                await ctx.Channel.SendMessageAsync(embed: errorEmbed);

                return;
            }

            await ctx.Channel.SendMessageAsync(embed: GoldAddedEmbed).ConfigureAwait(false);
        }

        [Command("pay")]
        [Description("Usage: !pay {user} {amount}")]
        public async Task PayUser(CommandContext ctx, DiscordMember member, int payAmount)
        {
            Profile userProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
            await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

            if (userProfile.Gold < payAmount)
            {
                var userProfileCheckFailEmbed = new DiscordEmbedBuilder
                {
                    Title = "You can't pay that much!",
                    Description = $"Seems like you're too poor to afford to pay {member.DisplayName} {payAmount.ToString("###,###,###,###,###")} Gold! Try paying them a smaller amount... We have shown you how much Gold you have below!",
                    Color = DiscordColor.IndianRed,
                };

                if (userProfile.Gold == 0) { userProfileCheckFailEmbed.AddField("Gold", userProfile.Gold.ToString()); }
                if (userProfile.Gold >= 1) { userProfileCheckFailEmbed.AddField("Gold", userProfile.Gold.ToString("###,###,###,###,###")); }

                await ctx.Channel.SendMessageAsync(embed: userProfileCheckFailEmbed).ConfigureAwait(false);

                return;
            }

            if(payAmount < 0)
            {

                var lessThanCheck = new DiscordEmbedBuilder
                {
                    Title = "You cannot pay less than 0!",
                    Description = "Nice try suckka!",
                    Color = DiscordColor.IndianRed,
                };

                await ctx.Channel.SendMessageAsync(embed: lessThanCheck).ConfigureAwait(false);

                return;
            }

            await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -payAmount, ctx.Member.Username);
            await _goldService.GrantGoldAsync(member.Id, ctx.Guild.Id, payAmount, ctx.Member.Username);

            var paidEmbed = new DiscordEmbedBuilder
            {
                Title = $"You have paid {member.DisplayName} {payAmount.ToString("###,###,###,###,###")} Gold!",
                Description = "Thank you for using the GG Bot Payment Network!",
                Color = DiscordColor.SpringGreen,
            };

            await ctx.Channel.SendMessageAsync(embed: paidEmbed).ConfigureAwait(false);
        }

        [Command("hourly")]
        [Description("Allows you to collect a random hourly 'Tax' ")]
        [Cooldown(1, 3600, CooldownBucketType.User)]
        public async Task HourlyCollect(CommandContext ctx)
        {
            await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

            var rndHourly = new Random();
            var hourlyCollect = rndHourly.Next(100, 1000);

            await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, hourlyCollect, ctx.Member.Username);

            var hourlyEmbed = new DiscordEmbedBuilder
            {
                Title = "You have collected your hourly 'Tax'!",
                Description = $"You just received {hourlyCollect.ToString("###,###,###,###,###")} Gold!",
                Color = DiscordColor.Cyan,
            };

            await ctx.Channel.SendMessageAsync(embed: hourlyEmbed);
        }

        [Command("daily")]
        [Description("Allows you to collect a random daily 'Tax' ")]
        [Cooldown(1, 86400, CooldownBucketType.User)]
        public async Task DailyCollect(CommandContext ctx)
        {
            await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

            var rndDaily = new Random();
            var dailyCollect = rndDaily.Next(100, 5000);

            await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, dailyCollect, ctx.Member.Username);

            var dailyEmbed = new DiscordEmbedBuilder
            {
                Title = "You have collected your Daily 'Tax'!",
                Description = $"You just received {dailyCollect.ToString("###,###,###,###,###")} Gold!",
                Color = DiscordColor.Cyan,
            };

            await ctx.Channel.SendMessageAsync(embed: dailyEmbed);
        }

        [Command("buildprofiletable")]
        [Description("Bring all users in the server to the Profile Table")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task BuildProfileTable(CommandContext ctx)
        {
            var members = await ctx.Guild.GetAllMembersAsync().ConfigureAwait(false);
            var profiles = members.Where(x => x.IsBot == false);

            foreach (DiscordMember profile in profiles)
            {
                if (profile.IsBot)
                {
                    return;
                }

                await _profileService.GetOrCreateProfileAsync(profile.Id, ctx.Guild.Id, profile.Username);

                await ctx.Channel.SendMessageAsync($"Profile created for {profile.Mention}").ConfigureAwait(false);

                Thread.Sleep(1000);
            }

            return;
            
        }

        [Command("top10")]
        [Description("Displays the Top 10 users")]
        public async Task Top10Blank(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("You need to specify whether you want `!top10 Gold` or `!top10 XP` only!");
        }

        [Command("top10")]
        [Description("Displays the Top 10 users!")]
        public async Task Top10(CommandContext ctx, string XPorGold)
        {
            if (XPorGold.ToLower() == "xp")
            {
                var xplist = _context.Profiles.Where(x => x.XP >= 0 && x.GuildId == ctx.Guild.Id).OrderByDescending(x => x.XP).Take(10).AsNoTracking().ToList();

                // Gets User Profile in List above.
                var first = xplist[0];
                var second = xplist[1];
                var third = xplist[2];
                var fourth = xplist[3];
                var fifth = xplist[4];
                var sixth = xplist[5];
                var seventh = xplist[6];
                var eighth = xplist[7];
                var ninth = xplist[8];
                var tenth = xplist[9];

                // Gets User Display Name in Discord
                DiscordMember member1 = await ctx.Guild.GetMemberAsync(first.DiscordId);
                DiscordMember member2 = await ctx.Guild.GetMemberAsync(second.DiscordId);
                DiscordMember member3 = await ctx.Guild.GetMemberAsync(third.DiscordId);
                DiscordMember member4 = await ctx.Guild.GetMemberAsync(fourth.DiscordId);
                DiscordMember member5 = await ctx.Guild.GetMemberAsync(fifth.DiscordId);
                DiscordMember member6 = await ctx.Guild.GetMemberAsync(sixth.DiscordId);
                DiscordMember member7 = await ctx.Guild.GetMemberAsync(seventh.DiscordId);
                DiscordMember member8 = await ctx.Guild.GetMemberAsync(eighth.DiscordId);
                DiscordMember member9 = await ctx.Guild.GetMemberAsync(ninth.DiscordId);
                DiscordMember member10 = await ctx.Guild.GetMemberAsync(tenth.DiscordId);

                var leaderboardEmbed = new DiscordEmbedBuilder
                {
                    Title = $"XP Leaderboard\n{member1.DisplayName} is currently in 1st!",
                    Description = "Here are the top 10 XP gainers in the Discord!",
                    Color = member1.Color,
                };

                leaderboardEmbed.WithFooter($"Leaderboard correct as of {DateTime.Now}");
                leaderboardEmbed.WithThumbnail(member1.AvatarUrl);
                leaderboardEmbed.AddField("1st", $"{member1.DisplayName} - {first.XP.ToString("###,###,###,###,###")} XP - Level {first.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("2nd", $"{member2.DisplayName} - {second.XP.ToString("###,###,###,###,###")} XP - Level {second.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("3rd", $"{member3.DisplayName} - {third.XP.ToString("###,###,###,###,###")} XP - Level {third.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("4th", $"{member4.DisplayName} - {fourth.XP.ToString("###,###,###,###,###")} XP - Level {fourth.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("5th", $"{member5.DisplayName} - {fifth.XP.ToString("###,###,###,###,###")} XP - Level {fifth.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("6th", $"{member6.DisplayName} - {sixth.XP.ToString("###,###,###,###,###")} XP - Level {sixth.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("7th", $"{member7.DisplayName} - {seventh.XP.ToString("###,###,###,###,###")} XP - Level {seventh.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("8th", $"{member8.DisplayName} - {eighth.XP.ToString("###,###,###,###,###")} XP - Level {eighth.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("9th", $"{member9.DisplayName} - {ninth.XP.ToString("###,###,###,###,###")} XP - Level {ninth.Level.ToString("###,###,###,###,###")}");
                leaderboardEmbed.AddField("10th", $"{member10.DisplayName} - {tenth.XP.ToString("###,###,###,###,###")} XP - Level {tenth.Level.ToString("###,###,###,###,###")}");

                await ctx.Channel.SendMessageAsync(embed: leaderboardEmbed).ConfigureAwait(false);

                return;
            }

            if (XPorGold.ToLower() == "gold")
            {
                var xplist = _context.Profiles.Where(x => x.XP >= 1 && x.GuildId == ctx.Guild.Id).OrderByDescending(x => x.Gold).Take(10).AsNoTracking().ToList();

                // Gets User Profile in List above.
                var first = xplist[0];
                var second = xplist[1];
                var third = xplist[2];
                var fourth = xplist[3];
                var fifth = xplist[4];
                var sixth = xplist[5];
                var seventh = xplist[6];
                var eighth = xplist[7];
                var ninth = xplist[8];
                var tenth = xplist[9];

                // Gets User Display Name in Discord
                DiscordMember member1 = await ctx.Guild.GetMemberAsync(first.DiscordId);
                DiscordMember member2 = await ctx.Guild.GetMemberAsync(second.DiscordId);
                DiscordMember member3 = await ctx.Guild.GetMemberAsync(third.DiscordId);
                DiscordMember member4 = await ctx.Guild.GetMemberAsync(fourth.DiscordId);
                DiscordMember member5 = await ctx.Guild.GetMemberAsync(fifth.DiscordId);
                DiscordMember member6 = await ctx.Guild.GetMemberAsync(sixth.DiscordId);
                DiscordMember member7 = await ctx.Guild.GetMemberAsync(seventh.DiscordId);
                DiscordMember member8 = await ctx.Guild.GetMemberAsync(eighth.DiscordId);
                DiscordMember member9 = await ctx.Guild.GetMemberAsync(ninth.DiscordId);
                DiscordMember member10 = await ctx.Guild.GetMemberAsync(tenth.DiscordId);

                var leaderboardEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Gold Leaderboard\n{member1.DisplayName} is currently in 1st!",
                    Description = "Here are the top 10 Gold gainers in the Discord!",
                    Color = member1.Color,
                };

                leaderboardEmbed.WithFooter($"Leaderboard correct as of {DateTime.Now}");
                leaderboardEmbed.WithThumbnail(member1.AvatarUrl);
                leaderboardEmbed.AddField("1st", $"{member1.DisplayName} - {first.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("2nd", $"{member2.DisplayName} - {second.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("3rd", $"{member3.DisplayName} - {third.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("4th", $"{member4.DisplayName} - {fourth.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("5th", $"{member5.DisplayName} - {fifth.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("6th", $"{member6.DisplayName} - {sixth.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("7th", $"{member7.DisplayName} - {seventh.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("8th", $"{member8.DisplayName} - {eighth.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("9th", $"{member9.DisplayName} - {ninth.Gold.ToString("###,###,###,###,###")} Gold");
                leaderboardEmbed.AddField("10th", $"{member10.DisplayName} - {tenth.Gold.ToString("###,###,###,###,###")} Gold");

                await ctx.Channel.SendMessageAsync(embed: leaderboardEmbed).ConfigureAwait(false);

                return;
            }
            await ctx.Channel.SendMessageAsync("You need to specify whether you want `!top10 Gold` or `!top10 XP` only!");

            return;
        }

    }
}
