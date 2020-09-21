namespace DiscordBot.DAL.Models.Configs
{
    public class GameChannelConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
