using DiscordBot.DAL.Models.CommunityStreamers;
using DiscordBot.DAL.Models.Configurations;
using DiscordBot.DAL.Models.Counters;
using DiscordBot.DAL.Models.CustomCommands;
using DiscordBot.DAL.Models.Egg;
using DiscordBot.DAL.Models.NowLive;
using DiscordBot.DAL.Models.Profiles;
using DiscordBot.DAL.Models.Quotes;
using DiscordBot.DAL.Models.ReactionRoles;
using DiscordBot.DAL.Models.Suggestions;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.DAL
{
    public class RPGContext : DbContext
    {
        public RPGContext(DbContextOptions<RPGContext> options) : base (options) { }

        //Community Streamers
        public DbSet<CommunityStreamer> CommunityStreamers { get; set; }

        //Configurations
        public DbSet<CurrencyNameConfig> CurrencyNameConfigs { get; set; }
        public DbSet<GameChannelConfig> GameChannelConfigs { get; set; }
        public DbSet<DoubleXPRoleConfig> DoubleXPRoleConfigs { get; set; }
        public DbSet<NowLiveRoleConfig> NowLiveRoleConfigs { get; set; }
        public DbSet<WelcomeMessageConfig> WelcomeMessageConfigs { get; set; }

        //Counters
        public DbSet<GoodBotBadBot> GoodBotBadBotCounters { get; set; }

        //Custom Commands
        public DbSet<CustomCommand> CustomCommands { get; set; }

        //Egg
        public DbSet<EggChannel> EggChannels { get; set; }
        public DbSet<EggNickname> EggNicknames { get; set; }
        public DbSet<EggRole> EggRoles { get; set; }

        //Now Live
        public DbSet<NowLiveMessage> NowLiveMessages { get; set; }
        public DbSet<NowLiveStreamer> NowLiveStreamers { get; set; }

        //Profiles
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ToNextXP> ToNextXPs { get; set; }

        //Quotes
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<SimpsonsQuote> SimpsonsQuotes { get; set;}

        //Reaction Roles
        public DbSet<ReactionRole> ReactionRoles { get; set; }

        //Suggestions
        public DbSet<Suggestion> Suggestions { get; set; }
    }
}
