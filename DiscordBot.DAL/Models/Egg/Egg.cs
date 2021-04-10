namespace DiscordBot.DAL.Models.Egg
{
    public class EggChannel : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string PreviousName { get; set; }
    }

    public class EggRole : Entity
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public string PreviousName { get; set; }
    }

    public class EggNickname : Entity
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string PreviousName { get; set; }
    }
}
