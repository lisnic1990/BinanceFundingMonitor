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
        private SoundPlayer? _fundingSoundPlayer;
        private string? _currentSoundPath;
        private string? _currentFundingSoundPath;

        public SoundService()
        {
            _soundPlayer = null;
            _fundingSoundPlayer = null;
            _currentSoundPath = null;
            _currentFundingSoundPath = null;
        }

        /// <summary>
        /// Установка пользовательского звукового файла для обновлений
        /// </summary>
        public bool SetCustomSound(string soundFilePath)
        {
            return SetSound(ref _soundPlayer, ref _currentSoundPath, soundFilePath);
        }

        /// <summary>
        /// Установка пользовательского звукового файла для funding уведомлений
        /// </summary>
        public bool SetFundingSound(string soundFilePath)
        {
            return SetSound(ref _fundingSoundPlayer, ref _currentFundingSoundPath, soundFilePath);
        }

        /// <summary>
        /// Общий метод установки звука
        /// </summary>
        private bool SetSound(ref SoundPlayer? player, ref string? path, string soundFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(soundFilePath))
                {
                    // Очистка пользовательского звука
                    player?.Dispose();
                    player = null;
                    path = null;
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
                player?.Dispose();
                player = new SoundPlayer(soundFilePath);

                // Предзагрузка звука для быстрого воспроизведения
                player.Load();

                path = soundFilePath;
                System.Diagnostics.Debug.WriteLine($"Установлен пользовательский звук: {soundFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка установки звука: {ex.Message}");
                player?.Dispose();
                player = null;
                path = null;
                return false;
            }
        }

        /// <summary>
        /// Получение пути к текущему звуковому файлу обновлений
        /// </summary>
        public string? GetCurrentSoundPath()
        {
            return _currentSoundPath;
        }

        /// <summary>
        /// Получение пути к текущему funding звуковому файлу
        /// </summary>
        public string? GetFundingSoundPath()
        {
            return _currentFundingSoundPath;
        }

        /// <summary>
        /// Проверка установлен ли пользовательский звук обновлений
        /// </summary>
        public bool HasCustomSound()
        {
            return _soundPlayer != null && !string.IsNullOrEmpty(_currentSoundPath);
        }

        /// <summary>
        /// Проверка установлен ли funding звук
        /// </summary>
        public bool HasFundingSound()
        {
            return _fundingSoundPlayer != null && !string.IsNullOrEmpty(_currentFundingSoundPath);
        }

        /// <summary>
        /// Воспроизведение звука при обновлении данных
        /// </summary>
        public void PlayUpdateSound()
        {
            PlaySound(_soundPlayer, 800, 100);
        }

        /// <summary>
        /// Воспроизведение звука funding уведомления
        /// </summary>
        public void PlayFundingSound()
        {
            PlaySound(_fundingSoundPlayer, 1200, 300);
        }

        /// <summary>
        /// Общий метод воспроизведения звука
        /// </summary>
        private void PlaySound(SoundPlayer? player, int beepFrequency, int beepDuration)
        {
            try
            {
                if (player != null)
                {
                    // Воспроизведение пользовательского звука
                    player.Play();
                }
                else
                {
                    // Воспроизведение системного beep
                    System.Console.Beep(beepFrequency, beepDuration);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения звука: {ex.Message}");

                // Fallback на системный beep при ошибке
                try
                {
                    System.Console.Beep(beepFrequency, beepDuration);
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

            _fundingSoundPlayer?.Dispose();
            _fundingSoundPlayer = null;
            _currentFundingSoundPath = null;
        }
    }
}