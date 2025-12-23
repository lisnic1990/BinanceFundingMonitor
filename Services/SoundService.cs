using System.Media;

namespace BinanceFundingMonitor.Services
{
    /// <summary>
    /// Сервис для воспроизведения звуковых сигналов
    /// </summary>
    public class SoundService
    {
        private readonly SoundPlayer _soundPlayer;

        public SoundService()
        {
            // Используем системный звук Beep
            _soundPlayer = new SoundPlayer();
        }

        /// <summary>
        /// Воспроизведение звука при обновлении данных
        /// </summary>
        public void PlayUpdateSound()
        {
            try
            {
                // Воспроизведение системного звукового сигнала
                // System.Console.Beep(800, 100); // Частота 800 Hz, длительность 100 ms

                SystemSounds.Asterisk.Play();
            }
            catch
            {
                // Игнорируем ошибки воспроизведения
            }
        }

        /// <summary>
        /// Воспроизведение звука предупреждения
        /// </summary>
        public void PlayWarningSound()
        {
            try
            {
                SystemSounds.Exclamation.Play();
            }
            catch
            {
                // Игнорируем ошибки воспроизведения
            }
        }

        /// <summary>
        /// Воспроизведение звука ошибки
        /// </summary>
        public void PlayErrorSound()
        {
            try
            {
                SystemSounds.Hand.Play();
            }
            catch
            {
                // Игнорируем ошибки воспроизведения
            }
        }
    }
}