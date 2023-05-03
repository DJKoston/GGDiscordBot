namespace DiscordBot.DAL.Models.Twitter
{
    public class Tweet : Entity
    {
        public ulong GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public string TwitterUser { get; set; }
        public string LastTweetLink { get; set; }
        public string LastTweetDateTime { get; set; }
    }
}
