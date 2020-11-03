using DiscordBot.DAL.Models.Items;
using System;
using System.Collections.Generic;

namespace DiscordBot.DAL.Models.Profiles
{
    public class ToNextXP : Entity
    {
        public int Level { get; set; }
        public int XPAmount { get; set; }
    }
}
