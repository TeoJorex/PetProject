namespace Gateway.Domain.Interfaces
{
    public interface IMessageQueueService
    {
        Task SendMessageAsync(VehicleEventModel vehicleEvent);
    }
}
