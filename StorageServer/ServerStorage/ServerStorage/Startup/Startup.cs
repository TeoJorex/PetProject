using Microsoft.Extensions.DependencyInjection;
using ServerStorage.BLL;
using ServerStorage.Domain.Interfaces;

namespace ServerStorage.Startup
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            services.AddSingleton<IVehicleEventRepository>(provider =>
                new VehicleEventRepository(connectionString));

            var rabbitMqHost = _configuration["RabbitMQ:HostName"];
            var queueName = _configuration["RabbitMQ:QueueName"];
            services.AddSingleton<IMessageConsumerService>(provider =>
              new MessageConsumerService(
                  provider.GetRequiredService<IVehicleEventRepository>(),
                  provider.GetRequiredService<ILogger<MessageConsumerService>>(),
                  rabbitMqHost,
                  queueName));
        }
    }
}
