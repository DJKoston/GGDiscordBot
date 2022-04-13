namespace DiscordBot.Bots.Commands
{
    public class PollCommands : BaseCommandModule
    {
        private TimeSpan convertedTimeSpan;

        [Command("poll")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task CreatePoll(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();

            var warningMessage = await ctx.Channel.SendMessageAsync("To use this command:\n\n`!poll \"This is my poll question\" \"This is my detailed options\" 1h :emoji1: :emoji2:`");

            await Task.Delay(10000);

            await warningMessage.DeleteAsync();
        }

        [Command("poll")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task CreatePoll(CommandContext ctx, string pollQuestion, string pollOptions, string timeSpan, params DiscordEmoji[] pollEmojis)
        {
            await ctx.Message.DeleteAsync();

            if (timeSpan.Contains('s'))
            {
                var timeToConvert = timeSpan.Replace("s", "").ToString();

                var doubl = Double.Parse(timeToConvert);

                convertedTimeSpan = TimeSpan.FromSeconds(doubl);
            }

            if (timeSpan.Contains('m'))
            {
                var timeToConvert = timeSpan.Replace("m", "").ToString();

                var doubl = Double.Parse(timeToConvert);

                convertedTimeSpan = TimeSpan.FromMinutes(doubl);
            }

            if (timeSpan.Contains('h'))
            {
                var timeToConvert = timeSpan.Replace("h", "").ToString();

                var doubl = Double.Parse(timeToConvert);

                convertedTimeSpan = TimeSpan.FromHours(doubl);
            }

            if (timeSpan.Contains('d'))
            {
                var timeToConvert = timeSpan.Replace("d", "").ToString();

                var doubl = Double.Parse(timeToConvert);

                convertedTimeSpan = TimeSpan.FromDays(doubl);
            }

            var interactivity = ctx.Client.GetInteractivity();

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = pollQuestion,
                Description = pollOptions
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(pollEmbed);

            var poll = await interactivity.DoPollAsync(pollMessage, pollEmojis, DSharpPlus.Interactivity.Enums.PollBehaviour.KeepEmojis, convertedTimeSpan);

            var list = new List<PollList>();

            foreach (DiscordEmoji emoji in pollEmojis)
            {
                var result = poll.FirstOrDefault(x => x.Emoji == emoji);

                var resultCount = result.Voted.Count();

                var resultToEnter = new PollList
                {
                    Option = result.Emoji.GetDiscordName(),
                    Votes = resultCount
                };

                list.Add(resultToEnter);
            }

            var winner = list.OrderByDescending(x => x.Votes).FirstOrDefault();

            var pollEndedEmbed = new DiscordEmbedBuilder
            {
                Title = $"Poll Results: {pollQuestion}",
            };

            if(winner.Votes == 1)
            {
                pollEndedEmbed.WithDescription($"Poll Winner is: {winner.Option} with {winner.Votes} Vote.");
            }

            else
            {
                pollEndedEmbed.WithDescription($"Poll Winner is: {winner.Option} with {winner.Votes} Votes.");
            }

            await pollMessage.RespondAsync(pollEndedEmbed);
        }
    }
}
