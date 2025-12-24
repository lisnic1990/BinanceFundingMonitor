using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BinanceFundingMonitor.Models;
using BinanceFundingMonitor.Services;
using System.Collections.Generic;

namespace BinanceFundingMonitor
{
    public partial class Form1 : Form
    {
        // ============= ПОЛЯ КЛАССА =============
        private FundingRateMonitor? _monitor;
        private SettingsService _settingsService;
        private SoundService _soundService;
        private AppSettings _settings;
        private Dictionary<string, int> _lastFundingSeconds;
        private System.Windows.Forms.Timer _updateTimer;

        // ============= КОНСТРУКТОР =============
        public Form1()
        {
            InitializeComponent();

            // Инициализация сервисов в конструкторе
            _settingsService = new SettingsService();
            _soundService = new SoundService();
            _settings = new AppSettings();
            _lastFundingSeconds = new Dictionary<string, int>();

            // Инициализация таймера для обновления UI
            _updateTimer = new System.Windows.Forms.Timer();
            _updateTimer.Interval = 1000; // Обновление каждую секунду
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            InitializeServices();
            LoadSettings();
            SetupDataGridView();
            this.FormClosing += Form1_FormClosing;
        }

        // ============= ИНИЦИАЛИЗАЦИЯ =============
        private void InitializeServices()
        {
            _monitor = new FundingRateMonitor(OnDataReceived, OnConnectionStatusChanged);
        }

