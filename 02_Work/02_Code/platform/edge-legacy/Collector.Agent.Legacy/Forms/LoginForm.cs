using System;
using System.Drawing;
using System.Windows.Forms;
using Collector.Agent.Legacy.Services;
using Collector.Core.ApiClient;
using Collector.Core.Models;
using NLog;

namespace Collector.Agent.Legacy.Forms
{
    public partial class LoginForm : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppService _appService;
        private readonly IAuthApiClient _authApiClient;
        private readonly IAdminApiClient _adminApiClient;

        public LoginForm()
        {
            _appService = AppService.Instance;
            _authApiClient = ServiceLocator.GetService<IAuthApiClient>();
            _adminApiClient = ServiceLocator.GetService<IAdminApiClient>();
            InitializeComponent();
            InitializeText();
            InitializeData();
            LoadSettings();
        }

        private void InitializeText()
        {
            // 窗体标题
            this.Text = L.T("LoginForm_Title");
            
            // 标题标签
            _titleLabel.Text = L.T("AppTitle");
            
            // 标签
            _gatewayLabel.Text = L.T("LoginForm_Label_Server");
            _nodeIdLabel.Text = L.T("LoginForm_Label_NodeId");
            _nodeNameLabel.Text = L.T("LoginForm_Label_NodeName");
            _usernameLabel.Text = L.T("LoginForm_Label_Username");
            _passwordLabel.Text = L.T("LoginForm_Label_Password");
            
            // 复选框
            _showPasswordCheckBox.Text = L.T("LoginForm_Checkbox_ShowPassword");
            
            // 按钮
            _loginButton.Text = L.T("LoginForm_Button_Login");
            _settingsButton.Text = L.T("LoginForm_Button_Settings");
        }

        private void InitializeData()
        {
            _nodeIdTextBox.Text = Environment.MachineName;
            _nodeNameTextBox.Text = L.T("LoginForm_DefaultNodeName", Environment.MachineName);
            
            var separator = new Label
            {
                Text = L.T("LoginForm_Label_UserAuth"),
                Location = new Point(30, 185),
                Size = new Size(350, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            this.Controls.Add(separator);
        }

        private void LoadSettings()
        {
            try
            {
                var settings = _appService.LoadSettings();
                if (settings != null)
                {
                    _gatewayTextBox.Text = settings.ApiGatewayUrl ?? "http://localhost:60620";
                    _nodeIdTextBox.Text = settings.NodeId ?? Environment.MachineName;
                    _nodeNameTextBox.Text = settings.NodeName ?? L.T("LoginForm_DefaultNodeName", Environment.MachineName);
                    _usernameTextBox.Text = settings.Username ?? "";
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "加载配置失败，使用默认值");
            }
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _passwordTextBox.PasswordChar = _showPasswordCheckBox.Checked ? '\0' : '●';
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(_gatewayTextBox.Text))
            {
                _statusLabel.Text = L.T("LoginForm_Error_GatewayEmpty");
                return;
            }

            if (string.IsNullOrWhiteSpace(_nodeIdTextBox.Text))
            {
                _statusLabel.Text = L.T("LoginForm_Error_NodeIdEmpty");
                return;
            }

            if (string.IsNullOrWhiteSpace(_usernameTextBox.Text) || string.IsNullOrWhiteSpace(_passwordTextBox.Text))
            {
                _statusLabel.Text = L.T("LoginForm_Error_CredentialsEmpty");
                return;
            }

            _loginButton.Enabled = false;
            _statusLabel.ForeColor = Color.Blue;
            _statusLabel.Text = L.T("LoginForm_Status_Logging");

            try
            {
                // 保存配置
                _appService.SaveSettings(
                    _gatewayTextBox.Text,
                    _nodeIdTextBox.Text,
                    _nodeNameTextBox.Text,
                    _usernameTextBox.Text
                );

                // 使用 IAuthApiClient 执行登录
                var loginResult = await _authApiClient.LoginAsync(new LoginRequest
                {
                    UserName = _usernameTextBox.Text,
                    Password = _passwordTextBox.Text
                });

                if (loginResult.IsSuccess)
                {
                    // 使用 IAdminApiClient 注册节点
                    _statusLabel.Text = L.T("LoginForm_Status_Registering");
                    var registerResult = await _adminApiClient.RegisterNodeAsync(new NodeRegisterRequest
                    {
                        NodeId = _nodeIdTextBox.Text,
                        NodeName = _nodeNameTextBox.Text,
                        Platform = "NET472",
                        Version = "2.0.0",
                        IpAddress = AppService.GetLocalIpAddress(),
                        OsInfo = Environment.OSVersion.ToString()
                    });

                    if (registerResult.IsSuccess)
                    {
                        Logger.Info("登录成功，用户: {0}", _usernameTextBox.Text);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        _statusLabel.ForeColor = Color.Red;
                        _statusLabel.Text = L.T("LoginForm_Error_RegisterFailed", registerResult.Msg);
                    }
                }
                else
                {
                    _statusLabel.ForeColor = Color.Red;
                    _statusLabel.Text = L.T("LoginForm_Error_LoginFailed", loginResult.Msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "登录过程出错");
                _statusLabel.ForeColor = Color.Red;
                _statusLabel.Text = L.T("LoginForm_Error_Exception", ex.Message);
            }
            finally
            {
                _loginButton.Enabled = true;
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm())
            {
                settingsForm.ShowDialog(this);
            }
        }
    }
}
