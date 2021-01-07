﻿using DiscordBot.Core.Services.Configs;
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
        private readonly INitroBoosterRoleConfigService _nitroBoosterRoleConfigService;

        public ProfileCommands(RPGContext context, IProfileService profileService, IExperienceService experienceService, IGoldService goldService, INitroBoosterRoleConfigService nitroBoosterRoleConfigService)
        {
            _context = context;
            _profileService = profileService;
            _experienceService = experienceService;
            _goldService = goldService;
            _nitroBoosterRoleConfigService = nitroBoosterRoleConfigService;
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
            var NBConfig = _nitroBoosterRoleConfigService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            if(NBConfig == null)
            {
                var memberUsername = ctx.Guild.Members[memberId].Username;

                var quotescount = _context.Quotes.Where(x => x.GuildId == ctx.Guild.Id);
                var quotes = quotescount.Count(x => x.DiscordUserQuotedId == memberId);
                var quotesby = quotescount.Count(x => x.AddedById == memberId);

                Profile profile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

                var member = ctx.Guild.Members[profile.DiscordId];

                var profileEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName}'s Profile",
                    Color = member.Color
                };

                profileEmbed.WithThumbnail(member.AvatarUrl);

                var nextLevel = _context.ToNextXP.FirstOrDefault(x => x.Level == profile.Level + 1).XPAmount;

                profileEmbed.AddField("XP", $"{profile.XP.ToString("###,###,###,###,###")} / {nextLevel.ToString("###,###,###,###,###")}");
                profileEmbed.AddField("Level", profile.Level.ToString("###,###,###,###,###"));

                if (profile.Gold == 0) { profileEmbed.AddField("Gold", profile.Gold.ToString()); }
                if (profile.Gold >= 1) { profileEmbed.AddField("Gold", profile.Gold.ToString("###,###,###,###,###")); };

                if (quotes == 0) { profileEmbed.AddField("You have been Quoted:", $"{quotes} Times"); }
                if (quotes == 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Time"); }
                if (quotes > 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Times"); }

                if (quotesby == 0) { profileEmbed.AddField("You have Quoted:", $"{quotesby} Quotes"); }
                if (quotesby == 1) { profileEmbed.AddField("You have Quoted:", $"{quotesby:###,###,###,###,###} Quote"); }
                if (quotesby > 1) { profileEmbed.AddField("You have Quoted:", $"{quotesby:###,###,###,###,###} Quotes"); }

                await ctx.Channel.SendMessageAsync(embed: profileEmbed).ConfigureAwait(false);
            }

            else
            {
                var memberUsername = ctx.Guild.Members[memberId].Username;

                var NitroBoosterRole = ctx.Guild.GetRole(NBConfig.RoleId);

                var quotescount = _context.Quotes.Where(x => x.GuildId == ctx.Guild.Id);
                var quotes = quotescount.Count(x => x.DiscordUserQuotedId == memberId);
                var quotesby = quotescount.Count(x => x.AddedById == memberId);

                Profile profile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

                var member = ctx.Guild.Members[profile.DiscordId];

                var profileEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName}'s Profile",
                    Color = member.Color
                };

                profileEmbed.WithThumbnail(member.AvatarUrl);

                if (profile.Gold == 0) { profileEmbed.AddField("Gold", profile.Gold.ToString()); }
                if (profile.Gold >= 1) { profileEmbed.AddField("Gold", profile.Gold.ToString("###,###,###,###,###")); };

                var nextLevel = _context.ToNextXP.FirstOrDefault(x => x.Level == profile.Level + 1).XPAmount;

                profileEmbed.AddField("XP", $"{profile.XP.ToString("###,###,###,###,###")} / {nextLevel.ToString("###,###,###,###,###")}");
                profileEmbed.AddField("Level", profile.Level.ToString("###,###,###,###,###"));

                if (quotes == 0) { profileEmbed.AddField("You have been Quoted:", $"{quotes} Times"); }
                if (quotes == 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Time"); }
                if (quotes > 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Times"); }

                if (quotesby == 0) { profileEmbed.AddField("You have Quoted:", $"{quotesby} Quotes"); }
                if (quotesby == 1) { profileEmbed.AddField("You have Quoted:", $"{quotesby:###,###,###,###,###} Quote"); }
                if (quotesby > 1) { profileEmbed.AddField("You have Quoted:", $"{quotesby:###,###,###,###,###} Quotes"); }

                if (member.Roles.Contains(NitroBoosterRole)) { profileEmbed.AddField("You have the Double XP Role!", "You currently get 2x XP and 2x Tax!"); }

                await ctx.Channel.SendMessageAsync(embed: profileEmbed).ConfigureAwait(false);
            }
        }

        [Command("grantxp")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
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
                Title = $"{xpGranted:###,###,###,###,###} XP given to {member.DisplayName}!",
                Color = DiscordColor.Blurple
            };

            XPAddedEmbed.AddField("XP", viewModel.Profile.XP.ToString("###,###,###,###,###"));
            XPAddedEmbed.AddField("Level", viewModel.Profile.Level.ToString("###,###,###,###,###"));

            await ctx.Channel.SendMessageAsync(embed: XPAddedEmbed).ConfigureAwait(false);

            if (!viewModel.LevelledUp) { return; }

            var leveledUpEmbed = new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName} is now Level {viewModel.Profile.Level:###,###,###,###,###}!",
                Color = DiscordColor.Gold,
            };

            leveledUpEmbed.WithThumbnail(member.AvatarUrl);

            await ctx.Channel.SendMessageAsync(embed: leveledUpEmbed).ConfigureAwait(false);
        }

        [Command("givegold")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
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
                Title = $"{goldGranted:###,###,###,###,###} Gold given to {member.DisplayName}!",
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
                    Description = $"It appears an error has occurred while trying to add {goldGranted:###,###,###,###,###} gold to {member.DisplayName}",
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
                    Title = $"You can't pay that much {ctx.Member.DisplayName}!",
                    Description = $"Seems like you're too poor to afford to pay {member.DisplayName} {payAmount:###,###,###,###,###} Gold! Try paying them a smaller amount... We have shown you how much Gold you have below!",
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
                    Title = $"You cannot pay less than 0 {ctx.Member.DisplayName}!",
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
                Title = $"You have paid {member.DisplayName} {payAmount:###,###,###,###,###} Gold!",
                Description = $"Thank you for using the GG Bot Payment Network {ctx.Member.DisplayName}!",
                Color = DiscordColor.SpringGreen,
            };

            await ctx.Channel.SendMessageAsync(embed: paidEmbed).ConfigureAwait(false);
        }

        [Command("hourly")]
        [Description("Allows you to collect a random hourly 'Tax' ")]
        [Cooldown(1, 3600, CooldownBucketType.User)]
        public async Task HourlyCollect(CommandContext ctx)
        {
            var NBConfig = _nitroBoosterRoleConfigService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            var NitroBoosterRole = ctx.Guild.GetRole(NBConfig.RoleId);

            if (ctx.Member.Roles.Contains(NitroBoosterRole))
            {
                await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                var rndHourly = new Random();
                int hourlyCollect = rndHourly.Next(100, 1000);
                int hourlyCollectNitro = hourlyCollect * 2;

                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, hourlyCollectNitro, ctx.Member.Username);

                var hourlyEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{ctx.Member.DisplayName} has just collected their hourly 'Tax'!",
                    Description = $"You just received {hourlyCollectNitro:###,###,###,###,###} Gold!",
                    Color = DiscordColor.Cyan,
                };

                hourlyEmbed.AddField("You collected 2x Normal Tax", "Because you have the Double XP Role!");

                await ctx.Channel.SendMessageAsync(embed: hourlyEmbed);

                return;
            }

            else
            {
                await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                var rndHourly = new Random();
                var hourlyCollect = rndHourly.Next(100, 1000);

                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, hourlyCollect, ctx.Member.Username);

                var hourlyEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{ctx.Member.DisplayName} has just collected their hourly 'Tax'!",
                    Description = $"You just received {hourlyCollect:###,###,###,###,###} Gold!",
                    Color = DiscordColor.Cyan,
                };

                await ctx.Channel.SendMessageAsync(embed: hourlyEmbed);

                return;
            }

            
        }

        [Command("daily")]
        [Description("Allows you to collect a random daily 'Tax' ")]
        [Cooldown(1, 86400, CooldownBucketType.User)]
        public async Task DailyCollect(CommandContext ctx)
        {
            var NBConfig = _nitroBoosterRoleConfigService.GetNitroBoosterConfig(ctx.Guild.Id).Result;

            var NitroBoosterRole = ctx.Guild.GetRole(NBConfig.RoleId);

            if (ctx.Member.Roles.Contains(NitroBoosterRole))
            {
                await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                var rndDaily = new Random();
                int dailyCollect = rndDaily.Next(500, 5000);
                int dailyCollectNitro = dailyCollect * 2;

                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, dailyCollectNitro, ctx.Member.Username);

                var dailyEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{ctx.Member.DisplayName} has collected their Daily 'Tax'!",
                    Description = $"You just received {dailyCollectNitro:###,###,###,###,###} Gold!",
                    Color = DiscordColor.Cyan,
                };

                dailyEmbed.AddField("You collected 2x Normal Tax", "Because you have the Double XP Role");

                await ctx.Channel.SendMessageAsync(embed: dailyEmbed);

                return;
            }

            else
            {
                await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);

                var rndDaily = new Random();
                var dailyCollect = rndDaily.Next(500, 5000);

                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, dailyCollect, ctx.Member.Username);

                var dailyEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{ctx.Member.DisplayName} has collected their Daily 'Tax'!",
                    Description = $"You just received {dailyCollect:###,###,###,###,###} Gold!",
                    Color = DiscordColor.Cyan,
                };

                await ctx.Channel.SendMessageAsync(embed: dailyEmbed);

                return;
            }

            
        }

        [Command("buildprofiletable")]
        [Description("Bring all users in the server to the Profile Table")]
        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
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
                DiscordUser member1 = await ctx.Client.GetUserAsync(first.DiscordId);
                DiscordUser member2 = await ctx.Client.GetUserAsync(second.DiscordId);
                DiscordUser member3 = await ctx.Client.GetUserAsync(third.DiscordId);
                DiscordUser member4 = await ctx.Client.GetUserAsync(fourth.DiscordId);
                DiscordUser member5 = await ctx.Client.GetUserAsync(fifth.DiscordId);
                DiscordUser member6 = await ctx.Client.GetUserAsync(sixth.DiscordId);
                DiscordUser member7 = await ctx.Client.GetUserAsync(seventh.DiscordId);
                DiscordUser member8 = await ctx.Client.GetUserAsync(eighth.DiscordId);
                DiscordUser member9 = await ctx.Client.GetUserAsync(ninth.DiscordId);
                DiscordUser member10 = await ctx.Client.GetUserAsync(tenth.DiscordId);

                var leaderboardEmbed = new DiscordEmbedBuilder
                {
                    Title = $"XP Leaderboard\n{member1.Username} is currently in 1st!",
                    Description = "Here are the top 10 XP gainers in the Discord!",
                    Color = DiscordColor.Gold,
                };

                leaderboardEmbed.WithFooter($"Leaderboard correct as of {DateTime.Now}");
                leaderboardEmbed.WithThumbnail(member1.AvatarUrl);
                leaderboardEmbed.AddField("1st", $"{member1.Username} - {first.XP:###,###,###,###,###} XP - Level {first.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("2nd", $"{member2.Username} - {second.XP:###,###,###,###,###} XP - Level {second.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("3rd", $"{member3.Username} - {third.XP:###,###,###,###,###} XP - Level {third.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("4th", $"{member4.Username} - {fourth.XP:###,###,###,###,###} XP - Level {fourth.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("5th", $"{member5.Username} - {fifth.XP:###,###,###,###,###} XP - Level {fifth.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("6th", $"{member6.Username} - {sixth.XP:###,###,###,###,###} XP - Level {sixth.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("7th", $"{member7.Username} - {seventh.XP:###,###,###,###,###} XP - Level {seventh.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("8th", $"{member8.Username} - {eighth.XP:###,###,###,###,###} XP - Level {eighth.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("9th", $"{member9.Username} - {ninth.XP:###,###,###,###,###} XP - Level {ninth.Level:###,###,###,###,###}");
                leaderboardEmbed.AddField("10th", $"{member10.Username} - {tenth.XP:###,###,###,###,###} XP - Level {tenth.Level:###,###,###,###,###}");

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
                DiscordUser member1 = await ctx.Client.GetUserAsync(first.DiscordId);
                DiscordUser member2 = await ctx.Client.GetUserAsync(second.DiscordId);
                DiscordUser member3 = await ctx.Client.GetUserAsync(third.DiscordId);
                DiscordUser member4 = await ctx.Client.GetUserAsync(fourth.DiscordId);
                DiscordUser member5 = await ctx.Client.GetUserAsync(fifth.DiscordId);
                DiscordUser member6 = await ctx.Client.GetUserAsync(sixth.DiscordId);
                DiscordUser member7 = await ctx.Client.GetUserAsync(seventh.DiscordId);
                DiscordUser member8 = await ctx.Client.GetUserAsync(eighth.DiscordId);
                DiscordUser member9 = await ctx.Client.GetUserAsync(ninth.DiscordId);
                DiscordUser member10 = await ctx.Client.GetUserAsync(tenth.DiscordId);

                var leaderboardEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Gold Leaderboard\n{member1.Username} is currently in 1st!",
                    Description = "Here are the top 10 Gold gainers in the Discord!",
                    Color = DiscordColor.Gold,
                };

                leaderboardEmbed.WithFooter($"Leaderboard correct as of {DateTime.Now}");
                leaderboardEmbed.WithThumbnail(member1.AvatarUrl);
                leaderboardEmbed.AddField("1st", $"{member1.Username} - {first.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("2nd", $"{member2.Username} - {second.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("3rd", $"{member3.Username} - {third.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("4th", $"{member4.Username} - {fourth.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("5th", $"{member5.Username} - {fifth.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("6th", $"{member6.Username} - {sixth.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("7th", $"{member7.Username} - {seventh.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("8th", $"{member8.Username} - {eighth.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("9th", $"{member9.Username} - {ninth.Gold:###,###,###,###,###} Gold");
                leaderboardEmbed.AddField("10th", $"{member10.Username} - {tenth.Gold:###,###,###,###,###} Gold");

                await ctx.Channel.SendMessageAsync(embed: leaderboardEmbed).ConfigureAwait(false);

                return;
            }
            await ctx.Channel.SendMessageAsync("You need to specify whether you want `!top10 Gold` or `!top10 XP` only!");

            return;
        }

    }
}
