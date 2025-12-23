using System;
using System.IO;
using System.Text.Json;
using BinanceFundingMonitor.Models;

namespace BinanceFundingMonitor.Services
{
    /// <summary>
    /// Сервис для сохранения и загрузки настроек приложения
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "BinanceFundingMonitor");

            // Создание папки если не существует
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _settingsFilePath = Path.Combine(appFolder, "settings.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Загрузка настроек из файла
        /// </summary>
        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Файл настроек не найден, создаем новый");
                    return new AppSettings();
                }

                var json = File.ReadAllText(_settingsFilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    System.Diagnostics.Debug.WriteLine("Файл настроек пустой");
                    return new AppSettings();
                }

                var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

                if (settings == null)
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось десериализовать настройки");
                    return new AppSettings();
                }

                System.Diagnostics.Debug.WriteLine($"Настройки загружены: {settings.WatchedSymbols?.Count ?? 0} символов");
                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
                return new AppSettings();
            }
        }

        /// <summary>
        /// Сохранение настроек в файл
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            try
            {
                // Убеждаемся что папка существует
                var directory = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(settings, _jsonOptions);

                // Сначала записываем во временный файл
                var tempFile = _settingsFilePath + ".tmp";
                File.WriteAllText(tempFile, json);

                // Затем перемещаем его на место основного файла
                if (File.Exists(_settingsFilePath))
                {
                    File.Delete(_settingsFilePath);
                }
                File.Move(tempFile, _settingsFilePath);

                System.Diagnostics.Debug.WriteLine($"Настройки успешно сохранены в: {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение пути к файлу настроек
        /// </summary>
        public string GetSettingsFilePath()
        {
            return _settingsFilePath;
        }
    }
}