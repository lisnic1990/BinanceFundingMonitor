using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinanceFundingMonitor.Models;

namespace BinanceFundingMonitor.Services
{
    /// <summary>
    /// Монитор для отслеживания funding rate нескольких пар
    /// </summary>
    public class FundingRateMonitor : IDisposable
    {
        private readonly Dictionary<string, BinanceWebSocketClient> _clients;
        private readonly Action<FundingRateData> _onDataReceived;
        private readonly Action<string, bool> _onConnectionStatusChanged;

        public FundingRateMonitor(
            Action<FundingRateData> onDataReceived,
            Action<string, bool> onConnectionStatusChanged)
        {
            _clients = new Dictionary<string, BinanceWebSocketClient>();
            _onDataReceived = onDataReceived;
            _onConnectionStatusChanged = onConnectionStatusChanged;
        }

        /// <summary>
        /// Добавление символа для мониторинга
        /// </summary>
        public async Task AddSymbolAsync(string symbol)
        {
            symbol = symbol.ToUpper();

            if (_clients.ContainsKey(symbol))
                return;

            var client = new BinanceWebSocketClient(symbol);
            client.OnFundingRateUpdate += _onDataReceived;
            client.OnConnectionStatusChanged += (isConnected) =>
                _onConnectionStatusChanged?.Invoke(symbol, isConnected);

            _clients[symbol] = client;
            await client.ConnectAsync();
        }

        /// <summary>
        /// Удаление символа из мониторинга
        /// </summary>
        public async Task RemoveSymbolAsync(string symbol)
        {
            symbol = symbol.ToUpper();

            if (_clients.TryGetValue(symbol, out var client))
            {
                await client.DisconnectAsync();
                client.Dispose();
                _clients.Remove(symbol);
            }
        }

        /// <summary>
        /// Проверка отслеживается ли символ
        /// </summary>
        public bool IsSymbolMonitored(string symbol)
        {
            return _clients.ContainsKey(symbol.ToUpper());
        }

        /// <summary>
        /// Получение списка отслеживаемых символов
        /// </summary>
        public IEnumerable<string> GetMonitoredSymbols()
        {
            return _clients.Keys;
        }

        /// <summary>
        /// Получение количества отслеживаемых символов
        /// </summary>
        public int GetMonitoredSymbolsCount()
        {
            return _clients.Count;
        }

        /// <summary>
        /// Получение количества подключенных символов
        /// </summary>
        public int GetConnectedSymbolsCount()
        {
            return _clients.Values.Count(c => c.IsConnected);
        }

        public void Dispose()
        {
            foreach (var client in _clients.Values)
            {
                client.DisconnectAsync().Wait();
                client.Dispose();
            }
            _clients.Clear();
        }
    }
}