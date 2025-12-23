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

        public AppSettings()
        {
            WatchedSymbols = new List<string>();
            AlwaysOnTop = false;
            PlaySoundOnUpdate = false;
        }
    }
}