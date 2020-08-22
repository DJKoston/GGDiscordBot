using DiscordBot.Core.Services.Quotes;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Quotes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [Command("addquote")]
        public async Task AddQuote(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Please add a quote as such: `!addquote @username {Remainder of Quote}` - DO NOT add the curly braces!").ConfigureAwait(false);
        }

        [Command("addquote")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task AddQuote(CommandContext ctx, DiscordMember discordMember, [RemainingText] string quote)
        {
            var quoteQuery = _context.Quotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            if(lastQuoteId == null)
            {
                int newQuoteId = 1;

                var quoteDb = new Quote()
                {
                    GuildId = ctx.Guild.Id,
                    QuoteId = newQuoteId,
                    AddedById = ctx.Member.Id,
                    DiscordUserQuotedId = discordMember.Id,
                    QuoteContents = quote,
                    DateAdded = DateAndTime.Now.ToString(),
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

                await ctx.Channel.SendMessageAsync(embed: quoteAddEmbed).ConfigureAwait(false);

                return;
            }

            if(lastQuoteId.QuoteId >= 1)
            {
                int newQuoteId = lastQuoteId.QuoteId + 1;

                var quoteDb = new Quote()
                {
                    GuildId = ctx.Guild.Id,
                    QuoteId = newQuoteId,
                    AddedById = ctx.Member.Id,
                    DiscordUserQuotedId = discordMember.Id,
                    QuoteContents = quote,
                    DateAdded = DateAndTime.Now.ToString(),
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

                await ctx.Channel.SendMessageAsync(embed: quoteAddEmbed).ConfigureAwait(false);

                return;
            }

            
        }

        [Command("totalquotes")]
        public async Task TotalQuotes(CommandContext ctx)
        {
            var quoteQuery = _context.Quotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            if(lastQuoteId == null)
            {
                var zeroQuoteEmbed = new DiscordEmbedBuilder
                {
                    Title = "There are currently 0 quotes in the bot!",
                    Color = DiscordColor.IndianRed,
                };
                
                await ctx.Channel.SendMessageAsync(embed: zeroQuoteEmbed).ConfigureAwait(false);

                return;
            }

            if(lastQuoteId.QuoteId == 1)
            {
                var zeroQuoteEmbed = new DiscordEmbedBuilder
                {
                    Title = $"There is currently {lastQuoteId.QuoteId} quote in the bot!",
                    Color = DiscordColor.Orange,
                };

                await ctx.Channel.SendMessageAsync(embed: zeroQuoteEmbed).ConfigureAwait(false);

                return;
            }

            if(lastQuoteId.QuoteId > 1)
            {
                var zeroQuoteEmbed = new DiscordEmbedBuilder
                {
                    Title = $"There are currently {lastQuoteId.QuoteId} quotes in the bot!",
                    Color = DiscordColor.Orange,
                };

                await ctx.Channel.SendMessageAsync(embed: zeroQuoteEmbed).ConfigureAwait(false);

                return;
            }
        }

        [Command("deletequote")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task DeleteQuote(CommandContext ctx, int quoteId)
        {
            var quoteQuery = _quoteService.GetQuoteAsync(quoteId);
            var quote = quoteQuery.Result;

            if(quote == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "There isnt a quote that exists with that quote number!"
                };

                embed.AddField("Try searching for another quote!", "For example `!deletequote 154`");
                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

               

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

            await ctx.Channel.SendMessageAsync(embed: quoteAddEmbed).ConfigureAwait(false);
        }

        [Command("quote")]
        public async Task GetQuote(CommandContext ctx)
        {
            var quoteQuery = _context.Quotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            if(lastQuoteId == null)
            {
                await ctx.Channel.SendMessageAsync("No Quote's exist. Please add one before searching for a quote.").ConfigureAwait(false);

                return;
            }

            var rnd = new Random();
            int rndQuote = rnd.Next(1, lastQuoteId.QuoteId + 1);

            var quote = _quoteService.GetQuoteAsync(rndQuote).Result;

            var quotedUser = ctx.Client.GetUserAsync(quote.DiscordUserQuotedId).Result;
            var quoterUser = ctx.Client.GetUserAsync(quote.AddedById).Result;

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

            await ctx.Channel.SendMessageAsync(embed: quoteAddEmbed).ConfigureAwait(false);

        }

        [Command("quote")]
        public async Task GetQuote(CommandContext ctx, int quoteNumber)
        {
            var quoteQuery = _context.Quotes.Where(x => x.QuoteId > 0).OrderByDescending(x => x.QuoteId).Take(1);
            var lastQuoteId = quoteQuery.FirstOrDefault(x => x.QuoteId > 0);

            var quote = _quoteService.GetQuoteAsync(quoteNumber).Result;

            var quotedUser = ctx.Client.GetUserAsync(quote.DiscordUserQuotedId).Result;
            var quoterUser = ctx.Client.GetUserAsync(quote.AddedById).Result;

            if(quoteNumber > lastQuoteId.Id)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "There isnt a quote that exists with that quote number!"
                };

                embed.AddField("Try searching for another quote!", "For example `!quote 154`");
                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

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

            await ctx.Channel.SendMessageAsync(embed: quoteAddEmbed).ConfigureAwait(false);
        }
    }
}
