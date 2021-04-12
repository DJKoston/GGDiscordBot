namespace DiscordBot.DAL.Models.Radios
{
    public class Radio : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string RadioURL { get; set; }
    }
}
