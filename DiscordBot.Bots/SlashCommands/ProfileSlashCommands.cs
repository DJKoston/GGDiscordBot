using DiscordBot.Core.Services.Configurations;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Configurations;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Bots.SlashCommands
{
    public class ProfileSlashCommands : ApplicationCommandModule
    {
        private readonly RPGContext _context;
        private readonly IProfileService _profileService;
        private readonly IGoldService _goldService;
        private readonly IDoubleXPRoleConfigService _doubleXPRoleConfigService;
        private readonly ICurrencyNameConfigService _currencyNameConfig;
        public string currencyName;

        public ProfileSlashCommands(RPGContext context, IProfileService profileService, IGoldService goldService, IDoubleXPRoleConfigService doubleXPRoleService, ICurrencyNameConfigService currencyNameConfig)
        {
            _context = context;
            _profileService = profileService;
            _goldService = goldService;
            _doubleXPRoleConfigService = doubleXPRoleService;
            _currencyNameConfig = currencyNameConfig;
        }

        [SlashCommand("myinfo", "Display's your Server Profile")]
        public async Task MyInfo(InteractionContext ctx)
        {
            var memberId = ctx.Member.Id;

            var dxpConfig = _doubleXPRoleConfigService.GetDoubleXPRole(ctx.Guild.Id).Result;

            var cnConfig = await _currencyNameConfig.GetCurrencyNameConfig(ctx.Guild.Id);

            if (cnConfig == null) { currencyName = "Gold"; }
            else { currencyName = cnConfig.CurrencyName; }

            if (dxpConfig == null)
            {
                var memberUsername = ctx.Guild.Members[memberId].Username;

                var quotescount = _context.Quotes.Where(x => x.GuildId == ctx.Guild.Id);
                var quotes = quotescount.Count(x => x.DiscordUserQuotedId == memberId);

                var profile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

                var member = ctx.Guild.Members[profile.DiscordId];

                var profileEmbed = new DiscordEmbedBuilder()
                {
                    Title = $"{member.DisplayName}'s Profile",
                    Color = member.Color
                };

                profileEmbed.WithThumbnail(member.AvatarUrl);

                var nextLevel = _context.ToNextXPs.FirstOrDefault(x => x.Level == profile.Level + 1).XPAmount;

                profileEmbed.AddField("XP", $"{profile.XP:###,###,###,###,###} / {nextLevel:###,###,###,###,###}");
                profileEmbed.AddField("Level", profile.Level.ToString("###,###,###,###,###"));

                if (profile.Gold == 0) { profileEmbed.AddField(currencyName, profile.Gold.ToString()); }
                if (profile.Gold >= 1) { profileEmbed.AddField(currencyName, profile.Gold.ToString("###,###,###,###,###")); };

                if (quotes == 0) { profileEmbed.AddField("You have been Quoted:", $"{quotes} Times"); }
                if (quotes == 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Time"); }
                if (quotes > 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Times"); }

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(profileEmbed);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder);
            }

            else
            {
                var memberUsername = ctx.Guild.Members[memberId].Username;

                var doubleXPRole = ctx.Guild.GetRole(dxpConfig.RoleId);

                var quotescount = _context.Quotes.Where(x => x.GuildId == ctx.Guild.Id);
                var quotes = quotescount.Count(x => x.DiscordUserQuotedId == memberId);

                var profile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id, memberUsername);

                var member = ctx.Guild.Members[profile.DiscordId];

                var profileEmbed = new DiscordEmbedBuilder()
                {
                    Title = $"{member.DisplayName}'s Profile",
                    Color = member.Color
                };

                profileEmbed.WithThumbnail(member.AvatarUrl);

                if (profile.Gold == 0) { profileEmbed.AddField(currencyName, profile.Gold.ToString()); }
                if (profile.Gold >= 1) { profileEmbed.AddField(currencyName, profile.Gold.ToString("###,###,###,###,###")); };

                var nextLevel = _context.ToNextXPs.FirstOrDefault(x => x.Level == profile.Level + 1).XPAmount;

                profileEmbed.AddField("XP", $"{profile.XP:###,###,###,###,###} / {nextLevel:###,###,###,###,###}");
                profileEmbed.AddField("Level", profile.Level.ToString("###,###,###,###,###"));

                if (quotes == 0) { profileEmbed.AddField("You have been Quoted:", $"{quotes} Times"); }
                if (quotes == 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Time"); }
                if (quotes > 1) { profileEmbed.AddField("You have been Quoted:", $"{quotes:###,###,###,###,###} Times"); }

                if (member.Roles.Contains(doubleXPRole)) { profileEmbed.AddField("You have the Double XP Role!", "You currently get 2x XP and 2x Tax!"); }

                var messageBuilder = new DiscordInteractionResponseBuilder();

                messageBuilder.AddEmbed(profileEmbed);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder); ;
            }
        }

        [SlashCommand("pay", "Pay a user some virtual money")]
        public async Task Pay(InteractionContext ctx,
            [Option("user", "User to pay")] DiscordUser user,
            [Option("amount", "Amount to pay user. Must be a number.")] double amount)
        {
            var member = await ctx.Guild.GetMemberAsync(user.Id);

            var payAmount = Convert.ToInt32(amount);

            var cnConfig = await _currencyNameConfig.GetCurrencyNameConfig(ctx.Guild.Id);

            if (cnConfig == null) { currencyName = "Gold"; }
            else { currencyName = cnConfig.CurrencyName; }

            var userProfile = await _profileService.GetOrCreateProfileAsync(ctx.Member.Id, ctx.Guild.Id, ctx.Member.Username);
            await _profileService.GetOrCreateProfileAsync(member.Id, ctx.Guild.Id, member.Username);

            if (userProfile.Gold < payAmount)
            {
                var userProfileCheckFailEmbed = new DiscordEmbedBuilder()
                {
                    Title = $"You can't pay that much {ctx.Member.DisplayName}!",
                    Description = $"Seems like you're too poor to afford to pay {member.DisplayName} {payAmount:###,###,###,###,###} {currencyName}! Try paying them a smaller amount... We have shown you how much {currencyName} you have below!",
                    Color = DiscordColor.IndianRed,
                };

                if (userProfile.Gold == 0) { userProfileCheckFailEmbed.AddField(currencyName, userProfile.Gold.ToString()); }
                if (userProfile.Gold >= 1) { userProfileCheckFailEmbed.AddField(currencyName, userProfile.Gold.ToString("###,###,###,###,###")); }

                var messageBuilder1 = new DiscordInteractionResponseBuilder();

                messageBuilder1.AddEmbed(userProfileCheckFailEmbed);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder1); ;

                return;
            }

            if (payAmount < 0)
            {

                var lessThanCheck = new DiscordEmbedBuilder()
                {
                    Title = $"You cannot pay less than 0 {currencyName} {ctx.Member.DisplayName}!",
                    Description = "Nice try suckka!",
                    Color = DiscordColor.IndianRed,
                };

                var messageBuilder2 = new DiscordInteractionResponseBuilder();

                messageBuilder2.AddEmbed(lessThanCheck);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder2);

                return;
            }

            await _goldService.GrantGoldAsync(ctx.Member.Id, ctx.Guild.Id, -payAmount, ctx.Member.Username);
            await _goldService.GrantGoldAsync(member.Id, ctx.Guild.Id, payAmount, ctx.Member.Username);

            var paidEmbed = new DiscordEmbedBuilder()
            {
                Title = $"You have paid {member.DisplayName} {payAmount:###,###,###,###,###} {currencyName}!",
                Description = $"Thank you for using the GG Bot Payment Network {ctx.Member.DisplayName}!",
                Color = DiscordColor.SpringGreen,
            };

            var messageBuilder3 = new DiscordInteractionResponseBuilder();

            messageBuilder3.AddEmbed(paidEmbed);

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, messageBuilder3);
        }
    }
}
