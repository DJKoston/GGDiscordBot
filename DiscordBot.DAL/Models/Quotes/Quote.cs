namespace DiscordBot.DAL.Models.Quotes
{
    public class Quote : Entity
    {
        public ulong GuildId { get; set; }
        public int QuoteId { get; set; }
        public ulong AddedById { get; set; }
        public ulong DiscordUserQuotedId { get; set; }
        public string QuoteContents { get; set; }
        public string DateAdded {get; set;}
        public string ChannelQuotedIn { get; set; }
    }
}
