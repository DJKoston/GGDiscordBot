namespace DiscordBot.DAL.Models.YouTube
{
    public class YTVideos : Entity
    {
        public string UserName { get; set; }
        public string UserThumbnail { get; set; }
        public string VideoTitle { get; set; }
        public string VideoDescription { get; set; }
        public string VideoThumbnail { get; set; }
        public string VideoPublishedDate { get; set; }
        public string VideoURL { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string UserId { get; set; }
    }
}
