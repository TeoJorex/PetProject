using ServerStorage.Domain.Interfaces;
using ServerStorage.Startup;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var messageConsumerService = host.Services.GetRequiredService<IMessageConsumerService>();
        await messageConsumerService.StartConsumingAsync();

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var startup = new Startup(hostContext.Configuration);
                startup.ConfigureServices(services);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
}