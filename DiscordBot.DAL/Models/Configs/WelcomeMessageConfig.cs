namespace DiscordBot.DAL.Models.Configs
{
    public class WelcomeMessageConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
