namespace DiscordBot.DAL.Models.Configurations
{
    public class DoubleXPRoleConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}