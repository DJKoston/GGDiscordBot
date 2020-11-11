using Newtonsoft.Json;

namespace DiscordBot.Bots.JsonConverts
{
    public struct TheSimpsons
    {
        [JsonProperty("quote")]
        public string Quote { get; private set; }
        [JsonProperty("character")]
        public string Character { get; private set; }
        [JsonProperty("image")]
        public string ImageURL { get; private set; }
        [JsonProperty("characterDirection")]
        public string CharaterDirection { get; private set; }
    }
}