        private void LoadSettings()
        {
            _settings = _settingsService.LoadSettings();

            // Применение настроек
            chkTopMost.Checked = _settings.AlwaysOnTop;
            chkPlaySound.Checked = _settings.PlaySoundOnUpdate;
            chkFundingAlert.Checked = _settings.PlayFundingAlert;
            this.TopMost = _settings.AlwaysOnTop;

            // Загрузка пользовательского звука обновлений
            if (!string.IsNullOrEmpty(_settings.CustomSoundPath))
            {
                if (_soundService.SetCustomSound(_settings.CustomSoundPath))
                {
                    UpdateSoundButtonText();
                }
                else
                {
                    _settings.CustomSoundPath = null;
                }
            }

            // Загрузка funding звука
            if (!string.IsNullOrEmpty(_settings.FundingSoundPath))
            {
                if (_soundService.SetFundingSound(_settings.FundingSoundPath))
                {
                    UpdateFundingSoundButtonText();
                }
                else
                {
                    _settings.FundingSoundPath = null;
                }
            }

            // Загрузка сохраненных тикеров
            if (_settings.WatchedSymbols != null && _settings.WatchedSymbols.Count > 0)
            {
                foreach (var symbol in _settings.WatchedSymbols)
                {
                    AddSymbolToMonitoring(symbol);
                }
            }
        }

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
                Width = 100,
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
                Width = 110,
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
                HeaderText = "Годовая",
                Width = 90,
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
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    ForeColor = Color.DarkGreen,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // Колонка: Изменение цены
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PriceChange",
                HeaderText = $"Изм. за {_settings.PriceChangePeriod}м",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // Колонка: Следующий funding
            dgvFundingRates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NextFunding",
                HeaderText = "След. funding",
                Width = 120,
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
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "HH:mm:ss",
                    ForeColor = Color.Gray
                }
            });
        }

        // ============= ОБРАБОТКА ДАННЫХ =============
        private void OnDataReceived(FundingRateData data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnDataReceived(data)));
                return;
            }

            // История цен уже обновлена в FundingRateMonitor
            // Проверка funding момента
            CheckFundingMoment(data);

            UpdateDataGridView(data);

            // Воспроизведение звука если включено
            if (chkPlaySound.Checked)
            {
                _soundService.PlayUpdateSound();
            }
        }

        private void CheckFundingMoment(FundingRateData data)
        {
            if (!_settings.PlayFundingAlert)
                return;

            var currentSeconds = data.GetSecondsUntilFunding();

            // Получаем предыдущее значение секунд
            if (_lastFundingSeconds.TryGetValue(data.Symbol, out int lastSeconds))
            {
                // Проверка 1: Таймер <= 5 секунд (впервые)
                if (currentSeconds <= 5 && currentSeconds > 0 && lastSeconds > 5)
                {
                    System.Diagnostics.Debug.WriteLine($"⏰ {data.Symbol}: Funding через {currentSeconds} секунд!");
                    _soundService.PlayFundingSound();
                }

                // Проверка 2: Таймер резко увеличился (funding произошел)
                if (currentSeconds > 3600 && lastSeconds < 60)
                {
                    System.Diagnostics.Debug.WriteLine($"💰 {data.Symbol}: Funding произошел! Таймер сброшен.");
                    _soundService.PlayFundingSound();
                }
            }

            // Сохраняем текущее значение
            _lastFundingSeconds[data.Symbol] = currentSeconds;
        }

        private void UpdateDataGridView(FundingRateData data)
        {
            var existingRow = dgvFundingRates.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(r => r.Cells["Symbol"].Value?.ToString() == data.Symbol);

            var timeUntilFunding = data.NextFundingTime - DateTime.UtcNow;
            var seconds = (int)timeUntilFunding.TotalSeconds;
            if (seconds < 0) seconds = 0;

            var hours = seconds / 3600;
            var minutes = (seconds % 3600) / 60;
            var secs = seconds % 60;

            string nextFundingText;
            if (hours > 0)
                nextFundingText = $"{hours:D2}:{minutes:D2}:{secs:D2}";
            else
                nextFundingText = $"{minutes:D2}:{secs:D2}";

            var fundingColor = GetFundingRateColor(data.FundingRate);

            // Получаем изменение цены за период
            var priceChange = data.GetPriceChange(_settings.PriceChangePeriod);
            string priceChangeText = priceChange.HasData ?
                $"{(priceChange.ChangePercent >= 0 ? "+" : "")}{priceChange.ChangePercent:F2}%" :
                "—";
            var priceChangeColor = GetPriceChangeColor(priceChange);

            if (existingRow != null)
            {
                // Обновление существующей строки
                existingRow.Tag = data;
                existingRow.Cells["FundingRate"].Value = data.GetFundingRatePercentage();
                existingRow.Cells["FundingRate"].Style.ForeColor = fundingColor;
                existingRow.Cells["AnnualRate"].Value = data.GetAnnualizedRate();
                existingRow.Cells["AnnualRate"].Style.ForeColor = fundingColor;
                existingRow.Cells["MarkPrice"].Value = data.MarkPrice;
                existingRow.Cells["PriceChange"].Value = priceChangeText;
                existingRow.Cells["PriceChange"].Style.ForeColor = priceChangeColor;
                existingRow.Cells["NextFunding"].Value = nextFundingText;
                existingRow.Cells["UpdateTime"].Value = data.Timestamp;

                // Подсветка строки при резком изменении цены
                if (priceChange.HasData && Math.Abs(priceChange.ChangePercent) >= _settings.PriceChangeThreshold)
                {
                    existingRow.DefaultCellStyle.BackColor = priceChange.ChangePercent > 0
                        ? Color.FromArgb(230, 255, 230)
                        : Color.FromArgb(255, 230, 230);
                }
                else
                {
                    existingRow.DefaultCellStyle.BackColor = Color.White;
                }
            }
            else
            {
                // Добавление новой строки
                var rowIndex = dgvFundingRates.Rows.Add();
                var row = dgvFundingRates.Rows[rowIndex];

                row.Tag = data;
                row.Cells["Symbol"].Value = data.Symbol;
                row.Cells["FundingRate"].Value = data.GetFundingRatePercentage();
                row.Cells["FundingRate"].Style.ForeColor = fundingColor;
                row.Cells["AnnualRate"].Value = data.GetAnnualizedRate();
                row.Cells["AnnualRate"].Style.ForeColor = fundingColor;
                row.Cells["MarkPrice"].Value = data.MarkPrice;
                row.Cells["PriceChange"].Value = priceChangeText;
                row.Cells["PriceChange"].Style.ForeColor = priceChangeColor;
                row.Cells["NextFunding"].Value = nextFundingText;
                row.Cells["UpdateTime"].Value = data.Timestamp;
            }
        }

        private Color GetFundingRateColor(decimal fundingRate)
        {
            if (fundingRate > 0.0001m) return Color.Red;
            if (fundingRate > 0) return Color.DarkRed;
            if (fundingRate < -0.0001m) return Color.Green;
            if (fundingRate < 0) return Color.DarkGreen;
            return Color.Black;
        }

        private Color GetPriceChangeColor(PriceChange change)
        {
            if (!change.HasData)
                return Color.Gray;

            if (change.ChangePercent > _settings.PriceChangeThreshold)
                return Color.DarkGreen;
            if (change.ChangePercent > 0)
                return Color.Green;
            if (change.ChangePercent < -_settings.PriceChangeThreshold)
                return Color.DarkRed;
            if (change.ChangePercent < 0)
                return Color.Red;

            return Color.Black;
        }

        private void OnConnectionStatusChanged(string symbol, bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnConnectionStatusChanged(symbol, isConnected)));
                return;
            }

            UpdateStatusLabel();
        }

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

        // ============= УПРАВЛЕНИЕ СИМВОЛАМИ =============
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

            if (_monitor.IsSymbolMonitored(symbol))
            {
                MessageBox.Show($"Символ {symbol} уже отслеживается!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!symbol.EndsWith("USDT"))
            {
                symbol += "USDT";
            }

            try
            {
                await _monitor.AddSymbolAsync(symbol);
                txtSymbol.Clear();
                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления символа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
                UpdateStatusLabel();
            }
        }

        // ============= УПРАВЛЕНИЕ ЗВУКАМИ =============
        private void SelectSoundFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите звуковой файл";
                openFileDialog.Filter = "WAV файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

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

                MessageBox.Show(
                    "Установлен стандартный звук",
                    "Готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

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

        private void SelectFundingSoundFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите звуковой файл для Funding";
                openFileDialog.Filter = "WAV файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                var currentPath = _soundService.GetFundingSoundPath();
                if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(currentPath);
                    openFileDialog.FileName = Path.GetFileName(currentPath);
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_soundService.SetFundingSound(openFileDialog.FileName))
                    {
                        MessageBox.Show(
                            $"Funding звук установлен:\n{Path.GetFileName(openFileDialog.FileName)}",
                            "Успешно",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        UpdateFundingSoundButtonText();
                        _soundService.PlayFundingSound();
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

        private void ResetFundingSoundToDefault()
        {
            var result = MessageBox.Show(
                "Вернуть стандартный звук для Funding?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _soundService.SetFundingSound(null!);
                UpdateFundingSoundButtonText();

                MessageBox.Show(
                    "Установлен стандартный Funding звук",
                    "Готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void UpdateFundingSoundButtonText()
        {
            if (_soundService.HasFundingSound())
            {
                var soundPath = _soundService.GetFundingSoundPath();
                var fileName = Path.GetFileName(soundPath);
                btnSelectFundingSound.Text = $"⏰ {fileName}";
                toolTip.SetToolTip(btnSelectFundingSound, $"Funding звук: {fileName}\nПравая кнопка - сбросить");
            }
            else
            {
                btnSelectFundingSound.Text = "⏰ Funding звук";
                toolTip.SetToolTip(btnSelectFundingSound, "Выберите WAV файл для Funding уведомлений");
            }
        }

        // ============= НАСТРОЙКИ =============
        private void OpenSettings()
        {
            using (var settingsForm = new SettingsForm(_settings))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _settings = settingsForm.GetSettings();
                    dgvFundingRates.Columns["PriceChange"].HeaderText = $"Изм. за {_settings.PriceChangePeriod}м";

                    MessageBox.Show(
                        "Настройки применены успешно!",
                        "Готово",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void SaveSettings()
        {
            if (_monitor == null) return;

            try
            {
                _settings.WatchedSymbols = _monitor.GetMonitoredSymbols().ToList();
                _settings.AlwaysOnTop = chkTopMost.Checked;
                _settings.PlaySoundOnUpdate = chkPlaySound.Checked;
                _settings.PlayFundingAlert = chkFundingAlert.Checked;
                _settings.CustomSoundPath = _soundService.GetCurrentSoundPath();
                _settings.FundingSoundPath = _soundService.GetFundingSoundPath();
                _settingsService.SaveSettings(_settings);

                System.Diagnostics.Debug.WriteLine($"Настройки сохранены: {_settings.WatchedSymbols.Count} символов");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        // ============= ОБРАБОТЧИКИ СОБЫТИЙ UI =============

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
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                AddSymbolToMonitoring(txtSymbol.Text);
            }
        }

        private void chkTopMost_CheckedChanged(object? sender, EventArgs e)
        {
            this.TopMost = chkTopMost.Checked;
        }

        private void chkPlaySound_CheckedChanged(object? sender, EventArgs e)
        {
            // Настройка применяется сразу, сохранение будет при закрытии
        }

        private void chkFundingAlert_CheckedChanged(object? sender, EventArgs e)
        {
            // Настройка применяется сразу, сохранение будет при закрытии
            _settings.PlayFundingAlert = chkFundingAlert.Checked;
        }

        private void btnSelectSound_Click(object? sender, EventArgs e)
        {
            SelectSoundFile();
        }

        private void btnSelectSound_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _soundService.HasCustomSound())
            {
                ResetSoundToDefault();
            }
        }

        private void btnSelectFundingSound_Click(object? sender, EventArgs e)
        {
            SelectFundingSoundFile();
        }

        private void btnSelectFundingSound_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _soundService.HasFundingSound())
            {
                ResetFundingSoundToDefault();
            }
        }

        private void btnSettings_Click(object? sender, EventArgs e)
        {
            OpenSettings();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvFundingRates.Rows)
            {
                if (row.Tag is FundingRateData data)
                {
                    var timeUntilFunding = data.NextFundingTime - DateTime.UtcNow;
                    var seconds = (int)timeUntilFunding.TotalSeconds;

                    if (seconds < 0)
                        seconds = 0;

                    var hours = seconds / 3600;
                    var minutes = (seconds % 3600) / 60;
                    var secs = seconds % 60;

                    string nextFundingText;
                    if (hours > 0)
                        nextFundingText = $"{hours:D2}:{minutes:D2}:{secs:D2}";
                    else
                        nextFundingText = $"{minutes:D2}:{secs:D2}";

                    row.Cells["NextFunding"].Value = nextFundingText;

                    // Подсветка за 5 секунд до funding
                    if (seconds <= 5 && seconds > 0)
                    {
                        row.Cells["NextFunding"].Style.ForeColor = Color.Red;
                        row.Cells["NextFunding"].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                    else
                    {
                        row.Cells["NextFunding"].Style.ForeColor = Color.DarkSlateGray;
                        row.Cells["NextFunding"].Style.Font = new Font("Segoe UI", 9F);
                    }
                }
            }
        }

        private async void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_monitor != null && _monitor.GetMonitoredSymbolsCount() > 0)
            {
                e.Cancel = true;

                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                txtSymbol.Enabled = false;
                chkTopMost.Enabled = false;
                chkPlaySound.Enabled = false;
                chkFundingAlert.Enabled = false;

                toolStripStatusLabel.Text = "Закрытие приложения...";

                SaveSettings();

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

                this.FormClosing -= Form1_FormClosing;
                Application.Exit();
            }
        }
    }
}