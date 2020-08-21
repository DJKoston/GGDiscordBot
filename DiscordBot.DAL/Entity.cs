using System.ComponentModel.DataAnnotations;

namespace DiscordBot.DAL
{
    public abstract class Entity
    {
        [Key]
        public int Id { get; set; }
    }
}
