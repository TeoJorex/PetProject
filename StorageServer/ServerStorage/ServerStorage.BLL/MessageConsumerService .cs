using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServerStorage.Domain.Interfaces;
using ServerStorage.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerStorage.BLL
{
    public class MessageConsumerService : IMessageConsumerService
    {
        private readonly IVehicleEventRepository _vehicleEventRepository;
        private readonly ILogger<MessageConsumerService> _logger;
        private readonly string _hostname;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public MessageConsumerService(
            IVehicleEventRepository vehicleEventRepository,
            ILogger<MessageConsumerService> logger,
            string hostname,
            string queueName)
        {
            _vehicleEventRepository = vehicleEventRepository;
            _logger = logger;
            _hostname = hostname;
            _queueName = queueName;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public Task StartConsumingAsync()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Получено сообщение из очереди.");

                try
                {
                    var vehicleEvent = JsonSerializer.Deserialize<VehicleEventModel>(message);

                    var record = new VehicleEventRecord
                    {
                        Id = Guid.NewGuid(),
                        RecordDate = DateTime.UtcNow,
                        JsonData = message
                    };

                    await _vehicleEventRepository.SaveVehicleEventAsync(record);

                    _logger.LogInformation("Сообщение сохранено в базе данных.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения.");
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
