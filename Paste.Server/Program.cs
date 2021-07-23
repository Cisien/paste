using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Threading.Tasks;


namespace Paste.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureLogging((context, logging) =>
                {
                    logging.AddSimpleConsole(o =>
                    {
                        o.SingleLine = context.HostingEnvironment.IsProduction();
                        o.ColorBehavior = context.HostingEnvironment.IsProduction() ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled;
                        o.UseUtcTimestamp = context.HostingEnvironment.IsProduction();
                        o.TimestampFormat = "yyyy-MM-ddTHH:mm:sszzz ";
                    });

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddDebug();
                    }
                })
                .UseStartup<Startup>();
            });

            return host;
        }
    }
}

