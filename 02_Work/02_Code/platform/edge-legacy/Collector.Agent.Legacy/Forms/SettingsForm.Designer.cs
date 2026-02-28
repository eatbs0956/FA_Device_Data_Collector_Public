namespace Collector.Agent.Legacy.Forms
{
    partial class SettingsForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this._tabControl = new System.Windows.Forms.TabControl();
            this._generalTab = new System.Windows.Forms.TabPage();
            this._communicationTab = new System.Windows.Forms.TabPage();
            this._loggingTab = new System.Windows.Forms.TabPage();
            this._saveButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._heartbeatIntervalNumeric = new System.Windows.Forms.NumericUpDown();
            this._configRefreshIntervalNumeric = new System.Windows.Forms.NumericUpDown();
            this._autoStartCheckBox = new System.Windows.Forms.CheckBox();
            this._minimizeToTrayCheckBox = new System.Windows.Forms.CheckBox();
            this._connectionTimeoutNumeric = new System.Windows.Forms.NumericUpDown();
            this._retryCountNumeric = new System.Windows.Forms.NumericUpDown();
            this._retryIntervalNumeric = new System.Windows.Forms.NumericUpDown();
            this._logLevelComboBox = new System.Windows.Forms.ComboBox();
            this._logRetentionDaysNumeric = new System.Windows.Forms.NumericUpDown();
            this._logToFileCheckBox = new System.Windows.Forms.CheckBox();
            this._rabbitMqHostTextBox = new System.Windows.Forms.TextBox();
            this._rabbitMqPortNumeric = new System.Windows.Forms.NumericUpDown();
            this._rabbitMqUserTextBox = new System.Windows.Forms.TextBox();
            this._rabbitMqPasswordTextBox = new System.Windows.Forms.TextBox();
            this._rabbitMqExchangeTextBox = new System.Windows.Forms.TextBox();
            this._tabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._heartbeatIntervalNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._configRefreshIntervalNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._connectionTimeoutNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._retryCountNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._retryIntervalNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._logRetentionDaysNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._rabbitMqPortNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._generalTab);
            this._tabControl.Controls.Add(this._communicationTab);
            this._tabControl.Controls.Add(this._loggingTab);
            this._tabControl.Location = new System.Drawing.Point(10, 10);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(562, 403);
            this._tabControl.TabIndex = 0;
            // 
            // _generalTab
            // 
            this._generalTab.Location = new System.Drawing.Point(4, 22);
            this._generalTab.Name = "_generalTab";
            this._generalTab.Padding = new System.Windows.Forms.Padding(3);
            this._generalTab.Size = new System.Drawing.Size(554, 377);
            this._generalTab.TabIndex = 0;
            this._generalTab.Text = "通用设置";
            this._generalTab.UseVisualStyleBackColor = true;
            // 
            // _communicationTab
            // 
            this._communicationTab.Location = new System.Drawing.Point(4, 22);
            this._communicationTab.Name = "_communicationTab";
            this._communicationTab.Padding = new System.Windows.Forms.Padding(3);
            this._communicationTab.Size = new System.Drawing.Size(457, 274);
            this._communicationTab.TabIndex = 1;
            this._communicationTab.Text = "通信设置";
            this._communicationTab.UseVisualStyleBackColor = true;
            // 
            // _loggingTab
            // 
            this._loggingTab.Location = new System.Drawing.Point(4, 22);
            this._loggingTab.Name = "_loggingTab";
            this._loggingTab.Size = new System.Drawing.Size(457, 274);
            this._loggingTab.TabIndex = 2;
            this._loggingTab.Text = "日志设置";
            this._loggingTab.UseVisualStyleBackColor = true;
            // 
            // _saveButton
            // 
            this._saveButton.Location = new System.Drawing.Point(402, 419);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new System.Drawing.Size(80, 30);
            this._saveButton.TabIndex = 1;
            this._saveButton.Text = "保存";
            this._saveButton.UseVisualStyleBackColor = true;
            this._saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Location = new System.Drawing.Point(492, 419);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(80, 30);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "取消";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _heartbeatIntervalNumeric
            // 
            this._heartbeatIntervalNumeric.Location = new System.Drawing.Point(170, 20);
            this._heartbeatIntervalNumeric.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this._heartbeatIntervalNumeric.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this._heartbeatIntervalNumeric.Name = "_heartbeatIntervalNumeric";
            this._heartbeatIntervalNumeric.Size = new System.Drawing.Size(80, 21);
            this._heartbeatIntervalNumeric.TabIndex = 3;
            this._heartbeatIntervalNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // _configRefreshIntervalNumeric
            // 
            this._configRefreshIntervalNumeric.Location = new System.Drawing.Point(170, 55);
            this._configRefreshIntervalNumeric.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this._configRefreshIntervalNumeric.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this._configRefreshIntervalNumeric.Name = "_configRefreshIntervalNumeric";
            this._configRefreshIntervalNumeric.Size = new System.Drawing.Size(80, 21);
            this._configRefreshIntervalNumeric.TabIndex = 4;
            this._configRefreshIntervalNumeric.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // _autoStartCheckBox
            // 
            this._autoStartCheckBox.AutoSize = true;
            this._autoStartCheckBox.Location = new System.Drawing.Point(20, 90);
            this._autoStartCheckBox.Name = "_autoStartCheckBox";
            this._autoStartCheckBox.Size = new System.Drawing.Size(96, 16);
            this._autoStartCheckBox.TabIndex = 5;
            this._autoStartCheckBox.Text = "开机自动启动";
            this._autoStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // _minimizeToTrayCheckBox
            // 
            this._minimizeToTrayCheckBox.AutoSize = true;
            this._minimizeToTrayCheckBox.Location = new System.Drawing.Point(20, 120);
            this._minimizeToTrayCheckBox.Name = "_minimizeToTrayCheckBox";
            this._minimizeToTrayCheckBox.Size = new System.Drawing.Size(120, 16);
            this._minimizeToTrayCheckBox.TabIndex = 6;
            this._minimizeToTrayCheckBox.Text = "最小化到系统托盘";
            this._minimizeToTrayCheckBox.UseVisualStyleBackColor = true;
            // 
            // _connectionTimeoutNumeric
            // 
            this._connectionTimeoutNumeric.Location = new System.Drawing.Point(170, 20);
            this._connectionTimeoutNumeric.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this._connectionTimeoutNumeric.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this._connectionTimeoutNumeric.Name = "_connectionTimeoutNumeric";
            this._connectionTimeoutNumeric.Size = new System.Drawing.Size(80, 21);
            this._connectionTimeoutNumeric.TabIndex = 7;
            this._connectionTimeoutNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // _retryCountNumeric
            // 
            this._retryCountNumeric.Location = new System.Drawing.Point(170, 55);
            this._retryCountNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this._retryCountNumeric.Name = "_retryCountNumeric";
            this._retryCountNumeric.Size = new System.Drawing.Size(80, 21);
            this._retryCountNumeric.TabIndex = 8;
            this._retryCountNumeric.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // _retryIntervalNumeric
            // 
            this._retryIntervalNumeric.Location = new System.Drawing.Point(170, 90);
            this._retryIntervalNumeric.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this._retryIntervalNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._retryIntervalNumeric.Name = "_retryIntervalNumeric";
            this._retryIntervalNumeric.Size = new System.Drawing.Size(80, 21);
            this._retryIntervalNumeric.TabIndex = 9;
            this._retryIntervalNumeric.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // _logLevelComboBox
            // 
            this._logLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._logLevelComboBox.FormattingEnabled = true;
            this._logLevelComboBox.Items.AddRange(new object[] {
            "Debug",
            "Info",
            "Warn",
            "Error"});
            this._logLevelComboBox.Location = new System.Drawing.Point(170, 20);
            this._logLevelComboBox.Name = "_logLevelComboBox";
            this._logLevelComboBox.Size = new System.Drawing.Size(120, 20);
            this._logLevelComboBox.TabIndex = 10;
            // 
            // _logRetentionDaysNumeric
            // 
            this._logRetentionDaysNumeric.Location = new System.Drawing.Point(170, 55);
            this._logRetentionDaysNumeric.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this._logRetentionDaysNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._logRetentionDaysNumeric.Name = "_logRetentionDaysNumeric";
            this._logRetentionDaysNumeric.Size = new System.Drawing.Size(80, 21);
            this._logRetentionDaysNumeric.TabIndex = 11;
            this._logRetentionDaysNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // _logToFileCheckBox
            // 
            this._logToFileCheckBox.AutoSize = true;
            this._logToFileCheckBox.Checked = true;
            this._logToFileCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._logToFileCheckBox.Location = new System.Drawing.Point(20, 90);
            this._logToFileCheckBox.Name = "_logToFileCheckBox";
            this._logToFileCheckBox.Size = new System.Drawing.Size(96, 16);
            this._logToFileCheckBox.TabIndex = 12;
            this._logToFileCheckBox.Text = "写入日志文件";
            this._logToFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // _rabbitMqHostTextBox
            // 
            this._rabbitMqHostTextBox.Location = new System.Drawing.Point(170, 125);
            this._rabbitMqHostTextBox.Name = "_rabbitMqHostTextBox";
            this._rabbitMqHostTextBox.Size = new System.Drawing.Size(200, 21);
            this._rabbitMqHostTextBox.TabIndex = 13;
            this._rabbitMqHostTextBox.Text = "localhost";
            // 
            // _rabbitMqPortNumeric
            // 
            this._rabbitMqPortNumeric.Location = new System.Drawing.Point(170, 160);
            this._rabbitMqPortNumeric.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this._rabbitMqPortNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._rabbitMqPortNumeric.Name = "_rabbitMqPortNumeric";
            this._rabbitMqPortNumeric.Size = new System.Drawing.Size(80, 21);
            this._rabbitMqPortNumeric.TabIndex = 14;
            this._rabbitMqPortNumeric.Value = new decimal(new int[] {
            5672,
            0,
            0,
            0});
            // 
            // _rabbitMqUserTextBox
            // 
            this._rabbitMqUserTextBox.Location = new System.Drawing.Point(170, 195);
            this._rabbitMqUserTextBox.Name = "_rabbitMqUserTextBox";
            this._rabbitMqUserTextBox.Size = new System.Drawing.Size(200, 21);
            this._rabbitMqUserTextBox.TabIndex = 15;
            this._rabbitMqUserTextBox.Text = "guest";
            // 
            // _rabbitMqPasswordTextBox
            // 
            this._rabbitMqPasswordTextBox.Location = new System.Drawing.Point(170, 230);
            this._rabbitMqPasswordTextBox.Name = "_rabbitMqPasswordTextBox";
            this._rabbitMqPasswordTextBox.Size = new System.Drawing.Size(200, 21);
            this._rabbitMqPasswordTextBox.TabIndex = 16;
            this._rabbitMqPasswordTextBox.UseSystemPasswordChar = true;
            // 
            // _rabbitMqExchangeTextBox
            // 
            this._rabbitMqExchangeTextBox.Location = new System.Drawing.Point(170, 265);
            this._rabbitMqExchangeTextBox.Name = "_rabbitMqExchangeTextBox";
            this._rabbitMqExchangeTextBox.Size = new System.Drawing.Size(200, 21);
            this._rabbitMqExchangeTextBox.TabIndex = 17;
            this._rabbitMqExchangeTextBox.Text = "collector.exchange";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._saveButton);
            this.Controls.Add(this._tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设置";
            this._tabControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._heartbeatIntervalNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._configRefreshIntervalNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._connectionTimeoutNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._retryCountNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._retryIntervalNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._logRetentionDaysNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._rabbitMqPortNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _generalTab;
        private System.Windows.Forms.TabPage _communicationTab;
        private System.Windows.Forms.TabPage _loggingTab;
        private System.Windows.Forms.Button _saveButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.NumericUpDown _heartbeatIntervalNumeric;
        private System.Windows.Forms.NumericUpDown _configRefreshIntervalNumeric;
        private System.Windows.Forms.CheckBox _autoStartCheckBox;
        private System.Windows.Forms.CheckBox _minimizeToTrayCheckBox;
        private System.Windows.Forms.NumericUpDown _connectionTimeoutNumeric;
        private System.Windows.Forms.NumericUpDown _retryCountNumeric;
        private System.Windows.Forms.NumericUpDown _retryIntervalNumeric;
        private System.Windows.Forms.ComboBox _logLevelComboBox;
        private System.Windows.Forms.NumericUpDown _logRetentionDaysNumeric;
        private System.Windows.Forms.CheckBox _logToFileCheckBox;
        // RabbitMQ 配置控件
        private System.Windows.Forms.TextBox _rabbitMqHostTextBox;
        private System.Windows.Forms.NumericUpDown _rabbitMqPortNumeric;
        private System.Windows.Forms.TextBox _rabbitMqUserTextBox;
        private System.Windows.Forms.TextBox _rabbitMqPasswordTextBox;
        private System.Windows.Forms.TextBox _rabbitMqExchangeTextBox;
    }
}
