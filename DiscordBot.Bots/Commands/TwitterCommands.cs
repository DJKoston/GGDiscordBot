using DiscordBot.DAL.Models.Twitter;
using TwitchLib.Api.Helix;

namespace DiscordBot.Bots.Commands
{
    [Group("twitter")]
    [RequirePermissions(Permissions.Administrator)]
    public class TwitterCommands : BaseCommandModule
    {
        private readonly ITwitterService _twitterService;

        public TwitterCommands(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        [Command("add")]
        public async Task AddTwitter(CommandContext ctx, string twitterUserName, DiscordChannel channel)
        {
            await _twitterService.AddNewMonitorAsync(ctx.Guild.Id, channel.Id, twitterUserName.ToLower());

            var messageBuilder = new DiscordMessageBuilder
            {
                Content = $"{twitterUserName.ToLower()} will now be have their tweets announced in {channel.Mention}",
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder);
        }

        [Command("remove")]
        public async Task RemoveTwitter(CommandContext ctx, string twitterUserName)
        {
            await _twitterService.RemoveMonitorAsync(ctx.Guild.Id, twitterUserName.ToLower());

            var messageBuilder = new DiscordMessageBuilder
            {
                Content = $"{twitterUserName} will now no longer be announced in this server.",
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder);
        }

        [Command("list")]
        public async Task ListTwitter(CommandContext ctx)
        {
            var guildMonitors = _twitterService.GetGuildMonitors(ctx.Guild.Id);

            var page1 = guildMonitors.Take(24);
            var page2 = guildMonitors.Skip(24).Take(24);
            var page3 = guildMonitors.Skip(48).Take(24);
            var page4 = guildMonitors.Skip(72).Take(24);
            var page5 = guildMonitors.Skip(96).Take(24);

            if (page1 != null)
            {
                var embed1 = new DiscordEmbedBuilder
                {
                    Title = $"{guildMonitors.Count} Twitter Users being announced in {ctx.Guild.Name}",
                    Color = DiscordColor.CornflowerBlue
                };

                foreach (Tweet monitor in page1)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(monitor.ChannelID);

                    embed1.AddField($"{monitor.TwitterUser}", $"{channel.Mention}", true);
                }
            }

            if (page2 != null)
            {
                var embed1 = new DiscordEmbedBuilder
                {
                    Title = $"{guildMonitors.Count} Twitter Users being announced in {ctx.Guild.Name}",
                    Color = DiscordColor.CornflowerBlue
                };

                foreach (Tweet monitor in page2)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(monitor.ChannelID);

                    embed1.AddField($"{monitor.TwitterUser}", $"{channel.Mention}", true);
                }
            }

            if (page3 != null)
            {
                var embed1 = new DiscordEmbedBuilder
                {
                    Title = $"{guildMonitors.Count} Twitter Users being announced in {ctx.Guild.Name}",
                    Color = DiscordColor.CornflowerBlue
                };

                foreach (Tweet monitor in page3)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(monitor.ChannelID);

                    embed1.AddField($"{monitor.TwitterUser}", $"{channel.Mention}", true);
                }
            }

            if (page4 != null)
            {
                var embed1 = new DiscordEmbedBuilder
                {
                    Title = $"{guildMonitors.Count} Twitter Users being announced in {ctx.Guild.Name}",
                    Color = DiscordColor.CornflowerBlue
                };

                foreach (Tweet monitor in page4)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(monitor.ChannelID);

                    embed1.AddField($"{monitor.TwitterUser}", $"{channel.Mention}", true);
                }
            }

            if (page5 != null)
            {
                var embed1 = new DiscordEmbedBuilder
                {
                    Title = $"{guildMonitors.Count} Twitter Users being announced in {ctx.Guild.Name}",
                    Color = DiscordColor.CornflowerBlue
                };

                foreach (Tweet monitor in page5)
                {
                    DiscordChannel channel = ctx.Guild.GetChannel(monitor.ChannelID);

                    embed1.AddField($"{monitor.TwitterUser}", $"{channel.Mention}", true);
                }
            }
        }
    }
}
