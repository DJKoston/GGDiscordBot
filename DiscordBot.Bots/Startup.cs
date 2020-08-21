using DiscordBot.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DiscordBot.Core.Services.CustomCommands;
using DiscordBot.Core.Services.Items;
using DiscordBot.Core.Services.Profiles;
using Microsoft.EntityFrameworkCore;
using DiscordBot.Core.Services.Quotes;

namespace DiscordBot.Bots
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RPGContext>(options =>
            {
                options.UseSqlServer("Data Source=SRV1;Initial Catalog=GGBotLive;Persist Security Info=True;User ID=sa;Password=RH14NN4k05t0n",
                   x => x.MigrationsAssembly("DiscordBot.DAL.Migrations"));
            });
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IExperienceService, ExperienceService>();
            services.AddScoped<IGoldService, GoldService>();
            services.AddScoped<ICustomCommandService, CustomCommandService>();
            services.AddScoped<IQuoteService, QuoteService>();
            services.AddRazorPages();

            var serviceProvider = services.BuildServiceProvider();

            var bot = new Bot(serviceProvider);
            services.AddSingleton(bot);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
