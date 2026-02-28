using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Agent.Localization;
using Collector.Core.ApiClient;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 登录 ViewModel
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<LoginViewModel> _logger;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    private bool _showPassword;

    public event EventHandler? LoginSucceeded;

    // 多语言属性
    public string AppTitle => LocalizationManager.T("App.Title");
    public string LoginTitle => LocalizationManager.T("Login.Title");
    public string UsernameLabel => LocalizationManager.T("Login.Username");
    public string UsernamePlaceholder => LocalizationManager.T("Login.UsernamePlaceholder");
    public string PasswordLabel => LocalizationManager.T("Login.Password");
    public string PasswordPlaceholder => LocalizationManager.T("Login.PasswordPlaceholder");
    public string RememberMeLabel => LocalizationManager.T("Login.RememberMe");
    public string LoginButtonText => IsBusy ? LocalizationManager.T("Login.LoggingIn") : LocalizationManager.T("Login.Button");
    
    // 显示密码相关
    public char PasswordChar => ShowPassword ? '\0' : '●';
    public string ShowPasswordIcon => ShowPassword ? "🙈" : "👁";
    public string ShowPasswordTooltip => ShowPassword 
        ? LocalizationManager.T("Login.HidePassword") 
        : LocalizationManager.T("Login.ShowPassword");

    public LoginViewModel(IAuthApiClient authApiClient, ILogger<LoginViewModel> logger)
    {
        _authApiClient = authApiClient;
        _logger = logger;
        
        // 订阅语言变更事件
        LocalizationManager.LanguageChanged += (s, e) => RefreshLabels();
    }

    partial void OnShowPasswordChanged(bool value)
    {
        OnPropertyChanged(nameof(PasswordChar));
        OnPropertyChanged(nameof(ShowPasswordIcon));
        OnPropertyChanged(nameof(ShowPasswordTooltip));
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsBusy))
        {
            OnPropertyChanged(nameof(LoginButtonText));
        }
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    private void RefreshLabels()
    {
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(LoginTitle));
        OnPropertyChanged(nameof(UsernameLabel));
        OnPropertyChanged(nameof(UsernamePlaceholder));
        OnPropertyChanged(nameof(PasswordLabel));
        OnPropertyChanged(nameof(PasswordPlaceholder));
        OnPropertyChanged(nameof(RememberMeLabel));
        OnPropertyChanged(nameof(LoginButtonText));
        OnPropertyChanged(nameof(ShowPasswordTooltip));
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = LocalizationManager.CurrentLanguage == "zh-CN" 
                ? "请输入用户名和密码" 
                : "Please enter username and password";
            return;
        }

        SetBusy(true, LocalizationManager.T("Login.LoggingIn"));
        ErrorMessage = null;

        try
        {
            var request = new LoginRequest
            {
                UserName = UserName,
                Password = Password
            };

            var result = await _authApiClient.LoginAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("登录成功: {UserName}", UserName);
                
                // 清空密码
                Password = string.Empty;
                
                // 触发登录成功事件
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.Msg ?? LocalizationManager.T("Login.Failed");
                _logger.LogWarning("登录失败: {Msg}", result.Msg);
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = LocalizationManager.T("Login.NetworkError") + $"\n{ex.Message}";
            _logger.LogError(ex, "登录网络异常");
        }
        catch (Exception ex)
        {
            ErrorMessage = LocalizationManager.T("Login.ServerError") + $"\n{ex.Message}";
            _logger.LogError(ex, "登录异常");
        }
        finally
        {
            SetBusy(false);
        }
    }
}
