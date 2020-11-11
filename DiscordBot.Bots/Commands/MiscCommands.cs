using DiscordBot.Bots.JsonConverts;
using DiscordBot.Core.Services.CommunityStreamers;
using DiscordBot.Core.Services.Configs;
using DiscordBot.Core.Services.Suggestions;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.CommunityStreamers;
using DiscordBot.DAL.Models.ReactionRoles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchLib.Api;

namespace DiscordBot.Bots.Commands
{
    public class MiscCommands : BaseCommandModule
    {
        private readonly RPGContext _context;
        private readonly ISuggestionService _suggestionService;
        private readonly ICommunityStreamerService _communityStreamerService;
        private readonly IGuildStreamerConfigService _guildStreamerConfigService;
        private readonly IConfiguration _configuration;

        public MiscCommands(RPGContext context, ISuggestionService suggestionService, ICommunityStreamerService communityStreamerService, IGuildStreamerConfigService guildStreamerConfigService, IConfiguration configuration)
        {
            _context = context;
            _suggestionService = suggestionService;
            _communityStreamerService = communityStreamerService;
            _guildStreamerConfigService = guildStreamerConfigService;
            _configuration = configuration;
        }

        [Command("ping")]
        [Description("Play Ping-Pong with the Bot")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("mathadd")]
        [Description("Add two numbers together")]
        public async Task MathAdd(CommandContext ctx, [Description("First Number")] int numberOne, [Description("Second Number")] int numberTwo)
        {
            await ctx.Channel.SendMessageAsync((numberOne + numberTwo).ToString()).ConfigureAwait(false);
        }

