namespace DiscordBot.DAL.Models.MessageStores
{
    public class NowLiveMessages : Entity
    {
        public ulong GuildId { get; set; }
        public string StreamerId { get; set; }
        public ulong AnnouncementChannelId { get; set; }
        public ulong AnnouncementMessageId { get; set; }
    }
}
