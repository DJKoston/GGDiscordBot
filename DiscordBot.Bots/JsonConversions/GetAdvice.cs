namespace DiscordBot.Bots.JsonConversions
{
    public struct Advice
    {
        [JsonProperty("id")]
        public string Id { get; private set; }
        [JsonProperty("advice")]
        public string AdviceOutput { get; private set; }
    }
}