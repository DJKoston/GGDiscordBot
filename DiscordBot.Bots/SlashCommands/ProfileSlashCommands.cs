namespace DiscordBot.Bots.SlashCommands
{
    public class ProfileSlashCommands : ApplicationCommandModule
    {
        private readonly RPGContext _context;
        private readonly IProfileService _profileService;
        private readonly IGoldService _goldService;
        private readonly IDoubleXPRoleConfigService _doubleXPRoleConfig;
        private readonly ICurrencyNameConfigService _currencyNameConfig;
        public string currencyName;

        public ProfileSlashCommands(RPGContext context, IProfileService profileService, IGoldService goldService, IDoubleXPRoleConfigService doubleXPRoleConfig, ICurrencyNameConfigService currencyNameConfig)
        {
            _context = context;
            _profileService = profileService;
            _goldService = goldService;
            _doubleXPRoleConfig = doubleXPRoleConfig;
            _currencyNameConfig = currencyNameConfig;
        }

        [SlashCommand("myinfo", "Get your server information!")]
        public async Task SlashProfile(InteractionContext ctx)
        {
            var NBConfig = await _doubleXPRoleConfig.GetDoubleXPRole(ctx.Guild.Id);
            DiscordRole doubleXPRole = null;
            if (NBConfig != null) { doubleXPRole = ctx.Guild.GetRole(NBConfig.RoleId); }
            var CNConfig = await _currencyNameConfig.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            var memberId = ctx.Member.Id;

            var memberUsername = ctx.Guild.Members[memberId].Username;

            Profile profile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

            var member = ctx.Guild.Members[profile.DiscordId];

            var profileEmbed = new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName}'s Profile",
                Color = member.Color
            };

            profileEmbed.WithThumbnail(member.AvatarUrl);

            var nextLevel = _context.ToNextXPs.FirstOrDefault(x => x.Level == profile.Level + 1).XPAmount;

            profileEmbed.AddField("XP", $"{profile.XP:###,###,###,###,###} / {nextLevel:###,###,###,###,###}");
            profileEmbed.AddField("Level", profile.Level.ToString("###,###,###,###,###"));

            if (profile.Gold == 0) { profileEmbed.AddField(currencyName, profile.Gold.ToString()); }
            if (profile.Gold >= 1) { profileEmbed.AddField(currencyName, profile.Gold.ToString("###,###,###,###,###")); }

            if (member.Roles.Contains(doubleXPRole)) { profileEmbed.AddField("You have the Double XP Role!", "You currently get 2x XP and 2x Tax!"); }

            DiscordInteractionResponseBuilder responseBuilder = new();
            responseBuilder.AddEmbed(profileEmbed);
            await ctx.CreateResponseAsync(responseBuilder);
        }

        [SlashCommand("pay", "Pay someone!")]
        public async Task SlashPay(InteractionContext ctx,
            [Option("user", "User to pay")] DiscordUser user,
            [Option("amount", "Amount to pay")] double pay)
        {
            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var payAmount = Convert.ToInt32(pay);

            var CNConfig = await _currencyNameConfig.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            Profile userProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
            await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

            DiscordEmbedBuilder embed = new();

            if (userProfile.Gold < payAmount)
            {
                embed.Title = $"You can't pay that much {ctx.Member.DisplayName}!";
                embed.Description = $"Seems like you're too poor to afford to pay {member.DisplayName} {payAmount:###,###,###,###,###} {currencyName}! Try paying them a smaller amount... We have shown you how much {currencyName} you have below!";
                embed.Color = DiscordColor.IndianRed;

                if (userProfile.Gold == 0) { embed.AddField(currencyName, userProfile.Gold.ToString()); }
                if (userProfile.Gold >= 1) { embed.AddField(currencyName, userProfile.Gold.ToString("###,###,###,###,###")); }
            }

            if (payAmount < 0)
            {
                embed.Title = $"You cannot pay less than 0 {currencyName} {ctx.Member.DisplayName}!";
                embed.Description = $"You cannot pay less than 0 {currencyName} {ctx.Member.DisplayName}!";
                embed.Color = DiscordColor.IndianRed;
            }

            else
            {
                await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -payAmount, ctx.Member.Username);
                await _goldService.GrantGoldAsync(member.Id, ctx.Guild.Id, payAmount, ctx.Member.Username);

                embed.Title = $"You have paid {member.DisplayName} {payAmount:###,###,###,###,###} {currencyName}!";
                embed.Description = $"Thank you for using the GG Bot Payment Network {ctx.Member.DisplayName}!";
                embed.Color = DiscordColor.SpringGreen;
            }

            DiscordInteractionResponseBuilder responseBuilder = new();
            responseBuilder.AddEmbed(embed);
            await ctx.CreateResponseAsync(responseBuilder);
        }

        [SlashCommand("top10", "Get the Top 10!")]
        public async Task SlashTop10(InteractionContext ctx,
            [Choice("xp", 1)]
            [Choice("currency", 2)]
            [Option("item", "Item to get Top 10 for!")] double choice)
        {
            var CNConfig = await _currencyNameConfig.GetCurrencyNameConfig(ctx.Guild.Id);

            if (CNConfig == null) { currencyName = "Gold"; }
            else { currencyName = CNConfig.CurrencyName; }

            if (choice == 1)
            {
                var xplist = _context.Profiles.Where(x => x.XP >= 0 && x.GuildId == ctx.Guild.Id).OrderByDescending(x => x.XP).Take(10).AsNoTracking().ToList();

                if(xplist.Count < 10) { await ctx.CreateResponseAsync("There are not 10 users in the leaderboard so we can't show you the leaderboard.", true); return; }

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

                DiscordInteractionResponseBuilder messageBuilder = new();

                messageBuilder.AddEmbed(leaderboardEmbed);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }

            if (choice == 2)
            {
                var xplist = _context.Profiles.Where(x => x.XP >= 1 && x.GuildId == ctx.Guild.Id).OrderByDescending(x => x.Gold).Take(10).AsNoTracking().ToList();

                if (xplist.Count < 10) { await ctx.CreateResponseAsync("There are not 10 users in the leaderboard so we can't show you the leaderboard.", true); return; }

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
                    Description = $"Here are the top 10 {currencyName} gainers in the Discord!",
                    Color = DiscordColor.Gold,
                };

                leaderboardEmbed.WithFooter($"Leaderboard correct as of {DateTime.Now}");
                leaderboardEmbed.WithThumbnail(member1.AvatarUrl);
                leaderboardEmbed.AddField("1st", $"{member1.Username} - {first.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("2nd", $"{member2.Username} - {second.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("3rd", $"{member3.Username} - {third.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("4th", $"{member4.Username} - {fourth.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("5th", $"{member5.Username} - {fifth.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("6th", $"{member6.Username} - {sixth.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("7th", $"{member7.Username} - {seventh.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("8th", $"{member8.Username} - {eighth.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("9th", $"{member9.Username} - {ninth.Gold:###,###,###,###,###} {currencyName}");
                leaderboardEmbed.AddField("10th", $"{member10.Username} - {tenth.Gold:###,###,###,###,###} {currencyName}");

                DiscordInteractionResponseBuilder messageBuilder = new();

                messageBuilder.AddEmbed(leaderboardEmbed);
                await ctx.CreateResponseAsync(messageBuilder);

                return;
            }
        }
    }
}
