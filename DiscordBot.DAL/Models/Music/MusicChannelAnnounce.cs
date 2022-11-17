namespace DiscordBot.DAL.Models.Games
{
    public class MusicChannel : Entity
    {
        public ulong GuildId { get; set; }
        public string ChannelId { get; set; }
    }
}
