using System.Runtime.CompilerServices;

namespace DiscordBot.DAL.Models.CommunityStreamers
{
    public class CommunityStreamer : Entity
    {
        public ulong GuildId { get; set; }
        public string streamerName { get; set; }
        public ulong requestorId { get; set; }
        public string DealtWith { get; set; }
        public ulong RequestMessage { get; set; }
    }
}
