using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using VehicleEmulator.Domain.Interfaces;

namespace VehicleEmulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleEventController : ControllerBase
    {
        private readonly IVehicleEventService _vehicleEventService;
        private readonly ILogger<VehicleEventController> _logger;

        public VehicleEventController(
            IVehicleEventService vehicleEventService,
            ILogger<VehicleEventController> logger)
        {
            _vehicleEventService = vehicleEventService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.LogInformation("WebSocket-соединение установлено с ID: {Id}", id);

                await HandleWebSocketConnectionAsync(webSocket, id);

                return new EmptyResult();
            }
            else
            {
                return BadRequest("Это не WebSocket-запрос.");
            }
        }

        private async Task HandleWebSocketConnectionAsync(WebSocket webSocket, string terminalId)
        {
            _logger.LogInformation("Начало обработки WebSocket-соединения для терминала: {TerminalId}", terminalId);

            var terminalIdBytes = Encoding.UTF8.GetBytes(terminalId);
            await webSocket.SendAsync(
                new ArraySegment<byte>(terminalIdBytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            _logger.LogInformation("ID терминала {TerminalId} отправлен.", terminalId);

            var buffer = new byte[1024];

            var random = new Random();

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    int delay = random.Next(1000, 5000);
                    await Task.Delay(delay);

                    var vehicleEvent = _vehicleEventService.GenerateVehicleEvent();
                    var dataPacket = _vehicleEventService.SerializeVehicleEvent(vehicleEvent);

                    bool isAcknowledged = false;

                    while (!isAcknowledged && webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(dataPacket),
                            WebSocketMessageType.Binary,
                            true,
                            CancellationToken.None);

                        _logger.LogInformation("Пакет данных отправлен терминалу {TerminalId}.", terminalId);

                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        var response = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        if (response == "@")
                        {
                            _logger.LogInformation("Получено подтверждение от терминала {TerminalId}.", terminalId);
                            isAcknowledged = true;
                        }
                        else
                        {
                            _logger.LogWarning("Подтверждение не получено от терминала {TerminalId}. Повторная отправка...", terminalId);
                        }
                    }
                }

                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Завершение отправки данных",
                    CancellationToken.None);

                _logger.LogInformation("WebSocket-соединение закрыто для терминала {TerminalId}.", terminalId);
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "Ошибка WebSocket при обработке терминала {TerminalId}.", terminalId);
            }
        }
    }
}
