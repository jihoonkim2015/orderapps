using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace pos.wpf.worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("workersettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IMongoDbContext, MongoDbContext>();
                })
                .UseWindowsService();
    }
}
