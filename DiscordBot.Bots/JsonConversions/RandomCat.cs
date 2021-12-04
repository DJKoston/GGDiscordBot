using Newtonsoft.Json;

namespace DiscordBot.Bots.JsonConversions
{
    public struct RandomCat
    {
        [JsonProperty("url")]
        public string Url { get; private set; }
    }
}
