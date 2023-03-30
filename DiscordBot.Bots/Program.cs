namespace DiscordBot.Bots
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            CreateHostBuilder(args, envName).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string envName) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    if (envName == "Production")
                    {
                        webBuilder.UseUrls("http://*:7001");
                    }
                    else if(envName == "Beta")
                    {
                        webBuilder.UseUrls("http://*:7003");
                    }
                    else
                    {
                        webBuilder.UseUrls(("http://*:7000"));
                    }
                });
    }
}