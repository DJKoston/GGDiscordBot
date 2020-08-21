using DiscordBot.DAL;
using DiscordBot.DAL.Models.CustomCommands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.CustomCommands
{
    public interface ICustomCommandService
    {
        Task CreateNewCommandAsync(CustomCommand customCommand);
        Task<CustomCommand> GetCommandAsync(string command);
        Task DeleteCommandAsync(CustomCommand customCommand);
    }
    public class CustomCommandService : ICustomCommandService
    {
        private readonly DbContextOptions<RPGContext> _options;

        public CustomCommandService(DbContextOptions<RPGContext> options)
        {
            _options = options;
        }

        public async Task CreateNewCommandAsync(CustomCommand customCommand)
        {
            using var context = new RPGContext(_options);

            await context.AddAsync(customCommand).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteCommandAsync(CustomCommand customCommand)
        {
            using var context = new RPGContext(_options);

            context.Remove(customCommand);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<CustomCommand> GetCommandAsync(string command)
        {
            using var context = new RPGContext(_options);

            command = command.ToLower();

            return await context.CustomCommands.FirstOrDefaultAsync(x => x.Trigger.ToLower() == command).ConfigureAwait(false);
        }
    }
}
