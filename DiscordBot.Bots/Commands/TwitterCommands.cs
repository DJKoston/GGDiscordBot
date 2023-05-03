using DiscordBot.DAL.Models.Twitter;
using System.Threading;
using Tweetinvi;
using Tweetinvi.Parameters.V2;
using TwitchLib.Api.Helix;

namespace DiscordBot.Bots.Commands
{
    [Group("twitter")]
    [RequirePermissions(Permissions.Administrator)]
    public class TwitterCommands : BaseCommandModule
    {
        private readonly ITwitterService _twitterService;
        private readonly IConfiguration _configuration;

        public TwitterCommands(ITwitterService twitterService, IConfiguration configuration)
        {
            _twitterService = twitterService;
            _configuration = configuration;
        }

        [Command("add")]
        public async Task AddTwitter(CommandContext ctx, string twitterUserName, DiscordChannel channel)
        {
            var isInDatabase = _twitterService.GetGuildMonitors(ctx.Guild.Id).FirstOrDefault(x => x.TwitterUser == twitterUserName);

            if (isInDatabase != null) 
            {
                var failedMessageBuilder = new DiscordMessageBuilder
                {
                    Content = $"{twitterUserName.ToLower()} is already being announce in this server. Please use `!twitter remove *username*` to remove them before trying again.",
                };

                failedMessageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(failedMessageBuilder);

                return;
            }

            var twitterBearerToken = _configuration["twitter-bearer"];
            var twitterAPIKey = _configuration["twitter-apikey"];
            var twitterAPIToken = _configuration["twitter-apitoken"];

            var twitterClient = new TwitterClient(twitterAPIKey, twitterAPIToken, twitterBearerToken);

            var searchParams = new SearchTweetsV2Parameters($"(from:{twitterUserName.ToLower()}) -is:retweet -is:reply")
            {
                PageSize = 10
            };
            var tweetSearch = await twitterClient.SearchV2.SearchTweetsAsync(searchParams);

            var latestTweet = tweetSearch.Tweets.FirstOrDefault();

            if(latestTweet != null)
            {
                await _twitterService.AddNewMonitorAsync(ctx.Guild.Id, channel.Id, twitterUserName.ToLower(), latestTweet.Id, latestTweet.CreatedAt.ToString());
            }
            else
            {
                var fakeDateTimeInjection = DateTime.UtcNow.AddDays(-8);

                await _twitterService.AddNewMonitorAsync(ctx.Guild.Id, channel.Id, twitterUserName.ToLower(), "000000000000", fakeDateTimeInjection.ToString());
            }

            var messageBuilder = new DiscordMessageBuilder
            {
                Content = $"{twitterUserName.ToLower()} will now have their tweets announced in {channel.Mention}",
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
                Content = $"{twitterUserName} will no longer be announced in this server.",
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


                if (embed1.Fields.Count != 0)
                {
                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = embed1,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder);
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

                if (embed1.Fields.Count != 0)
                {
                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = embed1,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder);
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

                if (embed1.Fields.Count != 0)
                {
                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = embed1,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder);
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

                if (embed1.Fields.Count != 0)
                {
                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = embed1,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder);
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

                if (embed1.Fields.Count != 0)
                {
                    var messageBuilder = new DiscordMessageBuilder
                    {
                        Embed = embed1,
                    };

                    messageBuilder.WithReply(ctx.Message.Id, true);

                    await ctx.Channel.SendMessageAsync(messageBuilder);
                }
            }
        }
    }
}
