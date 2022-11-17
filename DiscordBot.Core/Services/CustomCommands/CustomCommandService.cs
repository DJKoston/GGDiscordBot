﻿using DiscordBot.DAL;
using DiscordBot.DAL.Models.CustomCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Core.Services.CustomCommands
{
    public interface ICustomCommandService
    {
        Task CreateNewCommandAsync(CustomCommand customCommand);
        Task<CustomCommand> GetCommandAsync(string command, ulong GuildId);
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

            await context.AddAsync(customCommand);

            await context.SaveChangesAsync();
        }

        public async Task DeleteCommandAsync(CustomCommand customCommand)
        {
            using var context = new RPGContext(_options);

            context.Remove(customCommand);

            await context.SaveChangesAsync();
        }

        public async Task<CustomCommand> GetCommandAsync(string command, ulong GuildId)
        {
            using var context = new RPGContext(_options);

            command = command.ToLower();

            var GuildCommands = context.CustomCommands.Where(x => x.GuildId == GuildId);

            return await GuildCommands.FirstOrDefaultAsync(x => x.Trigger.ToLower() == command);
        }
    }
}