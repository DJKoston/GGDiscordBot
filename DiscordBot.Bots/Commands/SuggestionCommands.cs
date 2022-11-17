namespace DiscordBot.Bots.Commands
{
    [Group("suggestion")]
    [RequirePermissions(DSharpPlus.Permissions.Administrator)]

    public class SuggestionCommands : BaseCommandModule
    {
        private readonly ISuggestionService _suggestionService;

        public SuggestionCommands(ISuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        [Command("approve")]
        public async Task ApproveSuggestion(CommandContext ctx, int suggestionId)
        {
            var suggestion = await _suggestionService.GetSuggestion(ctx.Guild.Id, suggestionId);

            if (suggestion == null)
            {
                await ctx.Message.DeleteAsync();

                var approveRespnse = await ctx.Channel.SendMessageAsync("This ID does not exist. Please try again.");

                await Task.Delay(5000);

                await approveRespnse.DeleteAsync();

                return;
            }

            if (suggestion.RespondedTo == "APPROVED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This suggestion has already been approved!");

                await Task.Delay(5000);

                await approvedResponse.DeleteAsync();

                return;
            };

            if (suggestion.RespondedTo == "DENIED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This suggestion has already been rejected!");

                await Task.Delay(5000);

                await approvedResponse.DeleteAsync();

                return;
            }

            DiscordMember suggestor = await ctx.Guild.GetMemberAsync(suggestion.SuggestorId);

            if (suggestor != null) { await suggestor.SendMessageAsync($"Your Suggestion: `{suggestion.SuggestionText}` has been approved in the {ctx.Guild.Name} server!"); }

            suggestion.RespondedTo = "APPROVED";

            await _suggestionService.EditSuggestion(suggestion);

            DiscordChannel channel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "suggestions-log");

            DiscordMessage message = await channel.GetMessageAsync(suggestion.SuggestionEmbedMessage);

            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = $"Suggestion Created by: {suggestor.DisplayName}",
                Description = suggestion.SuggestionText,
                Color = DiscordColor.Green,
            };

            suggestionEmbed.AddField("This Suggestion Was Approved by:", $"{ctx.Member.Mention}");

            suggestionEmbed.WithFooter($"Suggestion: {suggestion.Id}");

            DiscordEmbed newEmbed = suggestionEmbed;

            await message.ModifyAsync(embed: newEmbed);

            await ctx.Message.DeleteAsync();

            var response = await ctx.Channel.SendMessageAsync($"Suggestion {suggestion.Id} has been approved!");

            await Task.Delay(5000);

            await response.DeleteAsync();
        }

        [Command("reject")]
        public async Task Reject(CommandContext ctx, int suggestionId, [RemainingText] string reason)
        {
            var suggestion = await _suggestionService.GetSuggestion(ctx.Guild.Id, suggestionId);

            if (suggestion == null)
            {
                await ctx.Message.DeleteAsync();

                var approveRespnse = await ctx.Channel.SendMessageAsync("This ID does not exist. Please try again.");

                await Task.Delay(5000);

                await approveRespnse.DeleteAsync();

                return;
            }

            if (suggestion.RespondedTo == "APPROVED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This suggestion has already been approved!");

                await Task.Delay(5000);

                await approvedResponse.DeleteAsync();

                return;
            };

            if (suggestion.RespondedTo == "DENIED")
            {
                await ctx.Message.DeleteAsync();

                var approvedResponse = await ctx.Channel.SendMessageAsync("This suggestion has already been rejected!");

                await Task.Delay(5000);

                await approvedResponse.DeleteAsync();

                return;
            }

            DiscordMember suggestor = await ctx.Guild.GetMemberAsync(suggestion.SuggestorId);

            if (reason == null) { }

            if (suggestor != null)
            {
                if (reason == null)
                {
                    await suggestor.SendMessageAsync($"Your Suggestion: `{suggestion.SuggestionText}` has been rejected in the {ctx.Guild.Name} server for the following reason:\n\n No reason given.");
                }
                else
                {
                    await suggestor.SendMessageAsync($"Your Suggestion: `{suggestion.SuggestionText}` has been rejected in the {ctx.Guild.Name} server for the following reason:\n\n {reason}");
                }

            }

            suggestion.RespondedTo = "DENIED";

            await _suggestionService.EditSuggestion(suggestion);

            DiscordChannel channel = ctx.Guild.Channels.Values.FirstOrDefault(x => x.Name == "suggestions-log");

            DiscordMessage message = await channel.GetMessageAsync(suggestion.SuggestionEmbedMessage);

            var suggestionEmbed = new DiscordEmbedBuilder
            {
                Title = $"Suggestion Created by: {suggestor.DisplayName}",
                Description = suggestion.SuggestionText,
                Color = DiscordColor.Red,
            };

            suggestionEmbed.AddField("This Suggestion Was Rejected by:", $"{ctx.Member.Mention}");
            suggestionEmbed.AddField("Reason:", reason);

            suggestionEmbed.WithFooter($"Suggestion: {suggestion.Id}");

            DiscordEmbed newEmbed = suggestionEmbed;

            await message.ModifyAsync(embed: newEmbed);

            await ctx.Message.DeleteAsync();

            var response = await ctx.Channel.SendMessageAsync($"Suggestion {suggestion.Id} has been rejected!");

            await Task.Delay(5000);

            await response.DeleteAsync();
        }

    }
}
