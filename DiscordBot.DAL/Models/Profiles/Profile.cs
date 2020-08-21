using DiscordBot.DAL.Models.Items;
using System;
using System.Collections.Generic;

namespace DiscordBot.DAL.Models.Profiles
{
    public class Profile : Entity
    {
        public ulong DiscordId { get; set; }
        public ulong GuildId { get; set; }
        public string DiscordName { get; set; }
        public int Gold { get; set; }
        public int XP { get; set; }
        public int Level
        {
            get
            {
                return (int)((1 + Math.Sqrt(1 + 8 * XP / 100)) / 2);
            }
        }
        public List<ProfileItem> Items { get; set; } = new List<ProfileItem>();
    }
}
