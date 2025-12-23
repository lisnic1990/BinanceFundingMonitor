using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BinanceFundingMonitor.Models;

namespace BinanceFundingMonitor.Services
{
    /// <summary>
    /// Клиент для подключения к Binance WebSocket API
    /// </summary>
    public class BinanceWebSocketClient : IDisposable
    {
        private readonly string _symbol;
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private const string WS_BASE_URL = "wss://fstream.binance.com/ws/";
        private bool _isRunning;

        public event Action<FundingRateData>? OnFundingRateUpdate;
        public event Action<bool>? OnConnectionStatusChanged;

        public bool IsConnected => _webSocket?.State == WebSocketState.Open;

        public BinanceWebSocketClient(string symbol)
        {
            _symbol = symbol.ToLower();
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Запуск WebSocket подключения
        /// </summary>
        public async Task ConnectAsync()
        {
            try
            {
                _isRunning = true;
                var streamName = $"{_symbol}@markPrice";
                var url = $"{WS_BASE_URL}{streamName}";

                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                await _webSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);
                OnConnectionStatusChanged?.Invoke(true);

                _ = Task.Run(() => ListenAsync());
            }
            catch (Exception)
            {
                OnConnectionStatusChanged?.Invoke(false);
                await ReconnectAsync();
            }
        }

        /// <summary>
        /// Прослушивание входящих сообщений
        /// </summary>
        private async Task ListenAsync()
        {
            if (_webSocket == null || _cancellationTokenSource == null) return;

            var buffer = new byte[8192];

            while (_isRunning && _webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        OnConnectionStatusChanged?.Invoke(false);
                        await ReconnectAsync();
                        return;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    OnConnectionStatusChanged?.Invoke(false);
                    await ReconnectAsync();
                    break;
                }
            }
        }

        /// <summary>
        /// Обработка полученного сообщения
        /// </summary>
        private void ProcessMessage(string message)
        {
            try
            {
                using var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                if (root.TryGetProperty("e", out var eventType) &&
                    eventType.GetString() == "markPriceUpdate")
                {
                    var data = new FundingRateData
                    {
                        Symbol = root.GetProperty("s").GetString() ?? _symbol.ToUpper(),
                        FundingRate = decimal.Parse(root.GetProperty("r").GetString() ?? "0"),
                        MarkPrice = decimal.Parse(root.GetProperty("p").GetString() ?? "0"),
                        NextFundingTime = DateTimeOffset.FromUnixTimeMilliseconds(
                            root.GetProperty("T").GetInt64()
                        ).DateTime,
                        Timestamp = DateTime.Now
                    };

                    OnFundingRateUpdate?.Invoke(data);
                }
            }
            catch { }
        }

        /// <summary>
        /// Переподключение при потере связи
        /// </summary>
        private async Task ReconnectAsync()
        {
            if (!_isRunning) return;

            await Task.Delay(5000);

            try
            {
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                await ConnectAsync();
            }
            catch
            {
                await Task.Delay(10000);
                await ReconnectAsync();
            }
        }

        /// <summary>
        /// Остановка WebSocket подключения
        /// </summary>
        public async Task DisconnectAsync()
        {
            _isRunning = false;

            if (_webSocket?.State == WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None
                    );
                }
                catch
                {
                    // Игнорируем ошибки при закрытии
                }
            }

            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _webSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}