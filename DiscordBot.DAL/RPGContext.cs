﻿using DiscordBot.DAL.Models.CustomCommands;
using DiscordBot.DAL.Models.Items;
using DiscordBot.DAL.Models.Profiles;
using DiscordBot.DAL.Models.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.DAL
{
    public class RPGContext : DbContext
    {
        public RPGContext(DbContextOptions<RPGContext> options) : base(options) { }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<CustomCommand> CustomCommands { get; set; }
        public DbSet<ProfileItem> ProfileItems { get; set; }
        public DbSet<Quote> Quotes { get; set; }
    }
}
