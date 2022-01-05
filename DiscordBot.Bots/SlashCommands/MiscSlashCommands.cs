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


    }
}
