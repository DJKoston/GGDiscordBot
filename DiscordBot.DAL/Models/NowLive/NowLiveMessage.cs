namespace DiscordBot.DAL.Models.NowLive
{
    public class NowLiveMessage : Entity
    {
        public ulong GuildId { get; set; }
        public string StreamerId { get; set; }
        public ulong AnnouncementChannelId { get; set; }
        public ulong AnnouncementMessageId { get; set; }
        public string StreamTitle { get; set; }
        public string StreamGame { get; set; }
    }
}
