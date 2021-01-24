using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.DAL.Models.Configs
{
    public class CurrencyNameConfig : Entity
    {
        public ulong GuildId { get; set; }
        public string CurrencyName { get; set; }
    }
}
