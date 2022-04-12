namespace DiscordBot.Bots
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var SqlServerConnectionString = Configuration["connectionstring"];

            services.AddDbContext<RPGContext>(options =>
            {
                options.UseSqlServer(SqlServerConnectionString,
                   x => x.MigrationsAssembly("DiscordBot.DAL.Migrations"));
            });
            //Services

            //Community Streamer Services
            services.AddScoped<ICommunityStreamerService, CommunityStreamerService>();

            //Configuration Services
            services.AddScoped<IButtonRoleService, ButtonRoleService>();
            services.AddScoped<ICurrencyNameConfigService, CurrencyNameConfigService>();
            services.AddScoped<IDoubleXPRoleConfigService, DoubleXPRoleConfigService>();
            services.AddScoped<INowLiveRoleConfigService, NowLiveRoleConfigService>();
            services.AddScoped<ILeaveMessageConfigService, LeaveMessageConfigService>();
            services.AddScoped<IWelcomeMessageConfigService, WelcomeMessageConfigService>();
            services.AddScoped<IXPToggleService, XPToggleService>();

            //Counter Services
            services.AddScoped<IGoodBotBadBotService, GoodBotBadBotService>();

            //Custom Command Services
            services.AddScoped<ICustomCommandService, CustomCommandService>();

            //Egg Services
            services.AddScoped<IEggService, EggService>();

            //Game Services
            services.AddScoped<INumberGuessService, NumberGuessService>();

            //Now Live Services
            services.AddScoped<INowLiveMessageService, NowLiveMessageService>();
            services.AddScoped<INowLiveStreamerService, NowLiveStreamerService>();

            //Profile Services
            services.AddScoped<IGoldService, GoldService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IXPService, XPService>();

            //Quote Services
            services.AddScoped<IQuoteService, QuoteService>();
            services.AddScoped<ISimpsonsQuoteService, SimpsonsQuoteService>();

            //Reaction Role Services
            services.AddScoped<IReactionRoleService, ReactionRoleService>();

            //Suggestion Services
            services.AddScoped<ISuggestionService, SuggestionService>();

            services.AddRazorPages();

            var serviceProvider = services.BuildServiceProvider();

            var bot = new Bot(serviceProvider, Configuration);
            services.AddSingleton(bot);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}