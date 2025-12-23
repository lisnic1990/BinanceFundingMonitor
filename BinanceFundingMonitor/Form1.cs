using BinanceFundingMonitor.Models;
using BinanceFundingMonitor.Services;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BinanceFundingMonitor
{
    public partial class Form1 : Form
    {
        private FundingRateMonitor? _monitor;
        private SettingsService _settingsService;
        private SoundService _soundService;
        private AppSettings _settings;

        public Form1()
        {
            InitializeComponent();

            // Инициализация сервисов в конструкторе
            _settingsService = new SettingsService();
            _soundService = new SoundService();
            _settings = new AppSettings();

            InitializeServices();
            LoadSettings();
            SetupDataGridView();
            this.FormClosing += Form1_FormClosing;
        }

        /// <summary>
        /// Инициализация сервисов
        /// </summary>
        private void InitializeServices()
        {
            _monitor = new FundingRateMonitor(OnDataReceived, OnConnectionStatusChanged);
        }

        /// <summary>
        /// Загрузка сохраненных настроек
        /// </summary>
        private void LoadSettings()
        {
            _settings = _settingsService.LoadSettings();

            // Загрузка сохраненных тикеров
            if (_settings.WatchedSymbols != null && _settings.WatchedSymbols.Count > 0)
            {
                foreach (var symbol in _settings.WatchedSymbols)
                {
                    AddSymbolToMonitoring(symbol);
                }
            }

            // Применение настроек
            chkTopMost.Checked = _settings.AlwaysOnTop;
            chkPlaySound.Checked = _settings.PlaySoundOnUpdate;
            this.TopMost = _settings.AlwaysOnTop;

            // Загрузка пользовательского звука
            if (!string.IsNullOrEmpty(_settings.CustomSoundPath))
            {
                if (_soundService.SetCustomSound(_settings.CustomSoundPath))
                {
                    UpdateSoundButtonText();
                }
                else
                {
                    // Если файл не найден, очищаем настройку
                    _settings.CustomSoundPath = null;
                    _settingsService.SaveSettings(_settings);
                }
            }
        }

        /// <summary>
        /// Настройка DataGridView
        /// </summary>
        private void SetupDataGridView()
        {
            dgvFundingRates.AutoGenerateColumns = false;
            dgvFundingRates.AllowUserToAddRows = false;
            dgvFundingRates.AllowUserToDeleteRows = false;
            dgvFundingRates.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFundingRates.MultiSelect = false;
            dgvFundingRates.ReadOnly = true;

            // Колонка: Символ
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Symbol",
                HeaderText = "Символ",
                DataPropertyName = "Symbol",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.DarkBlue
                }
            });

            // Колонка: Funding Rate
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FundingRate",
                HeaderText = "Funding Rate",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // Колонка: Годовая ставка
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AnnualRate",
                HeaderText = "Годовая ставка",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // Колонка: Цена
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MarkPrice",
                HeaderText = "Цена",
                DataPropertyName = "MarkPrice",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    ForeColor = Color.DarkGreen,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // Колонка: Следующий funding
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NextFunding",
                HeaderText = "След. funding через",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.DarkSlateGray
                }
            });

            // Колонка: Время обновления
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UpdateTime",
                HeaderText = "Обновлено",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "HH:mm:ss",
                    ForeColor = Color.Gray
                }
            });
        }

        /// <summary>
        /// Обработчик получения данных от монитора
        /// </summary>
        private void OnDataReceived(FundingRateData data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnDataReceived(data)));
                return;
            }

            UpdateDataGridView(data);

            // Воспроизведение звука если включено
            if (chkPlaySound.Checked)
            {
                _soundService.PlayUpdateSound();
            }
        }

        /// <summary>
        /// Обновление данных в таблице
        /// </summary>
        private void UpdateDataGridView(FundingRateData data)
        {
            var existingRow = dgvFundingRates.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(r => r.Cells["Symbol"].Value?.ToString() == data.Symbol);

            var timeUntilFunding = data.NextFundingTime - DateTime.UtcNow;
            var nextFundingText = $"{timeUntilFunding.Hours:D2}:{timeUntilFunding.Minutes:D2}:{timeUntilFunding.Seconds:D2}";

            var fundingColor = GetFundingRateColor(data.FundingRate);

            if (existingRow != null)
            {
                // Обновление существующей строки
                existingRow.Cells["FundingRate"].Value = data.GetFundingRatePercentage();
                existingRow.Cells["FundingRate"].Style.ForeColor = fundingColor;
                existingRow.Cells["AnnualRate"].Value = data.GetAnnualizedRate();
                existingRow.Cells["AnnualRate"].Style.ForeColor = fundingColor;
                existingRow.Cells["MarkPrice"].Value = data.MarkPrice;
                existingRow.Cells["NextFunding"].Value = nextFundingText;
                existingRow.Cells["UpdateTime"].Value = data.Timestamp;
            }
            else
            {
                // Добавление новой строки
                var rowIndex = dgvFundingRates.Rows.Add();
                var row = dgvFundingRates.Rows[rowIndex];

                row.Cells["Symbol"].Value = data.Symbol;
                row.Cells["FundingRate"].Value = data.GetFundingRatePercentage();
                row.Cells["FundingRate"].Style.ForeColor = fundingColor;
                row.Cells["AnnualRate"].Value = data.GetAnnualizedRate();
                row.Cells["AnnualRate"].Style.ForeColor = fundingColor;
                row.Cells["MarkPrice"].Value = data.MarkPrice;
                row.Cells["NextFunding"].Value = nextFundingText;
                row.Cells["UpdateTime"].Value = data.Timestamp;
            }
        }

        /// <summary>
        /// Определение цвета для funding rate
        /// </summary>
        private Color GetFundingRateColor(decimal fundingRate)
        {
            if (fundingRate > 0.0001m) return Color.Red;
            if (fundingRate > 0) return Color.DarkRed;
            if (fundingRate < -0.0001m) return Color.Green;
            if (fundingRate < 0) return Color.DarkGreen;
            return Color.Black;
        }

        /// <summary>
        /// Обработчик изменения статуса подключения
        /// </summary>
        private void OnConnectionStatusChanged(string symbol, bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnConnectionStatusChanged(symbol, isConnected)));
                return;
            }

            UpdateStatusLabel();
        }

        /// <summary>
        /// Обновление статусной строки
        /// </summary>
        private void UpdateStatusLabel()
        {
            if (_monitor == null) return;

            var totalSymbols = _monitor.GetMonitoredSymbolsCount();
            var connectedSymbols = _monitor.GetConnectedSymbolsCount();

            toolStripStatusLabel.Text = $"Подключено: {connectedSymbols} из {totalSymbols} | " +
                                       $"Последнее обновление: {DateTime.Now:HH:mm:ss}";

            if (connectedSymbols == totalSymbols && totalSymbols > 0)
            {
                toolStripStatusLabel.ForeColor = Color.Green;
            }
            else if (connectedSymbols > 0)
            {
                toolStripStatusLabel.ForeColor = Color.Orange;
            }
            else
            {
                toolStripStatusLabel.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Добавление символа для мониторинга
        /// </summary>
        private async void AddSymbolToMonitoring(string symbol)
        {
            if (_monitor == null) return;

            symbol = symbol.ToUpper().Trim();

            if (string.IsNullOrWhiteSpace(symbol))
            {
                MessageBox.Show("Введите символ криптовалюты!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка что символ еще не добавлен
            if (_monitor.IsSymbolMonitored(symbol))
            {
                MessageBox.Show($"Символ {symbol} уже отслеживается!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Добавление суффикса USDT если его нет
            if (!symbol.EndsWith("USDT"))
            {
                symbol += "USDT";
            }

            try
            {
                await _monitor.AddSymbolAsync(symbol);
                txtSymbol.Clear();
                // SaveSettings();
                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления символа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Удаление выбранного символа
        /// </summary>
        private async void RemoveSelectedSymbol()
        {
            if (_monitor == null) return;

            if (dgvFundingRates.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите символ для удаления!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dgvFundingRates.SelectedRows[0];
            var symbol = selectedRow.Cells["Symbol"].Value?.ToString();

            if (string.IsNullOrEmpty(symbol))
                return;

            var result = MessageBox.Show(
                $"Вы уверены что хотите удалить {symbol} из отслеживания?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                await _monitor.RemoveSymbolAsync(symbol);
                dgvFundingRates.Rows.Remove(selectedRow);
                // SaveSettings();
                UpdateStatusLabel();
            }
        }

        /// <summary>
        /// Сохранение текущих настроек
        /// </summary>
        private void SaveSettings()
        {
            if (_monitor == null) return;

            try
            {
                _settings.WatchedSymbols = _monitor.GetMonitoredSymbols().ToList();
                _settings.AlwaysOnTop = chkTopMost.Checked;
                _settings.PlaySoundOnUpdate = chkPlaySound.Checked;
                _settings.CustomSoundPath = _soundService.GetCurrentSoundPath();
                _settingsService.SaveSettings(_settings);

                // Логирование для отладки
                System.Diagnostics.Debug.WriteLine($"Настройки сохранены: {_settings.WatchedSymbols.Count} символов");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Выбор звукового файла
        /// </summary>
        private void SelectSoundFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите звуковой файл";
                openFileDialog.Filter = "WAV файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                // Если уже выбран файл, показываем его папку
                var currentPath = _soundService.GetCurrentSoundPath();
                if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(currentPath);
                    openFileDialog.FileName = Path.GetFileName(currentPath);
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_soundService.SetCustomSound(openFileDialog.FileName))
                    {
                        MessageBox.Show(
                            $"Звуковой файл успешно установлен:\n{Path.GetFileName(openFileDialog.FileName)}",
                            "Успешно",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        UpdateSoundButtonText();
                        // SaveSettings();

                        // Тестовое воспроизведение
                        _soundService.PlayUpdateSound();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Не удалось загрузить звуковой файл.\nПроверьте что это корректный WAV файл.",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Сброс звука на стандартный
        /// </summary>
        private void ResetSoundToDefault()
        {
            var result = MessageBox.Show(
                "Вернуть стандартный звук (системный beep)?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _soundService.SetCustomSound(null!);
                UpdateSoundButtonText();
                // SaveSettings();

                MessageBox.Show(
                    "Установлен стандартный звук",
                    "Готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Обновление текста кнопки звука
        /// </summary>
        private void UpdateSoundButtonText()
        {
            if (_soundService.HasCustomSound())
            {
                var soundPath = _soundService.GetCurrentSoundPath();
                var fileName = Path.GetFileName(soundPath);
                btnSelectSound.Text = $"🎵 {fileName}";
                toolTip.SetToolTip(btnSelectSound, $"Текущий звук: {fileName}\nНажмите для изменения\nПравая кнопка - сбросить");
            }
            else
            {
                btnSelectSound.Text = "🔊 Выбрать звук";
                toolTip.SetToolTip(btnSelectSound, "Выберите WAV файл для звука обновления");
            }
        }

        // === ОБРАБОТЧИКИ СОБЫТИЙ ===

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            AddSymbolToMonitoring(txtSymbol.Text);
        }

        private void btnRemove_Click(object? sender, EventArgs e)
        {
            RemoveSelectedSymbol();
        }

        private void txtSymbol_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Добавление по Enter
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                AddSymbolToMonitoring(txtSymbol.Text);
            }
        }

        private void chkTopMost_CheckedChanged(object? sender, EventArgs e)
        {
            this.TopMost = chkTopMost.Checked;
            // SaveSettings();
        }

        private void chkPlaySound_CheckedChanged(object? sender, EventArgs e)
        {
            // SaveSettings();
        }

        private void btnSelectSound_Click(object? sender, EventArgs e)
        {
            SelectSoundFile();
        }

        private void btnSelectSound_MouseDown(object? sender, MouseEventArgs e)
        {
            // Правая кнопка мыши - сброс на стандартный звук
            if (e.Button == MouseButtons.Right && _soundService.HasCustomSound())
            {
                ResetSoundToDefault();
            }
        }

        private async void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Отмена закрытия при первой попытке для корректного завершения
            if (_monitor != null && _monitor.GetMonitoredSymbolsCount() > 0)
            {
                e.Cancel = true;

                // Отключаем элементы управления
                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                txtSymbol.Enabled = false;
                chkTopMost.Enabled = false;
                chkPlaySound.Enabled = false;

                toolStripStatusLabel.Text = "Закрытие приложения...";

                // Сохраняем настройки перед закрытием
                SaveSettings();

                // Корректное закрытие всех соединений
                await Task.Run(async () =>
                {
                    if (_monitor != null)
                    {
                        var symbols = _monitor.GetMonitoredSymbols().ToList();
                        foreach (var symbol in symbols)
                        {
                            await _monitor.RemoveSymbolAsync(symbol);
                        }
                        _monitor.Dispose();
                        _monitor = null;
                    }
                });

                // Теперь можно закрыть форму
                this.FormClosing -= Form1_FormClosing;
                Application.Exit();
            }
        }
    }
}