        [Command("d12")]
        [Description("Rolls a 12 sided Dice")]
        public async Task RollTwelveDie(CommandContext ctx)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"🎲 The D12 has been rolled and the result is: {rnd.Next(1, 12)}").ConfigureAwait(false);
        }

        [Command("roles")]
        [Description("See what role commands you can use!")]
        public async Task Roles(CommandContext ctx)
        {
            var rolesEmbed = new DiscordEmbedBuilder
            {
                Title = "Role's You Can Self Assign!",
                Description = "Check the list below to find out what commands you can use to get your roles!",
                Color = DiscordColor.Blurple
            };
            rolesEmbed.AddField("`!role 18+`", "Gets the 18+ Role");
            rolesEmbed.WithFooter($"Command run by {ctx.Member.DisplayName}");

            await ctx.Channel.SendMessageAsync(embed: rolesEmbed).ConfigureAwait(false);
        }

        [Command("cmcs")]
        [Description("Alex only Command")]
        [RequireRoles(RoleCheckMode.Any, "Dotty <3")]
        public async Task ChristianMinecraftServer(CommandContext ctx, DiscordMember member)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"You have been banned from {ctx.Member.Mention}'s Christian Minecraft Server {member.Mention}! HOW DARE!").ConfigureAwait(false);
        }

        [Command("dadjoke")]
        [Description("Get a Dad Joke!")]
        public async Task DadJoke(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            WebRequest request = WebRequest.Create("https://api.scorpstuff.com/dadjokes.php");

            WebResponse response = request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);

                string responseFromServer = reader.ReadToEnd();

                await ctx.Channel.SendMessageAsync(responseFromServer).ConfigureAwait(false);
            }

            response.Close();
        }

        [Command("suggest")]
        [Description("Make a suggestion")]
        public async Task Suggest(CommandContext ctx, [RemainingText] string suggestion)
        {
            var suggestionChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "suggestions-log");

            if(suggestionChannel == null) { await ctx.Channel.SendMessageAsync("An Error has occured while trying to log your suggestion. Please contact an Admin and ask them to ensure the Suggestion-Log channel is set up.").ConfigureAwait(false); return; }

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

            await ctx.Channel.SendMessageAsync("Your suggestion has been logged!").ConfigureAwait(false);
        }

        [Command("ImAStreamer")]
        [Description("Let us Know Your Streamer Tag!")]
        public async Task StreamerTag(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("To let us know your're a streamer, please do the following command `!twitchchannel YourTwitchUserName`");
        }

        [Command("TwitchChannel")]
        [Description("Let us Know Your Streamer Tag!")]
        public async Task StreamerTag(CommandContext ctx, [RemainingText] string twitchUserName)
        {
            var api = new TwitchAPI();

            var clientid = _configuration["twitch-clientid"];
            var accesstoken = _configuration["twitch-accesstoken"];

            api.Settings.ClientId = clientid;
            api.Settings.AccessToken = accesstoken;

            var searchStreamer = await api.V5.Search.SearchChannelsAsync(twitchUserName);

            if (searchStreamer.Total == 0) { await ctx.Channel.SendMessageAsync($"There was no channel with the username {twitchUserName}."); return; }

            var streamerChannel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "streamers-to-approve");

            if (streamerChannel == null) { await ctx.Channel.SendMessageAsync("An Error has occured while trying to log your request. Please contact an Admin and ask them to ensure the streamers-to-approve channel is set up.").ConfigureAwait(false); return; }

            var newStreamer = new CommunityStreamer
            {
                GuildId = ctx.Guild.Id,
                requestorId = ctx.Member.Id,
                streamerName = twitchUserName,
                DealtWith = "NO",
            };

            await _communityStreamerService.CreateNewStreamer(newStreamer);

            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = $"New Streamer Request Created by: {ctx.Member.DisplayName}",
                Description = twitchUserName,
                Color = DiscordColor.HotPink,
            };

            suggestionEmbed.AddField("To Approve this streamer:", $"`!streamer approve {newStreamer.Id}` #channel-name");
            suggestionEmbed.AddField("To Decline this streamer:", $"`!streamer reject {newStreamer.Id}`");

            suggestionEmbed.WithFooter($"Suggestion: {newStreamer.Id}");

            var message = await streamerChannel.SendMessageAsync(embed: suggestionEmbed).ConfigureAwait(false);

            newStreamer.RequestMessage = message.Id;

            await _communityStreamerService.EditStreamer(newStreamer);

            await ctx.Channel.SendMessageAsync("Your request has been logged!").ConfigureAwait(false);
        }

        [Command("gifme")]
        public async Task GifMe(CommandContext ctx)
        {
            var search = "";

            await GifMeTask(ctx, search);
        }

        [Command("gifme")]
        public async Task GifMe(CommandContext ctx, [RemainingText] string search)
        {
            await GifMeTask(ctx, search);
        }

        public async Task GifMeTask(CommandContext ctx, [RemainingText] string search)
        {
            var giphy = new Giphy(_configuration["giphy-accesstoken"]);

            if(search == "")
            {
                var result = await giphy.RandomGif(new RandomParameter()
                {
                    Rating = Rating.R,
                });

                await ctx.Channel.SendMessageAsync(result.Data.ImageUrl);
            }
            else
            {
                var searchParams = new SearchParameter()
                {
                    Query = search,
                    Rating = Rating.R,
                };

                var searchResult = await giphy.GifSearch(searchParams);

                var searchCount = searchResult.Data.Count();

                var rand = new Random();
                var randomElement = rand.Next(0, searchCount +1);

                var result = searchResult.Data.ElementAt(randomElement);

                await ctx.Channel.SendMessageAsync(result.Images.Original.Url);
            }
            
        }

        [Command("stats")]
        public async Task BotStats(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "GG-Bot Stats",
                Color = ctx.Guild.CurrentMember.Color,
            };

            var guilds = ctx.Client.Guilds.Values;

            int channelCount = new int();

            foreach(DiscordGuild guild in guilds)
            {
                var thisGuildsChannels = guild.Channels.Count();

                channelCount = thisGuildsChannels + channelCount;
            }

            int memberCount = new int();

            foreach(DiscordGuild guild in guilds)
            {
                var thisGuildsMembers = guild.MemberCount;

                memberCount = thisGuildsMembers + memberCount;
            }

            var guildCount = ctx.Client.Guilds.Count();

            var nowLiveChannelCount = _guildStreamerConfigService.GetGuildStreamerList().Count();

            var botVersion = typeof(Bot).Assembly.GetName().Version.ToString();

            embed.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

            embed.WithFooter($"Stats last updated: {DateTime.Now} (UK Time)");
            embed.AddField("Discord Guilds:", guildCount.ToString("###,###,###,###"), false);
            embed.AddField("Discord Members:", memberCount.ToString("###,###,###,###"), false);
            embed.AddField("Discord Channels:", channelCount.ToString("###,###,###,###"), false);
            embed.AddField("Twitch Channels:", nowLiveChannelCount.ToString("###,###,###,###"), false);
            embed.AddField("Bot Version:", botVersion);
            embed.AddField("Ping:", $"{ctx.Client.Ping:###,###,###,###}ms", false);

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("simpsons")]
        [Description("Get a Simpsons Quote!")]
        public async Task SimpsonsQuote(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var request = new HttpClient();

            request.BaseAddress = new Uri("http://thesimpsonsquoteapi.glitch.me");

            request.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            request.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
            request.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            request.DefaultRequestHeaders.Add("Connection", "keep-alive");
            request.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            request.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36");

            HttpResponseMessage response = await request.GetAsync("quotes");

            var resp = await response.Content.ReadAsStringAsync();

            var formatted = resp.Replace("[", string.Empty);
            var formattedresp = formatted.Replace("]", string.Empty);

            var simpsons = JsonConvert.DeserializeObject<TheSimpsons>(formattedresp);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{simpsons.Quote}",
                Description = $"{simpsons.Character}",
                Color = DiscordColor.Yellow,
            };

            embed.WithImageUrl(simpsons.ImageURL);

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("advice")]
        public async Task Advice(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var request = new HttpClient();

            request.BaseAddress = new Uri("https://api.adviceslip.com/");

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

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
    }
}
