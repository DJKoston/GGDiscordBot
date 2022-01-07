namespace DiscordBot.DAL.Models.Configurations
{
    public class CurrencyNameConfig : Entity
    {
        public ulong GuildId { get; set; }
        public string CurrencyName { get; set; }
    }
}