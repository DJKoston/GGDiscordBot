namespace DiscordBot.DAL.Models.Configurations
{
    public class ButtonRoleConfig : Entity
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public string ButtonId { get; set; }
        public string GiveRemove { get; set; }
    }
}