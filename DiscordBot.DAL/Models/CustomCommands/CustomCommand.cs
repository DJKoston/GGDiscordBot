namespace DiscordBot.DAL.Models.CustomCommands
{
    public class CustomCommand : Entity
    {
        public string Trigger { get; set; }
        public string Action { get; set; }
    }
}
