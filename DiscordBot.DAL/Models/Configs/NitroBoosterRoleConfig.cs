namespace DiscordBot.DAL.Models.Configs
{
    public class NitroBoosterRoleConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}
