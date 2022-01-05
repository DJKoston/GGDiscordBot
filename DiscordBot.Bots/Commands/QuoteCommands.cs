namespace DiscordBot.Bots.Commands
{
    public class QuoteCommands : BaseCommandModule
    {
        private readonly IQuoteService _quoteService;
        private readonly RPGContext _context;

        public QuoteCommands(RPGContext context, IQuoteService quoteService)
        {
            _context = context;
            _quoteService = quoteService;
        }

        //Make as Slash Command
        [Command("addquote")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task AddQuote(CommandContext ctx)
        {
            var messageBuilder = new DiscordMessageBuilder
            {
                Content = "Please add a quote as such: `!addquote @username {Quote}` - DO NOT add the curly braces!",
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
        }

        [Command("addquote")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task AddQuote(CommandContext ctx, DiscordMember discordMember, [RemainingText] string quote)
        {
            var serverQuotes = _context.Quotes.Where(x => x.GuildId == ctx.Guild.Id);
            var quoteQuery = serverQuotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            if (lastQuoteId == null)
            {
                int newQuoteId = 1;

                var quoteDb = new Quote()
                {
                    GuildId = ctx.Guild.Id,
                    QuoteId = newQuoteId,
                    AddedById = ctx.Member.Id,
                    DiscordUserQuotedId = discordMember.Id,
                    QuoteContents = quote,
                    DateAdded = DateTime.Now.ToString(),
                    ChannelQuotedIn = ctx.Channel.Name,
                };

                await _quoteService.CreateNewQuoteAsync(quoteDb).ConfigureAwait(false);

                var quoteAddEmbed = new DiscordEmbedBuilder
                {
                    Title = "Quote Added",
                    Description = $"{quoteDb.QuoteContents} - {discordMember.DisplayName}",
                    Color = discordMember.Color,
                };

                quoteAddEmbed.WithThumbnail(discordMember.AvatarUrl);
                quoteAddEmbed.AddField("Quote ID:", quoteDb.QuoteId.ToString());
                quoteAddEmbed.AddField("Quoted By:", ctx.Member.DisplayName);
                quoteAddEmbed.WithFooter(quoteDb.DateAdded, "https://www.kindpng.com/picc/b/10-101445_white-clock-icon-png.png");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = quoteAddEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }

            if (lastQuoteId.QuoteId >= 1)
            {
                int newQuoteId = lastQuoteId.QuoteId + 1;

                var quoteDb = new Quote()
                {
                    GuildId = ctx.Guild.Id,
                    QuoteId = newQuoteId,
                    AddedById = ctx.Member.Id,
                    DiscordUserQuotedId = discordMember.Id,
                    QuoteContents = quote,
                    DateAdded = DateTime.Now.ToString(),
                    ChannelQuotedIn = ctx.Channel.Name,
                };

                await _quoteService.CreateNewQuoteAsync(quoteDb).ConfigureAwait(false);

                var quoteAddEmbed = new DiscordEmbedBuilder
                {
                    Title = "Quote Added",
                    Description = $"{quoteDb.QuoteContents} - {discordMember.DisplayName}",
                    Color = discordMember.Color,
                };

                quoteAddEmbed.WithThumbnail(discordMember.AvatarUrl);
                quoteAddEmbed.AddField("Quote ID:", quoteDb.QuoteId.ToString());
                quoteAddEmbed.AddField("Quoted By:", ctx.Member.DisplayName);
                quoteAddEmbed.WithFooter(quoteDb.DateAdded, "https://www.kindpng.com/picc/b/10-101445_white-clock-icon-png.png");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = quoteAddEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }
        }

        //Make as Slash Command
        [Command("totalquotes")]
        public async Task TotalQuotes(CommandContext ctx)
        {
            var serverQuotes = _context.Quotes.Where(x => x.GuildId == ctx.Guild.Id);
            var quoteQuery = serverQuotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            if (lastQuoteId == null)
            {
                var zeroQuoteEmbed = new DiscordEmbedBuilder
                {
                    Title = "There are currently 0 quotes in the bot!",
                    Color = DiscordColor.IndianRed,
                };

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = zeroQuoteEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }

            if (lastQuoteId.QuoteId == 1)
            {
                var zeroQuoteEmbed = new DiscordEmbedBuilder
                {
                    Title = $"There is currently {lastQuoteId.QuoteId} quote in the bot!",
                    Color = DiscordColor.Orange,
                };

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = zeroQuoteEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }

            if (lastQuoteId.QuoteId > 1)
            {
                var zeroQuoteEmbed = new DiscordEmbedBuilder
                {
                    Title = $"There are currently {lastQuoteId.QuoteId} quotes in the bot!",
                    Color = DiscordColor.Orange,
                };

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = zeroQuoteEmbed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }
        }

        //Make as Slash Command
        [Command("deletequote")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task DeleteQuote(CommandContext ctx, int quoteId)
        {
            var quote = await _quoteService.GetQuoteAsync(quoteId, ctx.Guild.Id);

            if (quote == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "There isnt a quote that exists with that quote number!"
                };

                embed.AddField("Try searching for another quote!", "For example `!deletequote 154`");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = embed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }

            var quoted = await ctx.Client.GetUserAsync(quote.DiscordUserQuotedId);
            var quoter = await ctx.Client.GetUserAsync(quote.AddedById);

            await _quoteService.DeleteQuoteAsync(quote);

            var quoteAddEmbed = new DiscordEmbedBuilder
            {
                Title = "Quote Deleted",
                Description = $"{quote.QuoteContents} - {quoted.Username}",
                Color = DiscordColor.IndianRed,
            };

            quoteAddEmbed.WithThumbnail(quoted.AvatarUrl);
            quoteAddEmbed.AddField("Quote ID:", quote.QuoteId.ToString());
            quoteAddEmbed.AddField("Quoted By:", quoter.Username);
            quoteAddEmbed.AddField("Deleted By:", ctx.Member.DisplayName);
            quoteAddEmbed.WithFooter(quote.DateAdded, "https://www.kindpng.com/picc/b/10-101445_white-clock-icon-png.png");

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = quoteAddEmbed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1).ConfigureAwait(false);
        }

        //Make as Slash Command
        [Command("quote")]
        public async Task GetQuote(CommandContext ctx)
        {
            var serverQuotes = _context.Quotes.Where(x => x.GuildId == ctx.Guild.Id);
            var quoteQuery = serverQuotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            if (lastQuoteId == null)
            {
                var messageBuilder3 = new DiscordMessageBuilder
                {
                    Content = "No Quote's exist. Please add one before searching for a quote.",
                };

                messageBuilder3.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder3).ConfigureAwait(false);

                return;
            }

            var rnd = new Random();
            int rndQuote = rnd.Next(1, lastQuoteId.QuoteId + 1);

            var quote = await _quoteService.GetQuoteAsync(rndQuote, ctx.Guild.Id);

            var quotedUser = await ctx.Client.GetUserAsync(quote.DiscordUserQuotedId);
            var quoterUser = await ctx.Client.GetUserAsync(quote.AddedById);

            var quoteAddEmbed = new DiscordEmbedBuilder
            {
                Title = $"Quote #{quote.QuoteId}",
                Description = $"{quote.QuoteContents} - {quotedUser.Username}",
                Color = DiscordColor.SpringGreen,
            };

            quoteAddEmbed.WithThumbnail(quotedUser.AvatarUrl);
            quoteAddEmbed.AddField("Quoted By:", quoterUser.Username);
            quoteAddEmbed.AddField("Channel Quoted in:", quote.ChannelQuotedIn);
            quoteAddEmbed.WithFooter(quote.DateAdded, "https://www.kindpng.com/picc/b/10-101445_white-clock-icon-png.png");

            var messageBuilder = new DiscordMessageBuilder
            {
                Embed = quoteAddEmbed,
            };

            messageBuilder.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
        }

        [Command("quote")]
        public async Task GetQuote(CommandContext ctx, int quoteNumber)
        {
            var quoteQuery = _context.Quotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            var quote = await _quoteService.GetQuoteAsync(quoteNumber, ctx.Guild.Id);

            var quotedUser = await ctx.Client.GetUserAsync(quote.DiscordUserQuotedId);
            var quoterUser = await ctx.Client.GetUserAsync(quote.AddedById);

            if (quoteNumber > lastQuoteId.Id)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "There isnt a quote that exists with that quote number!"
                };

                embed.AddField("Try searching for another quote!", "For example `!quote 154`");

                var messageBuilder = new DiscordMessageBuilder
                {
                    Embed = embed,
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }

            var quoteAddEmbed = new DiscordEmbedBuilder
            {
                Title = $"Quote #{quote.QuoteId}",
                Description = $"{quote.QuoteContents} - {quotedUser.Username}",
                Color = DiscordColor.SpringGreen,
            };

            quoteAddEmbed.WithThumbnail(quotedUser.AvatarUrl);
            quoteAddEmbed.AddField("Quoted By:", quoterUser.Username);
            quoteAddEmbed.AddField("Channel Quoted in:", quote.ChannelQuotedIn);
            quoteAddEmbed.WithFooter(quote.DateAdded, "https://www.kindpng.com/picc/b/10-101445_white-clock-icon-png.png");

            var messageBuilder1 = new DiscordMessageBuilder
            {
                Embed = quoteAddEmbed,
            };

            messageBuilder1.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder1).ConfigureAwait(false);
        }

        [Command("quote")]
        public async Task GetQuote(CommandContext ctx, DiscordMember discordMember)
        {
            var userQuotes = _context.Quotes.Where(x => x.DiscordUserQuotedId == discordMember.Id && x.GuildId == ctx.Guild.Id);

            var rnd = new Random();
            int rndQuote = rnd.Next(0, userQuotes.Count() + 1);

            var quote = userQuotes.Skip(rndQuote).Take(1).FirstOrDefault();

            if (quote == null)
            {
                var messageBuilder = new DiscordMessageBuilder
                {
                    Content = "We cannot find any quotes! Please try again or try another user.",
                };

                messageBuilder.WithReply(ctx.Message.Id, true);

                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);

                return;
            }

            var quotedUser = await ctx.Client.GetUserAsync(quote.DiscordUserQuotedId);
            var quoterUser = await ctx.Client.GetUserAsync(quote.AddedById);

            var quoteAddEmbed = new DiscordEmbedBuilder
            {
                Title = $"Quote #{quote.QuoteId}",
                Description = $"{quote.QuoteContents} - {quotedUser.Username}",
                Color = DiscordColor.SpringGreen,
            };

            quoteAddEmbed.WithThumbnail(quotedUser.AvatarUrl);
            quoteAddEmbed.AddField("Quoted By:", quoterUser.Username);
            quoteAddEmbed.AddField("Channel Quoted in:", quote.ChannelQuotedIn);
            quoteAddEmbed.WithFooter(quote.DateAdded, "https://www.kindpng.com/picc/b/10-101445_white-clock-icon-png.png");

            var messageBuilder2 = new DiscordMessageBuilder
            {
                Embed = quoteAddEmbed,
            };

            messageBuilder2.WithReply(ctx.Message.Id, true);

            await ctx.Channel.SendMessageAsync(messageBuilder2).ConfigureAwait(false);
        }
    }
}
