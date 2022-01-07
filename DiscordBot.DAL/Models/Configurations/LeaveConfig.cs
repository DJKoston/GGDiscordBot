using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DAL.Models.Configurations
{
    public class LeaveConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string LeaveMessage { get; set; }
        public string LeaveImage { get; set; }
    }
}
