using Microsoft.EntityFrameworkCore;

namespace DiscordBot.DAL
{
    public class RPGContext : DbContext
    {
        public RPGContext(DbContextOptions<RPGContext> options) : base (options) { }
    }
}
