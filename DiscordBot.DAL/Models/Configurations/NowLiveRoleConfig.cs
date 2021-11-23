namespace DiscordBot.DAL.Models.Configurations
{
    public class NowLiveRoleConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}