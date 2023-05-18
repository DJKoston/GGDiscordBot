namespace DiscordBot.Bots.Commands
{
    public class MiscCommands : BaseCommandModule
    {
        private readonly RPGContext _context;
        private readonly ISuggestionService _suggestionService;
        private readonly ICommunityStreamerService _communityStreamerService;
        private readonly INowLiveStreamerService _nowLiveStreamerService;
        private readonly IConfiguration _configuration;
        private readonly ISimpsonsQuoteService _simpsonsQuoteService;
        private readonly ITwitterService _twitterService;

        public MiscCommands(RPGContext context, ISuggestionService suggestionService, ICommunityStreamerService communityStreamerService, INowLiveStreamerService nowLiveStreamerService, IConfiguration configuration, ISimpsonsQuoteService simpsonsQuoteService, ITwitterService twitterService)
        {
            _context = context;
            _suggestionService = suggestionService;
            _communityStreamerService = communityStreamerService;
            _nowLiveStreamerService = nowLiveStreamerService;
            _configuration = configuration;
            _simpsonsQuoteService = simpsonsQuoteService;
            _twitterService = twitterService;
        }

        [Command("d12")]
        [Description("Rolls a 12 sided Dice")]
        public async Task RollTwelveDie(CommandContext ctx)
        {
            var rnd = new Random();

            await ctx.RespondAsync($"🎲 The D12 has been rolled and the result is: {rnd.Next(1, 13)}");
        }

        [Command("dadjoke")]
        [Description("Get a Dad Joke!")]
        public async Task DadJoke(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            HttpClient client = new();

            using (Stream dataStream = await client.GetStreamAsync("https://api.scorpstuff.com/dadjokes.php"))
            {
                StreamReader reader = new(dataStream);

                string responseFromServer = reader.ReadToEnd();

                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = responseFromServer,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder);
            }

            client.Dispose();
        }

        [Command("suggest")]
        [Description("Make a suggestion")]
        public async Task Suggest(CommandContext ctx, [RemainingText] string suggestion)
        {
            var suggestionChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "suggestions-log");

            if (suggestionChannel == null)
            {
                var messageBuilder1 = new DiscordMessageBuilder
                {
                    Content = "An Error has occured while trying to log your suggestion. Please contact an Admin and ask them to ensure the Suggestion-Log channel is set up.",
                };

                messageBuilder1.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder1);

                return;
            }

            else
            {
                var newSuggestion = new Suggestion
                {
                    GuildId = ctx.Guild.Id,
                    SuggestorId = ctx.Member.Id,
                    SuggestionText = suggestion,
                    RespondedTo = "NO",
                };

                await _suggestionService.CreateNewSuggestion(newSuggestion);

                var suggestionEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Suggestion Created by: {ctx.Member.DisplayName}",
                    Description = suggestion,
                    Color = DiscordColor.HotPink,
                };

                suggestionEmbed.AddField("To Approve this suggestion:", $"`!suggestion approve {newSuggestion.Id}`");
                suggestionEmbed.AddField("To Decline this suggestion:", $"`!suggestion reject {newSuggestion.Id}`");

                suggestionEmbed.WithFooter($"Suggestion: {newSuggestion.Id}");

                var message = await suggestionChannel.SendMessageAsync(embed: suggestionEmbed);

                newSuggestion.SuggestionEmbedMessage = message.Id;

                await _suggestionService.EditSuggestion(newSuggestion);

                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = "Your suggestion has been logged!",
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder);
            }
        }

        [Command("botstats")]
        public async Task BotStats(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "GG-Bot Stats",
                Color = ctx.Guild.CurrentMember.Color,
            };

            var guilds = ctx.Client.Guilds.Values;

            int channelCount = new();

            foreach (DiscordGuild guild in guilds)
            {
                var thisGuildsChannels = guild.Channels.Count;

                channelCount = thisGuildsChannels + channelCount;
            }

            int memberCount = new();

            foreach (DiscordGuild guild in guilds)
            {
                var thisGuildsMembers = guild.MemberCount;

                memberCount = thisGuildsMembers + memberCount;
            }

            var guildCount = ctx.Client.Guilds.Count;

            var nowLiveChannelCount = _nowLiveStreamerService.GetNowLiveStreamerList().Count;

            var botVersion = typeof(Bot).Assembly.GetName().Version.ToString();

            var botStartTime = Process.GetCurrentProcess().StartTime;

            var botUptime = DateTime.Now - botStartTime;

            var tweetsMonitored = _twitterService.GetAllMonitoredAccounts().Count;

            embed.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

            embed.WithFooter($"Stats last updated: {DateTime.Now} (UK Time)");
            embed.AddField("Discord Guilds:", guildCount.ToString("###,###,###,###"), false);
            embed.AddField("Discord Members:", memberCount.ToString("###,###,###,###"), false);
            embed.AddField("Discord Channels:", channelCount.ToString("###,###,###,###"), false);
            if (nowLiveChannelCount > 0) { embed.AddField("Twitch Channels:", nowLiveChannelCount.ToString("###,###,###,###"), false); }
            if (nowLiveChannelCount == 0) { embed.AddField("Twitch Channels:", "0", false); }
            if (tweetsMonitored > 0) { embed.AddField("Twitter Accounts:", $"{tweetsMonitored:###,###,###,###}", false); }
            if (tweetsMonitored == 0) { embed.AddField("Twitter Accounts:", "0", false); }
            embed.AddField("Bot Version:", botVersion, false);
            embed.AddField("Uptime:", $"{botUptime.Days}d {botUptime.Hours}h {botUptime.Minutes:00}m", false);
            embed.AddField("Ping:", $"{ctx.Client.Ping:###,###,###,###}ms", false);

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = embed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1);
        }

        [Command("uptime")]
        public async Task Uptime(CommandContext ctx)
        {
            var botStartTime = Process.GetCurrentProcess().StartTime;

            var botUptime = DateTime.Now - botStartTime;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"The Bot has been Live for: {botUptime.Days}d {botUptime.Hours}h {botUptime.Minutes}m",
                Description = $"The bot has been live since: {botStartTime.Hour:00}:{botStartTime.Minute:00} GMT on {botStartTime.Day:00}/{botStartTime.Month:00}/{botStartTime.Year:00}",
                Color = DiscordColor.Blurple
            };

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = embed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1);
        }

        [Command("advice")]
        public async Task Advice(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var request = new HttpClient
            {
                BaseAddress = new Uri("https://api.adviceslip.com/")
            };

            HttpResponseMessage response = await request.GetAsync("advice");

            var resp = await response.Content.ReadAsStringAsync();

            var formatted1 = resp.Replace("{\"slip\":", string.Empty);
            var formatted2 = formatted1.Replace("\"}}", "\"}");

            var advice = JsonConvert.DeserializeObject<Advice>(formatted2);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Advice #{advice.Id}",
                Description = advice.AdviceOutput,
                Color = DiscordColor.Aquamarine
            };

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = embed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1);
        }

        [Command("serverstats")]
        public async Task ServerStats(CommandContext ctx)
        {
            var guild = ctx.Guild;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{guild.Name} Stats",
                Color = guild.CurrentMember.Color,
            };

            int channelCount = guild.Channels.Count;

            int memberCount = guild.MemberCount;

            var nowLiveChannelCount = _context.NowLiveStreamers.Where(x => x.GuildId == ctx.Guild.Id).Count();

            var tweetsMonitored = _twitterService.GetGuildMonitors(ctx.Guild.Id).Count;

            embed.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

            embed.WithFooter($"Stats last updated: {DateTime.Now} (UK Time)");
            embed.AddField("Discord Members:", memberCount.ToString("###,###,###,###"), false);
            embed.AddField("Discord Channels:", channelCount.ToString("###,###,###,###"), false);
            if (nowLiveChannelCount > 0) { embed.AddField("Twitch Channels:", nowLiveChannelCount.ToString("###,###,###,###"), false); }
            if (nowLiveChannelCount == 0) { embed.AddField("Twitch Channels:", "0", false); }
            if (tweetsMonitored > 0) { embed.AddField("Twitter Accounts:", $"{tweetsMonitored.ToString("###,###,###,###")}", false); }
            if (tweetsMonitored == 0) { embed.AddField("Twitter Accounts:", "0", false); }
            embed.AddField("Ping:", $"{ctx.Client.Ping:###,###,###,###}ms", false);

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = embed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1);
        }

        [Command("swquote")]
        public async Task StarWarsQuote(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            HttpClientHandler clientHandler = new()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            var request = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri("http://swquotesapi.digitaljedi.dk/api/SWQuote/"),
            };

            HttpResponseMessage response = await request.GetAsync("RandomStarWarsQuote");

            var resp = await response.Content.ReadAsStringAsync();

            var quote = JsonConvert.DeserializeObject<StarWarsQuotes>(resp);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Star Wars Quote #{quote.Id}",
                Description = quote.Content,
                Color = DiscordColor.Orange
            };

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = embed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1);
        }

        [Command("simpsons")]
        public async Task SimpsonsQuote(CommandContext ctx)
        {
            var quote = await _simpsonsQuoteService.GetSimpsonsQuote();

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{quote.Quote}",
                Description = $"{quote.Character}",
                ImageUrl = quote.ImageURL,
                Color = DiscordColor.Yellow,
            };

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = embed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1);
        }

        [Command("bestgirl")]
        [Description("DudeBDR only Command!")]
        public async Task RemIsBestGirl(CommandContext ctx)
        {
            if (ctx.Member.Id == 498613660118155264)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "REM IS BEST GIRL. NO EXCEPTION",
                    ImageUrl = "https://i.pinimg.com/originals/14/02/9c/14029cb13f98ddd6edd0f5fddf401f87.jpg",
                    Color = DiscordColor.Azure,
                };

                var messageBuilder1 = new DiscordMessageBuilder
                {
                    Embed = embed,
                };

                messageBuilder1.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder1);
            }

            else
            {
                var messageBuilder1 = new DiscordMessageBuilder
                {
                    Content = "Wait... You're not DudeBDR, but still, REM IS BEST GIRL!",
                };

                messageBuilder1.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder1);
            }
        }

        [Command("affirmation")]
        [Description("Get a random affirmation of the day!")]
        public async Task RandomAffirmation(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            HttpClient client = new();

            using (Stream dataStream = await client.GetStreamAsync("https://api.koston.eu/affirmation"))
            {
                StreamReader reader = new(dataStream);

                string responseFromServer = reader.ReadToEnd();

                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = $"Cute and Positive Affirmation: {responseFromServer}",
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder);
            }

            client.Dispose();
        }
    }
}