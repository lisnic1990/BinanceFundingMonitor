using System;
using System.Windows.Forms;
using BinanceFundingMonitor.Models;

namespace BinanceFundingMonitor
{
    public partial class SettingsForm : Form
    {
        private AppSettings _settings;
        private NumericUpDown nudPeriod = null!;
        private NumericUpDown nudThreshold = null!;
        private Label lblPeriod = null!;
        private Label lblThreshold = null!;
        private Label lblPeriodDesc = null!;
        private Label lblThresholdDesc = null!;
        private Button btnOK = null!;
        private Button btnCancel = null!;

        public SettingsForm(AppSettings settings)
        {
            _settings = new AppSettings
            {
                PriceChangePeriod = settings.PriceChangePeriod,
                PriceChangeThreshold = settings.PriceChangeThreshold
            };

            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.nudPeriod = new NumericUpDown();
            this.nudThreshold = new NumericUpDown();
            this.lblPeriod = new Label();
            this.lblThreshold = new Label();
            this.lblPeriodDesc = new Label();
            this.lblThresholdDesc = new Label();
            this.btnOK = new Button();
            this.btnCancel = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.nudPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).BeginInit();
            this.SuspendLayout();

            // lblPeriod
            this.lblPeriod.AutoSize = true;
            this.lblPeriod.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPeriod.Location = new System.Drawing.Point(20, 20);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(250, 19);
            this.lblPeriod.TabIndex = 0;
            this.lblPeriod.Text = "Период отслеживания цены (минуты):";

            // nudPeriod
            this.nudPeriod.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.nudPeriod.Location = new System.Drawing.Point(20, 45);
            this.nudPeriod.Maximum = 30;
            this.nudPeriod.Minimum = 1;
            this.nudPeriod.Name = "nudPeriod";
            this.nudPeriod.Size = new System.Drawing.Size(120, 25);
            this.nudPeriod.TabIndex = 1;
            this.nudPeriod.Value = 5;

            // lblPeriodDesc
            this.lblPeriodDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPeriodDesc.ForeColor = System.Drawing.Color.Gray;
            this.lblPeriodDesc.Location = new System.Drawing.Point(20, 75);
            this.lblPeriodDesc.Name = "lblPeriodDesc";
            this.lblPeriodDesc.Size = new System.Drawing.Size(400, 40);
            this.lblPeriodDesc.TabIndex = 2;
            this.lblPeriodDesc.Text = "За какой период отслеживать изменение цены.\nНапример: 5 минут = покажет изменение цены за последние 5 минут.";

            // lblThreshold
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblThreshold.Location = new System.Drawing.Point(20, 125);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(260, 19);
            this.lblThreshold.TabIndex = 3;
            this.lblThreshold.Text = "Порог резкого импульса цены (%):";

            // nudThreshold
            this.nudThreshold.DecimalPlaces = 1;
            this.nudThreshold.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.nudThreshold.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.nudThreshold.Location = new System.Drawing.Point(20, 150);
            this.nudThreshold.Maximum = 10;
            this.nudThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            this.nudThreshold.Name = "nudThreshold";
            this.nudThreshold.Size = new System.Drawing.Size(120, 25);
            this.nudThreshold.TabIndex = 4;
            this.nudThreshold.Value = 1;

            // lblThresholdDesc
            this.lblThresholdDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblThresholdDesc.ForeColor = System.Drawing.Color.Gray;
            this.lblThresholdDesc.Location = new System.Drawing.Point(20, 180);
            this.lblThresholdDesc.Name = "lblThresholdDesc";
            this.lblThresholdDesc.Size = new System.Drawing.Size(400, 55);
            this.lblThresholdDesc.TabIndex = 5;
            this.lblThresholdDesc.Text = "При превышении этого порога строка будет подсвечена.\nНапример: 1.0% = если цена изменилась более чем на ±1%,\nстрока будет подсвечена зеленым (рост) или красным (падение).";

            // btnOK
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(76, 175, 80);
            this.btnOK.FlatStyle = FlatStyle.Flat;
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Location = new System.Drawing.Point(200, 260);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "✓ Применить";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location = new System.Drawing.Point(310, 260);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "✕ Отмена";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // SettingsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 320);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblThresholdDesc);
            this.Controls.Add(this.nudThreshold);
            this.Controls.Add(this.lblThreshold);
            this.Controls.Add(this.lblPeriodDesc);
            this.Controls.Add(this.nudPeriod);
            this.Controls.Add(this.lblPeriod);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "⚙️ Настройки мониторинга";

            ((System.ComponentModel.ISupportInitialize)(this.nudPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSettings()
        {
            nudPeriod.Value = _settings.PriceChangePeriod;
            nudThreshold.Value = _settings.PriceChangeThreshold;
        }

        private void btnOK_Click(object? sender, EventArgs e)
        {
            _settings.PriceChangePeriod = (int)nudPeriod.Value;
            _settings.PriceChangeThreshold = nudThreshold.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public AppSettings GetSettings()
        {
            return _settings;
        }
    }
}