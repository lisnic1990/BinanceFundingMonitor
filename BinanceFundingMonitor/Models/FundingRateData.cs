using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceFundingMonitor.Models
{
    /// <summary>
    /// Модель данных для ставки финансирования
    /// </summary>
    public class FundingRateData
    {
        public string Symbol { get; set; }
        public decimal FundingRate { get; set; }
        public DateTime NextFundingTime { get; set; }
        public decimal MarkPrice { get; set; }
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// История цен для отслеживания изменений
        /// </summary>
        private List<PricePoint> _priceHistory;

        public FundingRateData()
        {
            Symbol = string.Empty;
            Timestamp = DateTime.Now;
            _priceHistory = new List<PricePoint>();
        }

        /// <summary>
        /// Добавление точки цены в историю
        /// </summary>
        public void AddPricePoint(decimal price, DateTime timestamp)
        {
            _priceHistory.Add(new PricePoint { Price = price, Timestamp = timestamp });

            // Очистка старых данных (храним максимум 30 минут)
            var cutoffTime = timestamp.AddMinutes(-30);
            _priceHistory.RemoveAll(p => p.Timestamp < cutoffTime);
        }

        /// <summary>
        /// Получение изменения цены за указанный период
        /// </summary>
        public PriceChange GetPriceChange(int minutes)
        {
            if (_priceHistory.Count < 2)
                return new PriceChange { ChangePercent = 0, ChangeAbsolute = 0, HasData = false };

            var cutoffTime = DateTime.Now.AddMinutes(-minutes);
            var historicalPoint = _priceHistory
                .Where(p => p.Timestamp <= cutoffTime)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefault();

            if (historicalPoint == null)
            {
                // Если нет данных за указанный период, берем самую старую точку
                historicalPoint = _priceHistory.OrderBy(p => p.Timestamp).First();
            }

            var oldPrice = historicalPoint.Price;
            var newPrice = MarkPrice;

            if (oldPrice == 0)
                return new PriceChange { ChangePercent = 0, ChangeAbsolute = 0, HasData = false };

            var changeAbsolute = newPrice - oldPrice;
            var changePercent = (changeAbsolute / oldPrice) * 100;

            return new PriceChange
            {
                ChangePercent = changePercent,
                ChangeAbsolute = changeAbsolute,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                HasData = true
            };
        }

        /// <summary>
        /// Получение секунд до следующего funding
        /// </summary>
        public int GetSecondsUntilFunding()
        {
            var timeSpan = NextFundingTime - DateTime.UtcNow;
            return (int)timeSpan.TotalSeconds;
        }

        public string GetFundingRatePercentage()
        {
            return $"{(FundingRate * 100):F4}%";
        }

        public string GetAnnualizedRate()
        {
            decimal annualized = FundingRate * 3 * 365;
            return $"{(annualized * 100):F2}%";
        }
    }

    /// <summary>
    /// Точка цены с временной меткой
    /// </summary>
    public class PricePoint
    {
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Результат изменения цены
    /// </summary>
    public class PriceChange
    {
        public decimal ChangePercent { get; set; }
        public decimal ChangeAbsolute { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public bool HasData { get; set; }
    }
}