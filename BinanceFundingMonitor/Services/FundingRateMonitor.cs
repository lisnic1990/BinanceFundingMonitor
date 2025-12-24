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
        private readonly Dictionary<string, FundingRateData> _dataCache; // Кэш данных с историей
        private readonly Action<FundingRateData> _onDataReceived;
        private readonly Action<string, bool> _onConnectionStatusChanged;

        public FundingRateMonitor(
            Action<FundingRateData> onDataReceived,
            Action<string, bool> onConnectionStatusChanged)
        {
            _clients = new Dictionary<string, BinanceWebSocketClient>();
            _dataCache = new Dictionary<string, FundingRateData>(); // Инициализация кэша
            _onDataReceived = onDataReceived;
            _onConnectionStatusChanged = onConnectionStatusChanged;
        }

        /// <summary>
        /// Обработка полученных данных и обновление кэша
        /// </summary>
        private void OnDataReceived(FundingRateData newData)
        {
            // Получаем или создаем закэшированный объект
            if (!_dataCache.TryGetValue(newData.Symbol, out var cachedData))
            {
                // Первое получение данных для этого символа - создаем новый объект
                cachedData = newData;
                _dataCache[newData.Symbol] = cachedData;
            }
            else
            {
                // Обновляем существующий объект, сохраняя историю цен
                cachedData.FundingRate = newData.FundingRate;
                cachedData.NextFundingTime = newData.NextFundingTime;
                cachedData.MarkPrice = newData.MarkPrice;
                cachedData.Timestamp = newData.Timestamp;
            }

            // Добавляем текущую цену в историю
            cachedData.AddPricePoint(cachedData.MarkPrice, cachedData.Timestamp);

            // Передаем обновленный объект с историей
            _onDataReceived?.Invoke(cachedData);
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
            client.OnFundingRateUpdate += OnDataReceived; // Используем локальный обработчик
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
                _dataCache.Remove(symbol); // Удаляем из кэша
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

        /// <summary>
        /// Получение закэшированных данных для символа (с историей)
        /// </summary>
        public FundingRateData? GetCachedData(string symbol)
        {
            _dataCache.TryGetValue(symbol.ToUpper(), out var data);
            return data;
        }

        public void Dispose()
        {
            foreach (var client in _clients.Values)
            {
                client.DisconnectAsync().Wait();
                client.Dispose();
            }
            _clients.Clear();
            _dataCache.Clear(); // Очищаем кэш
        }
    }
}