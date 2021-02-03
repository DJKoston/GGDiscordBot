namespace DiscordBot.DAL.Models.Quotes
{
    public class SimpsonsQuotes : Entity
    {
        public string Quote { get; set; }
        public string Character { get; set; }
        public string ImageURL { get; set; }
    }
}
