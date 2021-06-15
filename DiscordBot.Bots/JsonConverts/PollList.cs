using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.JsonConverts
{
    public struct PollList
    {
        public string Option { get; set; }
        public int Votes { get; set; }
    }
}
