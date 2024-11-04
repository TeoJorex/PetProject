namespace ServerStorage.Domain.Interfaces
{
    public interface IMessageConsumerService
    {
        Task StartConsumingAsync();
    }
}
