namespace DiscordBot.DAL.Models.CommunityStreamers
{
    public class CommunityStreamer : Entity
    {
        public ulong GuildId { get; set; }
        public string StreamerName { get; set; }
        public ulong RequestorId { get; set; }
        public string DealtWith { get; set; }
        public ulong RequestMessage { get; set; }
    }
}