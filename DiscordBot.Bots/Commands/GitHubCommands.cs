using Octokit;

namespace DiscordBot.Bots.Commands
{
    public class GitHubCommands : BaseCommandModule
    {
        private readonly IConfiguration _configuration;

        public GitHubCommands(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Command("newissue")]
        [Description("Open a new GitHub Issue for GG-Bot")]
        public async Task AddNewIssue(CommandContext ctx, [RemainingText]string issue)
        {
            var github = new GitHubClient(new ProductHeaderValue("gg-bot"));

            var tokenAuth = new Credentials(_configuration["github-token"]);
            github.Credentials = tokenAuth;

            var repoOwner = "DJKoston";
            var repoName = "GGDiscordBot";

            var createIssue = new NewIssue($"New Issue from Discord User: {ctx.User.Username}");
            createIssue.Body = $"## Issue Details:\n### Issue Creator:\nUsername: {ctx.User.Username}\nServer: {ctx.Guild.Name}\n\nIssue: {issue}";

            var newIssue = await github.Issue.Create(repoOwner, repoName, createIssue);

            var updateIssue = newIssue.ToUpdate();
            updateIssue.AddAssignee("DJKoston");
            
            var updatedIssue = await github.Issue.Update(repoOwner, repoName, newIssue.Number, updateIssue);

            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Black,
                Title = $"New GitHub Issue Created: Issue #{newIssue.Number}",
                Description = $"Issue Details:\nUsername: {ctx.User.Username}\nServer: {ctx.Guild.Name}\nIssue: {issue}\n\nYou can check the issue here: {newIssue.HtmlUrl}\n\nIssue Created: <t:{secondsSinceEpoch}:R>",
                ImageUrl = $"https://opengraph.githubassets.com/54d9842c908322010edb97b469a055962982a290ebc87105c1e4d21d39509186/{repoOwner}/{repoName}/issues/{newIssue.Number}"
            }.WithAuthor("GitHub", iconUrl: "https://cdn-icons-png.flaticon.com/512/25/25231.png").WithUrl(newIssue.HtmlUrl);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
