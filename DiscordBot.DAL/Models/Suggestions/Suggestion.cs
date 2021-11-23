namespace DiscordBot.DAL.Models.Suggestions
{
    public class Suggestion : Entity
    {
        public ulong GuildId { get; set; }
        public ulong SuggestorId { get; set; }
        public string SuggestionText { get; set; }
        public string RespondedTo { get; set; }
        public ulong SuggestionEmbedMessage { get; set; }
    }
}
