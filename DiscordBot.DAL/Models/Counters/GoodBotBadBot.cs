namespace DiscordBot.DAL.Models.Counters
{
    public class GoodBotBadBot : Entity
    {
        public string BotName { get; set; }
        public int GoodBot { get; set; }
        public int BadBot { get; set; }
    }
}