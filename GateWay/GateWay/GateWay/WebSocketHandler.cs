using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace Gateway
{
    public class WebSocketHandler
    {
        private readonly IVehicleEventService _vehicleEventService;
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<WebSocketHandler> _logger;

        public WebSocketHandler(
            IVehicleEventService vehicleEventService,
            IMessageQueueService messageQueueService,
            ILogger<WebSocketHandler> logger)
        {
            _vehicleEventService = vehicleEventService;
            _messageQueueService = messageQueueService;
            _logger = logger;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024];

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var terminalId = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation("Подключен терминал: {TerminalId}", terminalId);

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("Соединение закрыто терминалом: {TerminalId}", terminalId);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения", CancellationToken.None);
                    }
                    else
                    {
                        _logger.LogInformation("Получено {Count} байт данных от терминала {TerminalId}.", result.Count, terminalId);

                        var data = new byte[result.Count];
                        Array.Copy(buffer, data, result.Count);

                        var vehicleEvent = _vehicleEventService.DeserializeVehicleEvent(data);

                        await _messageQueueService.SendMessageAsync(vehicleEvent);
                        _logger.LogInformation("Данные отправлены в очередь RabbitMQ.");

                        var ack = Encoding.UTF8.GetBytes("@");
                        await webSocket.SendAsync(new ArraySegment<byte>(ack), WebSocketMessageType.Text, true, CancellationToken.None);
                        _logger.LogInformation("Подтверждение отправлено терминалу {TerminalId}.", terminalId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке данных от терминала {TerminalId}.", terminalId);
                    break;
                }
            }
        }
    }
}
