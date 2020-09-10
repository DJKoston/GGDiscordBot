namespace DiscordBot.DAL.Models.ReactionRoles
{
    public class ReactionRole : Entity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong RoleId { get; set; }
        public ulong EmoteId { get; set; }
        public string UnicodeEmote { get; set; }
    }
}
