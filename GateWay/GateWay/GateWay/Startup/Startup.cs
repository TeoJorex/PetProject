using Gateway.BLL;
using Gateway.Domain.Interfaces;

namespace Gateway.Startup
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddSingleton<IVehicleEventService, VehicleEventService>();

            var rabbitMqHost = _configuration["RabbitMQ:HostName"];
            var queueName = _configuration["RabbitMQ:QueueName"];
            services.AddSingleton<IMessageQueueService>(new MessageQueueService(rabbitMqHost, queueName));

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/ws", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
                        await handler.HandleWebSocketAsync(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                });
            });
        }
    }
}
