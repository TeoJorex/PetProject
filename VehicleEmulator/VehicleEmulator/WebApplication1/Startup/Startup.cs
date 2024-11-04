using VehicleEmulator.BLL;
using VehicleEmulator.Domain.Interfaces;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) => _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        services.AddSingleton<IVehicleEventService, VehicleEventService>();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}