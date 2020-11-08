namespace DiscordBot.DAL.Models.Configs
{
    public class NowLiveRoleConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}
