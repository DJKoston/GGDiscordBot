using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    [Group("Admin")]
    [RequireRoles(RoleCheckMode.Any, "Admin")]
    
    public class AdminCommands : BaseCommandModule
    {
        [Command("ping")]
        public async Task PingTime(CommandContext ctx)
        {
            var pingtime = ctx.Client.Ping.ToString();

            await ctx.Channel.SendMessageAsync(pingtime).ConfigureAwait(false);
        }

        [Command("rolereacts")]
        public async Task RoleReacts(CommandContext ctx)
        {
            var roleChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "role-management");

            DiscordMessage streamerRoleMessage = await roleChannel.GetMessageAsync(538530070441099278);
            DiscordMessage gamePlatformMessage = await roleChannel.GetMessageAsync(538530958488371204);
            DiscordMessage announcementPingMessage = await roleChannel.GetMessageAsync(612698615894245378);

            DiscordEmoji twitchEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 725823600346529795);
            DiscordEmoji playstationEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 538531970343501824);
            DiscordEmoji xboxEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 538531692378324997);
            DiscordEmoji pcEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 538531394251653121);
            DiscordEmoji announcementEmoji = DiscordEmoji.FromName(ctx.Client, ":one:");

            await streamerRoleMessage.CreateReactionAsync(twitchEmoji);
            await gamePlatformMessage.CreateReactionAsync(playstationEmoji);
            await gamePlatformMessage.CreateReactionAsync(xboxEmoji);
            await gamePlatformMessage.CreateReactionAsync(pcEmoji);
            await announcementPingMessage.CreateReactionAsync(announcementEmoji);
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

            DiscordChannel announcementChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "announcements");
            DiscordChannel gamesChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "discord-games");

            await announcementChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            await gamesChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
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
