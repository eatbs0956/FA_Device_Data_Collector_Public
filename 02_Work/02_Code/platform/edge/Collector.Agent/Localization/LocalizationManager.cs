namespace Collector.Agent.Localization;

/// <summary>
/// 本地化资源管理器
/// </summary>
public static class LocalizationManager
{
    private static string _currentLanguage = "zh-CN";
    private static readonly Dictionary<string, Dictionary<string, string>> _resources = new();

    public static event EventHandler? LanguageChanged;

    static LocalizationManager()
    {
        // 初始化中文资源
        _resources["zh-CN"] = new Dictionary<string, string>
        {
            // 通用
            ["App.Title"] = "DevDCP 数据采集客户端",
            ["App.SubTitle"] = "数据采集客户端",
            ["Common.OK"] = "确定",
            ["Common.Cancel"] = "取消",
            ["Common.Save"] = "保存",
            ["Common.Close"] = "关闭",
            ["Common.Refresh"] = "刷新",
            ["Common.Start"] = "启动",
            ["Common.Stop"] = "停止",
            ["Common.Delete"] = "删除",
            ["Common.Edit"] = "编辑",
            ["Common.Add"] = "添加",
            ["Common.Search"] = "搜索",
            ["Common.Loading"] = "加载中...",
            ["Common.Success"] = "成功",
            ["Common.Error"] = "错误",
            ["Common.Warning"] = "警告",
            ["Common.Info"] = "信息",

            // 登录页面
            ["Login.Title"] = "请登录以继续",
            ["Login.Username"] = "用户名",
            ["Login.UsernamePlaceholder"] = "请输入用户名",
            ["Login.Password"] = "密码",
            ["Login.PasswordPlaceholder"] = "请输入密码",
            ["Login.RememberMe"] = "记住登录状态",
            ["Login.ShowPassword"] = "显示密码",
            ["Login.HidePassword"] = "隐藏密码",
            ["Login.Button"] = "登 录",
            ["Login.LoggingIn"] = "登录中...",
            ["Login.Failed"] = "登录失败",
            ["Login.InvalidCredentials"] = "用户名或密码错误",
            ["Login.NetworkError"] = "网络连接失败，请检查服务器地址",
            ["Login.ServerError"] = "服务器错误，请稍后重试",

            // 主窗口
            ["MainWindow.Title"] = "DevDCP 数据采集客户端",
            ["MainWindow.Logout"] = "退出登录",
            ["MainWindow.PullConfig"] = "拉取配置",
            ["MainWindow.ConfigChanged"] = "服务器配置已更新，请点击拉取最新配置",

            // 菜单
            ["Menu.Dashboard"] = "仪表板",
            ["Menu.Tasks"] = "任务管理",
            ["Menu.Devices"] = "设备状态",
            ["Menu.DataPreview"] = "数据预览",
            ["Menu.Diagnostics"] = "诊断工具",
            ["Menu.Logs"] = "系统日志",
            ["Menu.Settings"] = "系统设置",

            // 仪表板
            ["Dashboard.Title"] = "仪表板",
            ["Dashboard.TaskCount"] = "任务数量",
            ["Dashboard.DeviceCount"] = "设备数量",
            ["Dashboard.TagCount"] = "标签数量",
            ["Dashboard.CollectionRate"] = "采集频率",
            ["Dashboard.RunningTasks"] = "运行中的任务",
            ["Dashboard.RecentData"] = "最近采集数据",

            // 任务管理
            ["Tasks.Title"] = "任务管理",
            ["Tasks.Name"] = "任务名称",
            ["Tasks.Status"] = "状态",
            ["Tasks.CollectionCount"] = "采集次数",
            ["Tasks.ErrorCount"] = "错误次数",
            ["Tasks.LastRun"] = "最后执行",
            ["Tasks.LastError"] = "最后错误",
            ["Tasks.StartTask"] = "启动",
            ["Tasks.StopTask"] = "停止",
            ["Tasks.TriggerCollection"] = "手动采集",

            // 设备状态
            ["Devices.Title"] = "设备状态",
            ["Devices.Name"] = "设备名称",
            ["Devices.ConnectionStatus"] = "连接状态",
            ["Devices.ErrorCount"] = "错误次数",
            ["Devices.LastConnect"] = "最后连接",
            ["Devices.LastRead"] = "最后读取",
            ["Devices.LastError"] = "最后错误",
            ["Devices.TestConnection"] = "测试连接",
            ["Devices.Connected"] = "已连接",
            ["Devices.Disconnected"] = "已断开",
            ["Devices.Connecting"] = "连接中",
            ["Devices.Error"] = "错误",

            // 数据预览
            ["DataPreview.Title"] = "实时数据预览",
            ["DataPreview.AutoRefresh"] = "自动刷新",
            ["DataPreview.Clear"] = "清空",
            ["DataPreview.Time"] = "时间",
            ["DataPreview.TagId"] = "标签ID",
            ["DataPreview.TagName"] = "标签名称",
            ["DataPreview.RawValue"] = "原始值",
            ["DataPreview.Value"] = "转换值",
            ["DataPreview.Unit"] = "单位",
            ["DataPreview.Quality"] = "质量",

            // 诊断工具
            ["Diagnostics.Title"] = "诊断工具",
            ["Diagnostics.SelectDevice"] = "选择设备",
            ["Diagnostics.TestConnection"] = "测试连接",
            ["Diagnostics.ReadAllTags"] = "读取所有标签",
            ["Diagnostics.Results"] = "读取结果",
            ["Diagnostics.SelectedDeviceOps"] = "选中设备操作",
            ["Diagnostics.AllDeviceOps"] = "全部设备操作",
            ["Diagnostics.TestAllConnections"] = "全部测试连接",
            ["Diagnostics.ReadAllDevicesTags"] = "全部读取标签",
            ["Diagnostics.RefreshTooltip"] = "刷新设备列表",

            // 系统日志
            ["Logs.Title"] = "系统日志",
            ["Logs.Level"] = "级别",
            ["Logs.Message"] = "消息",
            ["Logs.Time"] = "时间",
            ["Logs.AutoScroll"] = "自动滚动",
            ["Logs.Clear"] = "清空",
            ["Logs.All"] = "全部",
            ["Logs.Search"] = "搜索:",
            ["Logs.SearchPlaceholder"] = "输入关键字过滤...",
            ["Logs.ShowCount"] = "显示 {0} / 共 {1} 条",
            ["Logs.RealtimeHint"] = "📋 实时日志 — 所有 Serilog 日志均在此显示",

            // 设备标签
            ["Devices.TagValues"] = "📋 标签实时值",
            ["Devices.CurrentValue"] = "当前值",

            // 系统设置
            ["Settings.Title"] = "系统设置",
            ["Settings.Node"] = "节点设置",
            ["Settings.NodeId"] = "节点ID",
            ["Settings.NodeName"] = "节点名称",
            ["Settings.Api"] = "API设置",
            ["Settings.GatewayUrl"] = "网关地址",
            ["Settings.RabbitMq"] = "消息队列设置",
            ["Settings.RabbitMqHost"] = "RabbitMQ地址",
            ["Settings.RabbitMqPort"] = "RabbitMQ端口",
            ["Settings.RabbitMqUser"] = "用户名",
            ["Settings.RabbitMqPassword"] = "密码",
            ["Settings.Language"] = "语言设置",
            ["Settings.DisplayLanguage"] = "显示语言",
            ["Settings.SaveSuccess"] = "设置已保存",
            ["Settings.SaveFailed"] = "保存设置失败",
            ["Settings.Heartbeat"] = "心跳间隔(秒)",
            ["Settings.GenerateNodeId"] = "生成",
            ["Settings.Reset"] = "重置",
            ["Settings.SaveConfig"] = "保存配置",
            ["Settings.Other"] = "其他配置",
            ["Settings.NodeIdPlaceholder"] = "节点唯一标识",
            ["Settings.NodeNamePlaceholder"] = "节点显示名称",
            ["Settings.GatewayUrlPlaceholder"] = "http://localhost:60620",
            ["Settings.PasswordPlaceholder"] = "密码",

            // 引擎状态
            ["Engine.Uninitialized"] = "未初始化",
            ["Engine.Configured"] = "已配置",
            ["Engine.Running"] = "运行中",
            ["Engine.Paused"] = "已暂停",
            ["Engine.Stopped"] = "已停止",
            ["Engine.Error"] = "错误",
            ["Engine.Unknown"] = "未知",

            // 仪表板
            ["Dashboard.RunningTasks"] = "🚀 运行中任务",
            ["Dashboard.ConnectedDevices"] = "🔗 已连接设备",
            ["Dashboard.TotalCollections"] = "📊 采集总次数",
            ["Dashboard.Errors"] = "⚠ 错误次数",
            ["Dashboard.TaskStatus"] = "📋 任务状态",
            ["Dashboard.DeviceStatus"] = "🖥 设备状态",
            ["Dashboard.RealtimePreview"] = "📊 实时数据预览",
            ["Dashboard.NoData"] = "暂无采集数据",
            ["Dashboard.LastUpdate"] = "最后更新",

            // DataGrid 通用 Header
            ["Header.TaskName"] = "任务名称",
            ["Header.Status"] = "状态",
            ["Header.CollectionCount"] = "采集次数",
            ["Header.ErrorCount"] = "错误次数",
            ["Header.LastRun"] = "最后执行",
            ["Header.LastError"] = "最后错误",
            ["Header.DeviceName"] = "设备名称",
            ["Header.ConnectionStatus"] = "连接状态",
            ["Header.LastConnect"] = "最后连接",
            ["Header.LastRead"] = "最后读取",
            ["Header.Time"] = "时间",
            ["Header.Device"] = "设备",
            ["Header.Tag"] = "标签",
            ["Header.TagName"] = "标签名称",
            ["Header.Value"] = "值",
            ["Header.RawValue"] = "原始值",
            ["Header.ConvertedValue"] = "转换值",
            ["Header.Quality"] = "质量",
            ["Header.Unit"] = "单位",
            ["Header.Address"] = "地址",
            ["Header.DataType"] = "数据类型",
            ["Header.UpdateTime"] = "更新时间",
            ["Header.Level"] = "级别",
            ["Header.Source"] = "来源",
            ["Header.Message"] = "消息",
            ["Header.Result"] = "结果",
            ["Header.Success"] = "成功",
            ["Header.Timestamp"] = "时间戳",
            ["Header.ErrorInfo"] = "错误信息",
        };

        // 初始化英文资源
        _resources["en-US"] = new Dictionary<string, string>
        {
            // Common
            ["App.Title"] = "DevDCP Data Collection Client",
            ["App.SubTitle"] = "Data Collection Client",
            ["Common.OK"] = "OK",
            ["Common.Cancel"] = "Cancel",
            ["Common.Save"] = "Save",
            ["Common.Close"] = "Close",
            ["Common.Refresh"] = "Refresh",
            ["Common.Start"] = "Start",
            ["Common.Stop"] = "Stop",
            ["Common.Delete"] = "Delete",
            ["Common.Edit"] = "Edit",
            ["Common.Add"] = "Add",
            ["Common.Search"] = "Search",
            ["Common.Loading"] = "Loading...",
            ["Common.Success"] = "Success",
            ["Common.Error"] = "Error",
            ["Common.Warning"] = "Warning",
            ["Common.Info"] = "Info",

            // Login
            ["Login.Title"] = "Please sign in to continue",
            ["Login.Username"] = "Username",
            ["Login.UsernamePlaceholder"] = "Enter username",
            ["Login.Password"] = "Password",
            ["Login.PasswordPlaceholder"] = "Enter password",
            ["Login.RememberMe"] = "Remember me",
            ["Login.ShowPassword"] = "Show password",
            ["Login.HidePassword"] = "Hide password",
            ["Login.Button"] = "Sign In",
            ["Login.LoggingIn"] = "Signing in...",
            ["Login.Failed"] = "Login failed",
            ["Login.InvalidCredentials"] = "Invalid username or password",
            ["Login.NetworkError"] = "Network error, please check server address",
            ["Login.ServerError"] = "Server error, please try again later",

            // MainWindow
            ["MainWindow.Title"] = "DevDCP Data Collection Client",
            ["MainWindow.Logout"] = "Logout",
            ["MainWindow.PullConfig"] = "Pull Config",
            ["MainWindow.ConfigChanged"] = "Server configuration updated, click to pull latest config",

            // Menu
            ["Menu.Dashboard"] = "Dashboard",
            ["Menu.Tasks"] = "Task Management",
            ["Menu.Devices"] = "Device Status",
            ["Menu.DataPreview"] = "Data Preview",
            ["Menu.Diagnostics"] = "Diagnostics",
            ["Menu.Logs"] = "System Logs",
            ["Menu.Settings"] = "Settings",

            // Dashboard
            ["Dashboard.Title"] = "Dashboard",
            ["Dashboard.TaskCount"] = "Tasks",
            ["Dashboard.DeviceCount"] = "Devices",
            ["Dashboard.TagCount"] = "Tags",
            ["Dashboard.CollectionRate"] = "Collection Rate",
            ["Dashboard.RunningTasks"] = "Running Tasks",
            ["Dashboard.RecentData"] = "Recent Data",

            // Tasks
            ["Tasks.Title"] = "Task Management",
            ["Tasks.Name"] = "Task Name",
            ["Tasks.Status"] = "Status",
            ["Tasks.CollectionCount"] = "Collections",
            ["Tasks.ErrorCount"] = "Errors",
            ["Tasks.LastRun"] = "Last Run",
            ["Tasks.LastError"] = "Last Error",
            ["Tasks.StartTask"] = "Start",
            ["Tasks.StopTask"] = "Stop",
            ["Tasks.TriggerCollection"] = "Manual Collection",

            // Devices
            ["Devices.Title"] = "Device Status",
            ["Devices.Name"] = "Device Name",
            ["Devices.ConnectionStatus"] = "Connection Status",
            ["Devices.ErrorCount"] = "Errors",
            ["Devices.LastConnect"] = "Last Connect",
            ["Devices.LastRead"] = "Last Read",
            ["Devices.LastError"] = "Last Error",
            ["Devices.TestConnection"] = "Test Connection",
            ["Devices.Connected"] = "Connected",
            ["Devices.Disconnected"] = "Disconnected",
            ["Devices.Connecting"] = "Connecting",
            ["Devices.Error"] = "Error",

            // DataPreview
            ["DataPreview.Title"] = "Real-time Data Preview",
            ["DataPreview.AutoRefresh"] = "Auto Refresh",
            ["DataPreview.Clear"] = "Clear",
            ["DataPreview.Time"] = "Time",
            ["DataPreview.TagId"] = "Tag ID",
            ["DataPreview.TagName"] = "Tag Name",
            ["DataPreview.RawValue"] = "Raw Value",
            ["DataPreview.Value"] = "Value",
            ["DataPreview.Unit"] = "Unit",
            ["DataPreview.Quality"] = "Quality",

            // Diagnostics
            ["Diagnostics.Title"] = "Diagnostics",
            ["Diagnostics.SelectDevice"] = "Select Device",
            ["Diagnostics.TestConnection"] = "Test Connection",
            ["Diagnostics.ReadAllTags"] = "Read All Tags",
            ["Diagnostics.Results"] = "Results",
            ["Diagnostics.SelectedDeviceOps"] = "Selected Device",
            ["Diagnostics.AllDeviceOps"] = "All Devices",
            ["Diagnostics.TestAllConnections"] = "Test All Connections",
            ["Diagnostics.ReadAllDevicesTags"] = "Read All Tags",
            ["Diagnostics.RefreshTooltip"] = "Refresh device list",

            // Logs
            ["Logs.Title"] = "System Logs",
            ["Logs.Level"] = "Level",
            ["Logs.Message"] = "Message",
            ["Logs.Time"] = "Time",
            ["Logs.AutoScroll"] = "Auto Scroll",
            ["Logs.Clear"] = "Clear",
            ["Logs.All"] = "All",
            ["Logs.Search"] = "Search:",
            ["Logs.SearchPlaceholder"] = "Filter by keyword...",
            ["Logs.ShowCount"] = "Showing {0} / Total {1}",
            ["Logs.RealtimeHint"] = "📋 Real-time Logs — All Serilog logs displayed here",

            // Device Tags
            ["Devices.TagValues"] = "📋 Tag Real-time Values",
            ["Devices.CurrentValue"] = "Current Value",

            // Settings
            ["Settings.Title"] = "Settings",
            ["Settings.Node"] = "Node Settings",
            ["Settings.NodeId"] = "Node ID",
            ["Settings.NodeName"] = "Node Name",
            ["Settings.Api"] = "API Settings",
            ["Settings.GatewayUrl"] = "Gateway URL",
            ["Settings.RabbitMq"] = "Message Queue Settings",
            ["Settings.RabbitMqHost"] = "RabbitMQ Host",
            ["Settings.RabbitMqPort"] = "RabbitMQ Port",
            ["Settings.RabbitMqUser"] = "Username",
            ["Settings.RabbitMqPassword"] = "Password",
            ["Settings.Language"] = "Language",
            ["Settings.DisplayLanguage"] = "Display Language",
            ["Settings.SaveSuccess"] = "Settings saved",
            ["Settings.SaveFailed"] = "Failed to save settings",
            ["Settings.Heartbeat"] = "Heartbeat Interval(s)",
            ["Settings.GenerateNodeId"] = "Generate",
            ["Settings.Reset"] = "Reset",
            ["Settings.SaveConfig"] = "Save Config",
            ["Settings.Other"] = "Other Settings",
            ["Settings.NodeIdPlaceholder"] = "Unique node identifier",
            ["Settings.NodeNamePlaceholder"] = "Node display name",
            ["Settings.GatewayUrlPlaceholder"] = "http://localhost:60620",
            ["Settings.PasswordPlaceholder"] = "Password",

            // Engine State
            ["Engine.Uninitialized"] = "Uninitialized",
            ["Engine.Configured"] = "Configured",
            ["Engine.Running"] = "Running",
            ["Engine.Paused"] = "Paused",
            ["Engine.Stopped"] = "Stopped",
            ["Engine.Error"] = "Error",
            ["Engine.Unknown"] = "Unknown",

            // Dashboard
            ["Dashboard.RunningTasks"] = "🚀 Running Tasks",
            ["Dashboard.ConnectedDevices"] = "🔗 Connected Devices",
            ["Dashboard.TotalCollections"] = "📊 Total Collections",
            ["Dashboard.Errors"] = "⚠ Errors",
            ["Dashboard.TaskStatus"] = "📋 Task Status",
            ["Dashboard.DeviceStatus"] = "🖥 Device Status",
            ["Dashboard.RealtimePreview"] = "📊 Real-time Data Preview",
            ["Dashboard.NoData"] = "No data collected yet",
            ["Dashboard.LastUpdate"] = "Last Update",

            // DataGrid Headers
            ["Header.TaskName"] = "Task Name",
            ["Header.Status"] = "Status",
            ["Header.CollectionCount"] = "Collections",
            ["Header.ErrorCount"] = "Errors",
            ["Header.LastRun"] = "Last Run",
            ["Header.LastError"] = "Last Error",
            ["Header.DeviceName"] = "Device Name",
            ["Header.ConnectionStatus"] = "Connection",
            ["Header.LastConnect"] = "Last Connect",
            ["Header.LastRead"] = "Last Read",
            ["Header.Time"] = "Time",
            ["Header.Device"] = "Device",
            ["Header.Tag"] = "Tag",
            ["Header.TagName"] = "Tag Name",
            ["Header.Value"] = "Value",
            ["Header.RawValue"] = "Raw Value",
            ["Header.ConvertedValue"] = "Converted",
            ["Header.Quality"] = "Quality",
            ["Header.Unit"] = "Unit",
            ["Header.Address"] = "Address",
            ["Header.DataType"] = "Data Type",
            ["Header.UpdateTime"] = "Updated",
            ["Header.Level"] = "Level",
            ["Header.Source"] = "Source",
            ["Header.Message"] = "Message",
            ["Header.Result"] = "Result",
            ["Header.Success"] = "Success",
            ["Header.Timestamp"] = "Timestamp",
            ["Header.ErrorInfo"] = "Error Info",
        };
    }

    /// <summary>
    /// 当前语言
    /// </summary>
    public static string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            // 防御性：value 可能为 null（配置未设置），避免传入 null 给字典
            if (string.IsNullOrWhiteSpace(value)) return;

            if (_currentLanguage != value && _resources.ContainsKey(value))
            {
                _currentLanguage = value;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// 支持的语言列表
    /// </summary>
    public static IReadOnlyList<LanguageInfo> SupportedLanguages { get; } = new List<LanguageInfo>
    {
        new("zh-CN", "简体中文"),
        new("en-US", "English"),
    };

    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    public static string GetString(string key)
    {
        if (_resources.TryGetValue(_currentLanguage, out var langResources))
        {
            if (langResources.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        
        // 回退到中文
        if (_resources.TryGetValue("zh-CN", out var defaultResources))
        {
            if (defaultResources.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return key; // 找不到则返回key本身
    }

    /// <summary>
    /// 简写方法
    /// </summary>
    public static string T(string key) => GetString(key);
}

/// <summary>
/// 语言信息
/// </summary>
public record LanguageInfo(string Code, string DisplayName);
