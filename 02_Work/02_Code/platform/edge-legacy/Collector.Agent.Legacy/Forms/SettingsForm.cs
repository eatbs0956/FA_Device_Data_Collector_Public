using System;
using System.Drawing;
using System.Windows.Forms;
using Collector.Agent.Legacy.Services;
using NLog;

namespace Collector.Agent.Legacy.Forms
{
    public partial class SettingsForm : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppService _appService;
        
        // 语言选择控件
        private ComboBox _languageComboBox;

        public SettingsForm()
        {
            _appService = AppService.Instance;
            InitializeComponent();
            InitializeText();
            InitializeTabPages();
            InitializeEvents();
            LoadSettings();
        }

        private void InitializeText()
        {
            // 窗体标题
            this.Text = L.T("SettingsForm_Title");
            
            // Tab 标签
            _generalTab.Text = L.T("SettingsForm_Tab_General");
            _communicationTab.Text = L.T("SettingsForm_Tab_Communication");
            _loggingTab.Text = L.T("SettingsForm_Tab_Logging");
            
            // 按钮
            _saveButton.Text = L.T("Common_Save");
            _cancelButton.Text = L.T("Common_Cancel");
            
            // 复选框
            _autoStartCheckBox.Text = L.T("SettingsForm_Checkbox_AutoStart");
            _minimizeToTrayCheckBox.Text = L.T("SettingsForm_Checkbox_MinimizeToTray");
            _logToFileCheckBox.Text = L.T("SettingsForm_Checkbox_LogToFile");
        }

        private void InitializeTabPages()
        {
            CreateGeneralSettings();
            CreateCommunicationSettings();
            CreateLoggingSettings();
        }

        private void InitializeEvents()
        {
            _cancelButton.Click += (s, e) => this.Close();
        }

