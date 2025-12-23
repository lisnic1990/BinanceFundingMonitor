using System;
using System.IO;
using System.Media;

namespace BinanceFundingMonitor.Services
{
    /// <summary>
    /// Сервис для воспроизведения звуковых сигналов
    /// </summary>
    public class SoundService
    {
        private SoundPlayer? _soundPlayer;
        private string? _currentSoundPath;

        public SoundService()
        {
            _soundPlayer = null;
            _currentSoundPath = null;
        }

        /// <summary>
        /// Установка пользовательского звукового файла
        /// </summary>
        public bool SetCustomSound(string soundFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(soundFilePath))
                {
                    // Очистка пользовательского звука
                    _soundPlayer?.Dispose();
                    _soundPlayer = null;
                    _currentSoundPath = null;
                    return true;
                }

                if (!File.Exists(soundFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Звуковой файл не найден: {soundFilePath}");
                    return false;
                }

                // Проверка что это WAV файл
                var extension = Path.GetExtension(soundFilePath).ToLower();
                if (extension != ".wav")
                {
                    System.Diagnostics.Debug.WriteLine($"Неподдерживаемый формат: {extension}. Только WAV файлы поддерживаются.");
                    return false;
                }

                // Создание нового SoundPlayer
                _soundPlayer?.Dispose();
                _soundPlayer = new SoundPlayer(soundFilePath);

                // Предзагрузка звука для быстрого воспроизведения
                _soundPlayer.Load();

                _currentSoundPath = soundFilePath;
                System.Diagnostics.Debug.WriteLine($"Установлен пользовательский звук: {soundFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка установки звука: {ex.Message}");
                _soundPlayer?.Dispose();
                _soundPlayer = null;
                _currentSoundPath = null;
                return false;
            }
        }

        /// <summary>
        /// Получение пути к текущему звуковому файлу
        /// </summary>
        public string? GetCurrentSoundPath()
        {
            return _currentSoundPath;
        }

        /// <summary>
        /// Проверка установлен ли пользовательский звук
        /// </summary>
        public bool HasCustomSound()
        {
            return _soundPlayer != null && !string.IsNullOrEmpty(_currentSoundPath);
        }

        /// <summary>
        /// Воспроизведение звука при обновлении данных
        /// </summary>
        public void PlayUpdateSound()
        {
            try
            {
                if (_soundPlayer != null)
                {
                    // Воспроизведение пользовательского звука
                    _soundPlayer.Play();
                }
                else
                {
                    // Воспроизведение системного beep
                    System.Console.Beep(800, 100); // Частота 800 Hz, длительность 100 ms
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения звука: {ex.Message}");

                // Fallback на системный beep при ошибке
                try
                {
                    System.Console.Beep(800, 100);
                }
                catch
                {
                    // Игнорируем ошибки beep
                }
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

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            _soundPlayer?.Dispose();
            _soundPlayer = null;
            _currentSoundPath = null;
        }
    }
}