using System;

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

        public FundingRateData()
        {
            Symbol = string.Empty;
            Timestamp = DateTime.Now;
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
}