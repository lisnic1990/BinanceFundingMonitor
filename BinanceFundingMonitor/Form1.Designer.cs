namespace BinanceFundingMonitor
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvFundingRates;
        private System.Windows.Forms.TextBox txtSymbol;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnSelectSound;
        private System.Windows.Forms.Button btnSelectFundingSound;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.CheckBox chkTopMost;
        private System.Windows.Forms.CheckBox chkPlaySound;
        private System.Windows.Forms.CheckBox chkFundingAlert;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelControl;
        private System.Windows.Forms.Label lblSymbol;
        private System.Windows.Forms.ToolTip toolTip;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            _soundService?.Dispose();
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dgvFundingRates = new System.Windows.Forms.DataGridView();
            this.txtSymbol = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnSelectSound = new System.Windows.Forms.Button();
            this.btnSelectFundingSound = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.chkTopMost = new System.Windows.Forms.CheckBox();
            this.chkPlaySound = new System.Windows.Forms.CheckBox();
            this.chkFundingAlert = new System.Windows.Forms.CheckBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelControl = new System.Windows.Forms.Panel();
            this.lblSymbol = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);

            ((System.ComponentModel.ISupportInitialize)(this.dgvFundingRates)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.panelControl.SuspendLayout();
            this.SuspendLayout();

            // dgvFundingRates
            this.dgvFundingRates.BackgroundColor = System.Drawing.Color.White;
            this.dgvFundingRates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFundingRates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFundingRates.Location = new System.Drawing.Point(0, 170);
            this.dgvFundingRates.Name = "dgvFundingRates";
            this.dgvFundingRates.RowHeadersWidth = 51;
            this.dgvFundingRates.Size = new System.Drawing.Size(1150, 408);
            this.dgvFundingRates.TabIndex = 0;

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 60);
            this.panelTop.TabIndex = 1;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(420, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "📊 Binance Funding Rate Monitor";

            // panelControl
            this.panelControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelControl.Controls.Add(this.lblSymbol);
            this.panelControl.Controls.Add(this.txtSymbol);
            this.panelControl.Controls.Add(this.btnAdd);
            this.panelControl.Controls.Add(this.btnRemove);
            this.panelControl.Controls.Add(this.btnSelectSound);
            this.panelControl.Controls.Add(this.btnSelectFundingSound);
            this.panelControl.Controls.Add(this.btnSettings);
            this.panelControl.Controls.Add(this.chkTopMost);
            this.panelControl.Controls.Add(this.chkPlaySound);
            this.panelControl.Controls.Add(this.chkFundingAlert);
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl.Location = new System.Drawing.Point(0, 60);
            this.panelControl.Name = "panelControl";
            this.panelControl.Padding = new System.Windows.Forms.Padding(10);
            this.panelControl.Size = new System.Drawing.Size(1150, 110);
            this.panelControl.TabIndex = 2;

            // lblSymbol
            this.lblSymbol.AutoSize = true;
            this.lblSymbol.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSymbol.Location = new System.Drawing.Point(13, 15);
            this.lblSymbol.Name = "lblSymbol";
            this.lblSymbol.Size = new System.Drawing.Size(143, 15);
            this.lblSymbol.TabIndex = 5;
            this.lblSymbol.Text = "Добавить криптовалюту:";

            // txtSymbol
            this.txtSymbol.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSymbol.Location = new System.Drawing.Point(165, 13);
            this.txtSymbol.Name = "txtSymbol";
            this.txtSymbol.Size = new System.Drawing.Size(100, 25);
            this.txtSymbol.TabIndex = 0;
            this.txtSymbol.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSymbol_KeyPress);

            // btnAdd
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(275, 13);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 25);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "➕ Добавить";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // btnRemove
            this.btnRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRemove.ForeColor = System.Drawing.Color.White;
            this.btnRemove.Location = new System.Drawing.Point(385, 13);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(100, 25);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "❌ Удалить";
            this.btnRemove.UseVisualStyleBackColor = false;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);

            // btnSelectSound
            this.btnSelectSound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnSelectSound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectSound.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnSelectSound.ForeColor = System.Drawing.Color.White;
            this.btnSelectSound.Location = new System.Drawing.Point(13, 50);
            this.btnSelectSound.Name = "btnSelectSound";
            this.btnSelectSound.Size = new System.Drawing.Size(140, 25);
            this.btnSelectSound.TabIndex = 5;
            this.btnSelectSound.Text = "🔊 Выбрать звук";
            this.btnSelectSound.UseVisualStyleBackColor = false;
            this.btnSelectSound.Click += new System.EventHandler(this.btnSelectSound_Click);
            this.btnSelectSound.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnSelectSound_MouseDown);
            this.toolTip.SetToolTip(this.btnSelectSound, "Выберите WAV файл для звука обновления\nПравая кнопка мыши - сбросить на стандартный");

            // btnSelectFundingSound
            this.btnSelectFundingSound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.btnSelectFundingSound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectFundingSound.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnSelectFundingSound.ForeColor = System.Drawing.Color.White;
            this.btnSelectFundingSound.Location = new System.Drawing.Point(163, 50);
            this.btnSelectFundingSound.Name = "btnSelectFundingSound";
            this.btnSelectFundingSound.Size = new System.Drawing.Size(140, 25);
            this.btnSelectFundingSound.TabIndex = 6;
            this.btnSelectFundingSound.Text = "⏰ Funding звук";
            this.btnSelectFundingSound.UseVisualStyleBackColor = false;
            this.btnSelectFundingSound.Click += new System.EventHandler(this.btnSelectFundingSound_Click);
            this.btnSelectFundingSound.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnSelectFundingSound_MouseDown);
            this.toolTip.SetToolTip(this.btnSelectFundingSound, "Выберите WAV файл для Funding уведомлений\nПравая кнопка мыши - сбросить на стандартный");

            // btnSettings
            this.btnSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(313, 50);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(120, 25);
            this.btnSettings.TabIndex = 7;
            this.btnSettings.Text = "⚙️ Настройки";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            this.toolTip.SetToolTip(this.btnSettings, "Настройки мониторинга цены и порогов");

            // chkTopMost
            this.chkTopMost.AutoSize = true;
            this.chkTopMost.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkTopMost.Location = new System.Drawing.Point(13, 85);
            this.chkTopMost.Name = "chkTopMost";
            this.chkTopMost.Size = new System.Drawing.Size(138, 19);
            this.chkTopMost.TabIndex = 3;
            this.chkTopMost.Text = "📌 Поверх всех окон";
            this.chkTopMost.UseVisualStyleBackColor = true;
            this.chkTopMost.CheckedChanged += new System.EventHandler(this.chkTopMost_CheckedChanged);

            // chkPlaySound
            this.chkPlaySound.AutoSize = true;
            this.chkPlaySound.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkPlaySound.Location = new System.Drawing.Point(170, 85);
            this.chkPlaySound.Name = "chkPlaySound";
            this.chkPlaySound.Size = new System.Drawing.Size(159, 19);
            this.chkPlaySound.TabIndex = 4;
            this.chkPlaySound.Text = "🔊 Звук при обновлении";
            this.chkPlaySound.UseVisualStyleBackColor = true;
            this.chkPlaySound.CheckedChanged += new System.EventHandler(this.chkPlaySound_CheckedChanged);

            // chkFundingAlert
            this.chkFundingAlert.AutoSize = true;
            this.chkFundingAlert.Checked = true;
            this.chkFundingAlert.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFundingAlert.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkFundingAlert.Location = new System.Drawing.Point(350, 85);
            this.chkFundingAlert.Name = "chkFundingAlert";
            this.chkFundingAlert.Size = new System.Drawing.Size(200, 19);
            this.chkFundingAlert.TabIndex = 8;
            this.chkFundingAlert.Text = "⏰ Звук за 5 сек до Funding";
            this.chkFundingAlert.UseVisualStyleBackColor = true;
            this.chkFundingAlert.CheckedChanged += new System.EventHandler(this.chkFundingAlert_CheckedChanged);
            this.toolTip.SetToolTip(this.chkFundingAlert, "Проигрывать звук за 5 секунд до Funding и при его наступлении");

            // statusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 578);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip.TabIndex = 3;

            // toolStripStatusLabel
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(200, 17);
            this.toolStripStatusLabel.Text = "Готов к работе";

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 600);
            this.Controls.Add(this.dgvFundingRates);
            this.Controls.Add(this.panelControl);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.statusStrip);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Binance Funding Rate Monitor";

            ((System.ComponentModel.ISupportInitialize)(this.dgvFundingRates)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelControl.ResumeLayout(false);
            this.panelControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}