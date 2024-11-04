using Gateway.Domain.Interfaces;
using Gateway.Domain;
using System.Text.Json;
using RabbitMQ.Client;

namespace Gateway.BLL
{
    public class MessageQueueService : IMessageQueueService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageQueueService(string hostname, string queueName)
        {
            var factory = new ConnectionFactory() { HostName = hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            QueueName = queueName;
        }

        public string QueueName { get; }

        public Task SendMessageAsync(VehicleEventModel vehicleEvent)
        {
            var message = JsonSerializer.Serialize(vehicleEvent);
            var body = System.Text.Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                 routingKey: QueueName,
                                 basicProperties: null,
                                 body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
