using System.Collections.Generic;

namespace BinanceFundingMonitor.Models
{
    /// <summary>
    /// Модель настроек приложения
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Список отслеживаемых символов
        /// </summary>
        public List<string> WatchedSymbols { get; set; }

        /// <summary>
        /// Режим "Поверх всех окон"
        /// </summary>
        public bool AlwaysOnTop { get; set; }

        /// <summary>
        /// Воспроизводить звук при обновлении
        /// </summary>
        public bool PlaySoundOnUpdate { get; set; }

        /// <summary>
        /// Путь к пользовательскому звуковому файлу (WAV)
        /// </summary>
        public string? CustomSoundPath { get; set; }

        /// <summary>
        /// Путь к звуковому файлу для funding уведомлений
        /// </summary>
        public string? FundingSoundPath { get; set; }

        /// <summary>
        /// Включить звук за 5 секунд до funding
        /// </summary>
        public bool PlayFundingAlert { get; set; }

        /// <summary>
        /// Период отслеживания изменения цены (в минутах)
        /// </summary>
        public int PriceChangePeriod { get; set; }

        /// <summary>
        /// Порог для определения резкого импульса (в процентах)
        /// </summary>
        public decimal PriceChangeThreshold { get; set; }

        public AppSettings()
        {
            WatchedSymbols = new List<string>();
            AlwaysOnTop = false;
            PlaySoundOnUpdate = false;
            CustomSoundPath = null;
            FundingSoundPath = null;
            PlayFundingAlert = true;
            PriceChangePeriod = 5; // 5 минут по умолчанию
            PriceChangeThreshold = 1.0m; // 1% по умолчанию
        }
    }
}