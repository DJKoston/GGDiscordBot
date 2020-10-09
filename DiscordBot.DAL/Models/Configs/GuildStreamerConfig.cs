namespace DiscordBot.DAL.Models.Configs
{
    public class GuildStreamerConfig : Entity
    {
        public ulong GuildId { get; set; }
        public string StreamerId { get; set; }
        public ulong AnnounceChannelId { get; set; }
        public string AnnouncementMessage { get; set; }
        public string StreamerName { get; set; }
    }
}