        private void CreateGeneralSettings()
        {
            int y = 20;
            int labelWidth = 150;

            // 语言设置
            AddLabel(_generalTab, L.T("SettingsForm_Label_Language"), 20, y);
            _languageComboBox = new ComboBox
            {
                Location = new Point(20 + labelWidth, y),
                Size = new Size(150, 21),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _languageComboBox.Items.Add(L.T("SettingsForm_Language_System"));
            _languageComboBox.Items.Add("简体中文");
            _languageComboBox.Items.Add("English");
            _languageComboBox.SelectedIndex = 0;
            _generalTab.Controls.Add(_languageComboBox);
            y += 35;

            AddLabel(_generalTab, L.T("SettingsForm_Label_HeartbeatInterval"), 20, y);
            _heartbeatIntervalNumeric.Location = new Point(20 + labelWidth, y);
            _generalTab.Controls.Add(_heartbeatIntervalNumeric);
            y += 35;

            AddLabel(_generalTab, L.T("SettingsForm_Label_ConfigRefreshInterval"), 20, y);
            _configRefreshIntervalNumeric.Location = new Point(20 + labelWidth, y);
            _generalTab.Controls.Add(_configRefreshIntervalNumeric);
            y += 35;

            _autoStartCheckBox.Location = new Point(20, y);
            _generalTab.Controls.Add(_autoStartCheckBox);
            y += 30;

            _minimizeToTrayCheckBox.Location = new Point(20, y);
            _generalTab.Controls.Add(_minimizeToTrayCheckBox);
        }

        private void CreateCommunicationSettings()
        {
            int y = 20;
            int labelWidth = 150;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_ConnectionTimeout"), 20, y);
            _connectionTimeoutNumeric.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_connectionTimeoutNumeric);
            y += 35;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_RetryCount"), 20, y);
            _retryCountNumeric.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_retryCountNumeric);
            y += 35;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_RetryInterval"), 20, y);
            _retryIntervalNumeric.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_retryIntervalNumeric);
            y += 40;

            // RabbitMQ 分隔线标题
            var mqGroupLabel = new Label
            {
                Text = "── RabbitMQ ──",
                Location = new Point(20, y),
                Size = new Size(420, 20),
                ForeColor = Color.Gray
            };
            _communicationTab.Controls.Add(mqGroupLabel);
            y += 25;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_RabbitMqHost"), 20, y);
            _rabbitMqHostTextBox.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_rabbitMqHostTextBox);
            y += 35;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_RabbitMqPort"), 20, y);
            _rabbitMqPortNumeric.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_rabbitMqPortNumeric);
            y += 35;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_RabbitMqUser"), 20, y);
            _rabbitMqUserTextBox.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_rabbitMqUserTextBox);
            y += 35;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_RabbitMqPassword"), 20, y);
            _rabbitMqPasswordTextBox.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_rabbitMqPasswordTextBox);
            y += 35;

            AddLabel(_communicationTab, L.T("SettingsForm_Label_RabbitMqExchange"), 20, y);
            _rabbitMqExchangeTextBox.Location = new Point(20 + labelWidth, y);
            _communicationTab.Controls.Add(_rabbitMqExchangeTextBox);
        }

        private void CreateLoggingSettings()
        {
            int y = 20;
            int labelWidth = 150;

            AddLabel(_loggingTab, L.T("SettingsForm_Label_LogLevel"), 20, y);
            _logLevelComboBox.Location = new Point(20 + labelWidth, y);
            _logLevelComboBox.SelectedIndex = 1;
            _loggingTab.Controls.Add(_logLevelComboBox);
            y += 35;

            AddLabel(_loggingTab, L.T("SettingsForm_Label_LogRetention"), 20, y);
            _logRetentionDaysNumeric.Location = new Point(20 + labelWidth, y);
            _loggingTab.Controls.Add(_logRetentionDaysNumeric);
            y += 35;

            _logToFileCheckBox.Location = new Point(20, y);
            _loggingTab.Controls.Add(_logToFileCheckBox);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                Size = new Size(140, 20)
            };
            parent.Controls.Add(label);
        }

        private void LoadSettings()
        {
            try
            {
                // 加载语言设置
                var currentLanguage = LanguageManager.GetCurrentLanguage();
                switch (currentLanguage)
                {
                    case "zh-CN":
                        _languageComboBox.SelectedIndex = 1;
                        break;
                    case "en-US":
                        _languageComboBox.SelectedIndex = 2;
                        break;
                    default:
                        _languageComboBox.SelectedIndex = 0;
                        break;
                }
                
                // 加载日志级别设置
                var logLevel = _appService.GetSetting("LogLevel", "Info");
                switch (logLevel)
                {
                    case "Trace":
                        _logLevelComboBox.SelectedIndex = 0;
                        break;
                    case "Debug":
                        _logLevelComboBox.SelectedIndex = 1;
                        break;
                    case "Info":
                        _logLevelComboBox.SelectedIndex = 2;
                        break;
                    case "Warn":
                        _logLevelComboBox.SelectedIndex = 3;
                        break;
                    case "Error":
                        _logLevelComboBox.SelectedIndex = 4;
                        break;
                    default:
                        _logLevelComboBox.SelectedIndex = 2; // 默认 Info
                        break;
                }
                
                // 加载其他设置
                _heartbeatIntervalNumeric.Value = int.Parse(_appService.GetSetting("HeartbeatInterval", "30"));
                _configRefreshIntervalNumeric.Value = int.Parse(_appService.GetSetting("ConfigRefreshInterval", "300"));
                _connectionTimeoutNumeric.Value = int.Parse(_appService.GetSetting("ConnectionTimeout", "30"));
                _retryCountNumeric.Value = int.Parse(_appService.GetSetting("RetryCount", "3"));
                _retryIntervalNumeric.Value = int.Parse(_appService.GetSetting("RetryInterval", "5"));
                _logRetentionDaysNumeric.Value = int.Parse(_appService.GetSetting("LogRetentionDays", "30"));
                _autoStartCheckBox.Checked = bool.Parse(_appService.GetSetting("AutoStart", "false"));
                _minimizeToTrayCheckBox.Checked = bool.Parse(_appService.GetSetting("MinimizeToTray", "true"));
                _logToFileCheckBox.Checked = bool.Parse(_appService.GetSetting("LogToFile", "true"));
                
                // 加载 RabbitMQ 设置
                var settings = _appService.LoadSettings();
                _rabbitMqHostTextBox.Text = settings.RabbitMqHost ?? "localhost";
                _rabbitMqPortNumeric.Value = settings.RabbitMqPort > 0 ? settings.RabbitMqPort : 5672;
                _rabbitMqUserTextBox.Text = settings.RabbitMqUser ?? "guest";
                _rabbitMqPasswordTextBox.Text = settings.RabbitMqPassword ?? "";
                _rabbitMqExchangeTextBox.Text = settings.RabbitMqExchange ?? "collector.exchange";
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "加载设置失败");
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 保存语言设置
                string language = "";
                switch (_languageComboBox.SelectedIndex)
                {
                    case 1:
                        language = "zh-CN";
                        break;
                    case 2:
                        language = "en-US";
                        break;
                    default:
                        language = "";
                        break;
                }
                
                // 检查语言是否变更
                bool languageChanged = language != LanguageManager.GetCurrentLanguage();
                
                if (languageChanged)
                {
                    LanguageManager.SetCulture(language);
                    _appService.SaveLanguageSetting(language);
                }
                
                // 保存日志级别
                string logLevel = "";
                switch (_logLevelComboBox.SelectedIndex)
                {
                    case 0:
                        logLevel = "Trace";
                        break;
                    case 1:
                        logLevel = "Debug";
                        break;
                    case 2:
                        logLevel = "Info";
                        break;
                    case 3:
                        logLevel = "Warn";
                        break;
                    case 4:
                        logLevel = "Error";
                        break;
                    default:
                        logLevel = "Info";
                        break;
                }
                _appService.SaveSetting("LogLevel", logLevel);
                
                // 保存其他设置
                _appService.SaveSetting("HeartbeatInterval", _heartbeatIntervalNumeric.Value.ToString());
                _appService.SaveSetting("ConfigRefreshInterval", _configRefreshIntervalNumeric.Value.ToString());
                _appService.SaveSetting("ConnectionTimeout", _connectionTimeoutNumeric.Value.ToString());
                _appService.SaveSetting("RetryCount", _retryCountNumeric.Value.ToString());
                _appService.SaveSetting("RetryInterval", _retryIntervalNumeric.Value.ToString());
                _appService.SaveSetting("LogRetentionDays", _logRetentionDaysNumeric.Value.ToString());
                _appService.SaveSetting("AutoStart", _autoStartCheckBox.Checked.ToString());
                _appService.SaveSetting("MinimizeToTray", _minimizeToTrayCheckBox.Checked.ToString());
                _appService.SaveSetting("LogToFile", _logToFileCheckBox.Checked.ToString());
                
                // 保存 RabbitMQ 设置
                _appService.SaveRabbitMqSettings(
                    _rabbitMqHostTextBox.Text.Trim(),
                    (int)_rabbitMqPortNumeric.Value,
                    _rabbitMqUserTextBox.Text.Trim(),
                    _rabbitMqPasswordTextBox.Text,
                    _rabbitMqExchangeTextBox.Text.Trim());
                
                // 动态更新 NLog 日志级别
                UpdateNLogLevel(logLevel);
                
                if (languageChanged)
                {
                    MessageBox.Show(
                        L.T("SettingsForm_Msg_RestartRequired"),
                        L.T("Msg_Info"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                
                Logger.Info("设置已保存");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存设置失败");
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateNLogLevel(string logLevel)
        {
            try
            {
                var config = NLog.LogManager.Configuration;
                if (config != null)
                {
                    var level = NLog.LogLevel.FromString(logLevel);
                    foreach (var rule in config.LoggingRules)
                    {
                        rule.EnableLoggingForLevel(level);
                    }
                    NLog.LogManager.ReconfigExistingLoggers();
                    Logger.Info($"日志级别已更新为: {logLevel}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "更新日志级别失败");
            }
        }
    }
}
