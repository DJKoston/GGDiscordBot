namespace DiscordBot.DAL.Models.Games
{
    public class NumberGuess : Entity
    {
        public ulong GuildId { get; set; }
        public int Number { get; set; }
    }
}
