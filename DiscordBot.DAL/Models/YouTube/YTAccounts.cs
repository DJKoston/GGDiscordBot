namespace DiscordBot.DAL.Models.YouTube
{
    public class YTAccounts : Entity
    {
        public ulong GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public string YouTubeUserId { get; set; }
        public string LastVideoDateTime { get; set; }
        public string LastVideoURL { get; set; }
    }
}
