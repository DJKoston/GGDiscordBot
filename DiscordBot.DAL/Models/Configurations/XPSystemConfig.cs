namespace DiscordBot.DAL.Models.Configurations
{
    public class XPSystemConfig : Entity
    {
        public ulong GuildId { get; set; }
        public string Status { get; set; }
    }
}