﻿// <auto-generated />
using DiscordBot.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DiscordBot.DAL.Migrations.Migrations
{
    [DbContext(typeof(RPGContext))]
    partial class RPGContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("DiscordBot.DAL.Models.CommunityStreamers.CommunityStreamer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("DealtWith")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("RequestMessage")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("RequestorId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("StreamerName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CommunityStreamers");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Configurations.ButtonRoleConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ButtonId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GiveRemove")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("ButtonRoleConfigs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Configurations.CurrencyNameConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("CurrencyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("CurrencyNameConfigs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Configurations.DoubleXPRoleConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("DoubleXPRoleConfigs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Configurations.LeaveConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("LeaveImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LeaveMessage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("LeaveConfigs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Configurations.NowLiveRoleConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("NowLiveRoleConfigs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Configurations.WelcomeConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("WelcomeImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WelcomeMessage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("WelcomeConfigs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Configurations.XPSystemConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("XPSystemConfigs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Counters.GoodBotBadBot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("BadBot")
                        .HasColumnType("int");

                    b.Property<string>("BotName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GoodBot")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("GoodBotBadBotCounters");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.CustomCommands.CustomCommand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Action")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Trigger")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CustomCommands");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Egg.EggChannel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("PreviousName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EggChannels");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Egg.EggNickname", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("PreviousName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("UserId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("EggNicknames");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Egg.EggRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("PreviousName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("EggRoles");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Games.MusicPlaylist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("SongTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SongURI")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MusicPlaylists");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Games.NumberGuess", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int>("Number")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("NumberGuesses");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.NowLive.NowLiveMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("AnnouncementChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("AnnouncementMessageId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("StreamGame")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreamTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreamerId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("NowLiveMessages");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.NowLive.NowLiveStreamer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("AnnounceChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("AnnouncementMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("StreamerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreamerName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("NowLiveStreamers");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Profiles.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("DiscordName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Gold")
                        .HasColumnType("int");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int>("XP")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Profiles.ToNextXP", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<int>("XPAmount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ToNextXPs");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Quotes.Quote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("AddedById")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("ChannelQuotedIn")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DateAdded")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("DiscordUserQuotedId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("QuoteContents")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("QuoteId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Quotes.SimpsonsQuote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Character")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Quote")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SimpsonsQuotes");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.ReactionRoles.ReactionRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("EmoteId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("RemoveAddRole")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("UnicodeEmote")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ReactionRoles");
                });

            modelBuilder.Entity("DiscordBot.DAL.Models.Suggestions.Suggestion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("RespondedTo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("SuggestionEmbedMessage")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("SuggestionText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("SuggestorId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Suggestions");
                });
#pragma warning restore 612, 618
        }
    }
}
