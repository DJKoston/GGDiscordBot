namespace DiscordBot.DAL.Models.Games
{
    public class MusicPlaylist : Entity
    {
        public ulong GuildId { get; set; }
        public string SongURI { get; set; }
        public string SongTitle { get; set; }
    }
}
