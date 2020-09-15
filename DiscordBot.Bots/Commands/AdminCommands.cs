using DiscordBot.Core.Services.ReactionRoles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.ReactionRoles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{


    [Group("Admin")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    
    public class AdminCommands : BaseCommandModule
    {
        [Command("ping")]
        public async Task PingTime(CommandContext ctx)
        {
            var pingtime = ctx.Client.Ping.ToString();

            await ctx.Channel.SendMessageAsync($"Phew! I made it over the airwaves! This round trip took {pingtime}ms!").ConfigureAwait(false);
        }


        [Command("dtstart")]
        public async Task DowntimeStart(CommandContext ctx, [RemainingText] string reason)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Discord Bot Downtime",
                Color = DiscordColor.Blue,
                Description = $"The Discord Bot is going down for some maintainence or updates."
            };

            embed.AddField("Reason", reason);

            DiscordGuild guild = ctx.Client.Guilds.Values.FirstOrDefault(x => x.Name == "Generation Gamers");
            DiscordChannel announcementChannel = guild.Channels.Values.FirstOrDefault(x => x.Name == "announcements");
            DiscordChannel gamesChannel = guild.Channels.Values.FirstOrDefault(x => x.Name == "discord-games");

            await announcementChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            await gamesChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            await ctx.Client.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = ActivityType.Watching,
                Name = "For my new update!",
            }, UserStatus.DoNotDisturb);
        }

        [Command("dtcomplete")]
        public async Task DowntimeComplete(CommandContext ctx, [RemainingText] string update)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Discord Bot Downtime",
                Color = DiscordColor.Blue,
                Description = $"The Discord Bot is back up after some maintainence or updates."
            };

            embed.AddField("Updates", update);

            DiscordChannel announcementChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "announcements");
            DiscordChannel gamesChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "discord-games");


            await announcementChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            await gamesChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
    }
}
