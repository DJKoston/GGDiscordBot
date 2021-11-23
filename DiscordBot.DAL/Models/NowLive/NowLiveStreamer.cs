namespace DiscordBot.DAL.Models.NowLive
{
    public class NowLiveStreamer : Entity
    {
        public ulong GuildId { get; set; }
        public string StreamerId { get; set; }
        public ulong AnnounceChannelId { get; set; }
        public string AnnouncementMessage { get; set; }
        public string StreamerName { get; set; }
    }
}