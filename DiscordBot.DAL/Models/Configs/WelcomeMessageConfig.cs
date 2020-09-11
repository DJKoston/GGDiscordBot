namespace DiscordBot.DAL.Models.Configs
{
    public class WelcomeMessageConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string WelcomeMessage { get; set; }
        public string WelcomeImage { get; set; }
        public string LeaveMessage { get; set; }
        public string LeaveImage { get; set; }
    }
}
