using Newtonsoft.Json;

namespace DiscordBot.Bots.JsonConverts
{
    public struct RandomCat
    {
        [JsonProperty("url")]
        public string Url { get; private set; }
    }
}
