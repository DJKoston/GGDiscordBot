namespace DiscordBot.DAL.Models.Configurations
{
    public class WelcomeConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string WelcomeMessage { get; set; }
        public string WelcomeImage { get; set; }
    }
}