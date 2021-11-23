namespace DiscordBot.DAL.Models.Configurations
{
    public class NitroBoosterRoleConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}