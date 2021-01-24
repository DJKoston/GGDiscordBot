using Newtonsoft.Json;

namespace DiscordBot.Bots.JsonConverts
{
    public struct StarWarsQuotes
    {
        [JsonProperty("id")]
        public int Id { get; private set; }
        [JsonProperty("content")]
        public string Content { get; private set; }
        [JsonProperty("faction")]
        public int Faction { get; private set; }
    }
}
