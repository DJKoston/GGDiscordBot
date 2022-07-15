namespace DiscordBot.Bots.SlashCommands
{
    public class MiscSlashCommands : ApplicationCommandModule
    {
        private readonly RPGContext _context;
        private readonly ISuggestionService _suggestionService;
        private readonly ICommunityStreamerService _communityStreamerService;
        private readonly INowLiveStreamerService _nowLiveStreamerService;
        private readonly IGoodBotBadBotService _goodBotBadBotService;
        private readonly IConfiguration _configuration;
        private readonly ISimpsonsQuoteService _simpsonsQuoteService;

        public MiscSlashCommands(RPGContext context, ISuggestionService suggestionService, ICommunityStreamerService communityStreamerService, INowLiveStreamerService nowLiveStreamerService, IGoodBotBadBotService goodBotBadBotService, IConfiguration configuration, ISimpsonsQuoteService simpsonsQuoteService)
        {
            _context = context;
            _suggestionService = suggestionService;
            _communityStreamerService = communityStreamerService;
            _nowLiveStreamerService = nowLiveStreamerService;
            _goodBotBadBotService = goodBotBadBotService;
            _configuration = configuration;
            _simpsonsQuoteService = simpsonsQuoteService;
        }

        [SlashCommand("d12", "Roll a D12 Dice")]
        public async Task SlashRollTwelveDie(InteractionContext ctx)
        {
            var rnd = new Random();

            await ctx.CreateResponseAsync($"🎲 The D12 has been rolled and the result is: {rnd.Next(1, 13)}");
        }

        [SlashCommand("dadjoke", "Get a dad joke!")]
        public async Task SlashDadJoke(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("I am currently pulling a joke from my Dad-abase!");

            HttpClient client = new();

            using (Stream dataStream = await client.GetStreamAsync("https://api.scorpstuff.com/dadjokes.php"))
            {
                StreamReader reader = new(dataStream);

                string responseFromServer = reader.ReadToEnd();

                var messageBuilder = new DiscordWebhookBuilder
                {
                    Content = responseFromServer,
                };

                await ctx.EditResponseAsync(messageBuilder);
            }

            client.Dispose();
        }

        [SlashCommand("suggest", "Make a suggestion for the Discord!")]
        public async Task SlashSuggest(InteractionContext ctx,
            [Option("suggestion", "What would you like to suggest?")] string suggestion)
        {
            var suggestionChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "suggestions-log");

            if (suggestionChannel == null)
            {
                await ctx.CreateResponseAsync("An Error has occured while trying to log your suggestion. Please contact an Admin and ask them to ensure the suggestions-log channel is set up.", true);

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

                var message = await suggestionChannel.SendMessageAsync(embed: suggestionEmbed).ConfigureAwait(false);

                newSuggestion.SuggestionEmbedMessage = message.Id;

                await _suggestionService.EditSuggestion(newSuggestion);

                await ctx.CreateResponseAsync("Your suggestion has been logged!", true);
            }
        }


        [SlashCommand("botstats", "Get Bot Stats!")]
        public async Task SlashBotStats(InteractionContext ctx)
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

            embed.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

            embed.WithFooter($"Stats last updated: {DateTime.Now} (UK Time)");
            embed.AddField("Discord Guilds:", guildCount.ToString("###,###,###,###"), false);
            embed.AddField("Discord Members:", memberCount.ToString("###,###,###,###"), false);
            embed.AddField("Discord Channels:", channelCount.ToString("###,###,###,###"), false);
            if (nowLiveChannelCount > 0) { embed.AddField("Twitch Channels:", nowLiveChannelCount.ToString("###,###,###,###"), false); }
            if (nowLiveChannelCount == 0) { embed.AddField("Twitch Channels:", "0", false); }
            embed.AddField("Bot Version:", botVersion);
            embed.AddField("Ping:", $"{ctx.Client.Ping:###,###,###,###}ms", false);
            embed.AddField("Uptime:", $"{botUptime.Days}d {botUptime.Hours}h {botUptime.Minutes:00}m");

            DiscordInteractionResponseBuilder messageBuilder = new();

            messageBuilder.AddEmbed(embed);
            messageBuilder.IsEphemeral = true;
            await ctx.CreateResponseAsync(messageBuilder);
        }

        [SlashCommand("uptime", "Get's Bot Uptime")]
        public async Task SlashUptime(InteractionContext ctx)
        {
            var botStartTime = Process.GetCurrentProcess().StartTime;

            var botUptime = DateTime.Now - botStartTime;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"The Bot has been Live for: {botUptime.Days}d {botUptime.Hours}h {botUptime.Minutes}m",
                Description = $"The bot has been live since: {botStartTime.Hour:00}:{botStartTime.Minute:00} GMT on {botStartTime.Day:00}/{botStartTime.Month:00}/{botStartTime.Year:00}",
                Color = DiscordColor.Blurple
            };

            DiscordInteractionResponseBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embed);
            messageBuilder.IsEphemeral = true;
            await ctx.CreateResponseAsync(messageBuilder);
        }

        [SlashCommand("advice", "Need some advice? You can get some here!")]
        public async Task SlashAdvice(InteractionContext ctx)
        {
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

            DiscordInteractionResponseBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embed);
            await ctx.CreateResponseAsync(messageBuilder);
        }

        [SlashCommand("serverstats", "Get Server Stats!")]
        public async Task SlashServerStats(InteractionContext ctx)
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

            embed.WithThumbnail(guild.IconUrl);

            embed.WithFooter($"Stats last updated: {DateTime.Now} (UK Time)");
            if (memberCount > 0) { embed.AddField("Discord Members:", memberCount.ToString("###,###,###,###"), false); }
            if (memberCount == 0) { embed.AddField("Discord Members:", "0", false); }
            if (channelCount > 0) { embed.AddField("Discord Channels:", channelCount.ToString("###,###,###,###"), false); }
            if (channelCount == 0) { embed.AddField("Discord Channels:", "0", false); }
            if (nowLiveChannelCount > 0) { embed.AddField("Twitch Channels:", nowLiveChannelCount.ToString("###,###,###,###"), false); }
            if (nowLiveChannelCount == 0) { embed.AddField("Twitch Channels:", "0", false); }
            embed.AddField("Ping:", $"{ctx.Client.Ping:###,###,###,###}ms", false);

            DiscordInteractionResponseBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embed);
            messageBuilder.IsEphemeral = true;
            await ctx.CreateResponseAsync(messageBuilder);
        }

        [SlashCommand("swquote", "Get a random Star Wars Quote!")]
        public async Task SlashStarWarsQuote(InteractionContext ctx)
        {
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

            DiscordInteractionResponseBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embed);
            messageBuilder.IsEphemeral= true;
            await ctx.CreateResponseAsync(messageBuilder);
        }

        [SlashCommand("praised", "Get's how many times the bot has been praised!")]
        public async Task SlashPraised(InteractionContext ctx)
        {
            var goodBot = await _goodBotBadBotService.GetGoodBot();

            var messageString = "";

            if (goodBot.GoodBot == 0 || goodBot.GoodBot > 1)
            {
                messageString = $"I have been praised {goodBot.GoodBot} times! 😊😊😊";
            }

            if (goodBot.GoodBot == 1)
            {
                messageString = $"I have been praised {goodBot.GoodBot} time! 😊😊😊";
            }

            await ctx.CreateResponseAsync(messageString);
        }

        [SlashCommand("scolded", "Get's how many times the bot has been scolded!")]
        public async Task SlashScolded(InteractionContext ctx)
        {
            var badBot = await _goodBotBadBotService.GetBadBot();

            var messageString = "";

            if (badBot.BadBot == 0 || badBot.BadBot > 1)
            {
                messageString = $"I have been scolded {badBot.BadBot} times! 😞 I'll try to do better!";
            }

            if (badBot.BadBot == 1)
            {
                messageString = $"I have been scolded {badBot.BadBot} time! 😞 I'll try to do better!";
            }

            await ctx.CreateResponseAsync(messageString);
        }

        [SlashCommand("simpsonsquote", "Get a Random Simpsons Quote!")]
        public async Task SlashSimpsonsQuote(InteractionContext ctx)
        {
            var quote = await _simpsonsQuoteService.GetSimpsonsQuote();

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{quote.Quote}",
                Description = $"{quote.Character}",
                ImageUrl = quote.ImageURL,
                Color = DiscordColor.Yellow,
            };

            DiscordInteractionResponseBuilder responseBuilder = new();
            responseBuilder.AddEmbed(embed);
            responseBuilder.IsEphemeral = true;
            await ctx.CreateResponseAsync(responseBuilder);
        }

        [SlashCommand("affirmation", "Get a Random Affirmation, Useful for when you're feeling down.")]
        public async Task SlashAffirmation(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("I see you want some soothing words, one moment while I have a think.");

            HttpClient client = new();

            using (Stream dataStream = await client.GetStreamAsync("https://api.koston.eu/affirmation"))
            {
                StreamReader reader = new(dataStream);

                string responseFromServer = reader.ReadToEnd();

                var messageBuilder = new DiscordWebhookBuilder
                {
                    Content = $"Cute and Positive Affirmation: {responseFromServer}",
                };

                await ctx.EditResponseAsync(messageBuilder);
            }

            client.Dispose();
        }

        [SlashCommand("snowball", "Throw a snowball at someone in need of it!")]
        public async Task SlashSnowball(InteractionContext ctx, [Option("target", "User to target with the snowball")] DiscordUser user)
        {
            await ctx.CreateResponseAsync($"{ctx.Member.Mention} attempts to throw a snowball at {user.Mention}");

            HttpClient client = new();

            using (Stream dataStream = await client.GetStreamAsync("https://api.koston.eu/snowball"))
            {
                StreamReader reader = new(dataStream);

                string responseFromServer = reader.ReadToEnd();

                string newResponse = responseFromServer.Replace("$thrower", ctx.Member.Mention);

                var messageBuilder = new DiscordFollowupMessageBuilder
                {
                    Content = newResponse,
                };

                await Task.Delay(2000);

                await ctx.FollowUpAsync(messageBuilder);
            }

            client.Dispose();
        }
    }
}
