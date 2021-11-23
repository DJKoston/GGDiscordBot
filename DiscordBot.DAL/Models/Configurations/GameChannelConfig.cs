namespace DiscordBot.DAL.Models.Configurations
{
    public class GameChannelConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
