using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Collector.Agent.Legacy.Services;
using Collector.Core.ApiClient;
using Collector.Core.Engine;
using Collector.Core.Models;
using NLog;

namespace Collector.Agent.Legacy.Forms
{
    public partial class MainForm : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppService _appService;
        private readonly SignalRService _signalRService;
        private readonly ICollectionEngine _collectionEngine;
        private readonly IAdminApiClient _adminApiClient;
        private System.Windows.Forms.Timer _statusTimer;
        private long _dataPointsProcessed;
        private DateTime? _lastCollectionTime;

        // 用于保护配置刷新和采集之间的同步
        private readonly SemaphoreSlim _configRefreshLock = new SemaphoreSlim(1, 1);

        private Label _nodeIdValueLabel;
        private Label _nodeNameValueLabel;
        private Label _platformValueLabel;
        private Label _statusValueLabel;

        private Label _deviceCountValueLabel;
        private Label _taskCountValueLabel;
        private Label _dataPointsValueLabel;
        private Label _lastCollectionValueLabel;

        // 当前配置数据
        private NodeConfig _currentConfig;

        public MainForm()
        {
            _appService = AppService.Instance;
            _signalRService = new SignalRService();
            _collectionEngine = ServiceLocator.GetService<ICollectionEngine>();
            _adminApiClient = ServiceLocator.GetService<IAdminApiClient>();
            InitializeComponent();
            InitializeText();
            InitializeMenus();
            InitializeListViews();
            InitializeLabels();
            InitializeEvents();
            InitializeCollectionEngineEvents();
        }

        /// <summary>
        /// 初始化采集引擎事件
        /// </summary>
        private void InitializeCollectionEngineEvents()
        {
            // 订阅数据采集事件
            _collectionEngine.DataCollected += OnDataCollected;
            
            // 订阅错误事件
            _collectionEngine.ErrorOccurred += OnErrorOccurred;
            
            // 订阅设备状态变化事件
            _collectionEngine.DeviceStatusChanged += OnDeviceStatusChanged;
            
            // 订阅引擎状态变化事件
            _collectionEngine.StateChanged += OnEngineStateChanged;
        }

        private void OnDataCollected(object sender, CollectionData data)
        {
            foreach (var dp in data.DataPoints)
            {
                _dataPointsProcessed++;
                _lastCollectionTime = data.Timestamp.LocalDateTime;
                
                // 控制 UI 更新频率
                if (_dataPointsProcessed % 10 == 0 || _dataPointsProcessed < 10)
                {
                    UpdateStatisticsUI();
                }
                
                var formattedValue = FormatTagValue(dp.Value);
                var deviceName = _currentConfig?.Devices?
                    .FirstOrDefault(d => d.Id == data.DeviceId)?.DeviceName ?? data.DeviceCode;
                
                AppendLog(string.Format(L.T("MainForm_Log_TagValueCollected"), 
                    deviceName, dp.TagName, formattedValue), LogLevel.Info);
            }
        }

        private void OnErrorOccurred(object sender, EngineErrorEventArgs e)
        {
            var deviceName = "引擎";
            if (e.DeviceId.HasValue && _currentConfig != null)
            {
                deviceName = _currentConfig.Devices?
                    .FirstOrDefault(d => d.Id == e.DeviceId)?.DeviceName ?? e.DeviceId.ToString();
            }
            
            AppendLog(string.Format(L.T("MainForm_Log_CollectionError"),
                deviceName, e.Message), LogLevel.Error);
        }

        private void OnDeviceStatusChanged(object sender, DeviceRuntimeStatus status)
        {
            var stateText = status.State.ToString();
            var isConnected = status.State == DeviceConnectionState.Connected;
            var logLevel = isConnected ? LogLevel.Info : LogLevel.Warn;
            var statusKey = isConnected ? "MainForm_Log_DeviceConnected" : "MainForm_Log_DeviceDisconnected";
            AppendLog(string.Format(L.T(statusKey), status.DeviceName), logLevel);
            
            UpdateDeviceStatus(status.DeviceId.ToString(), stateText);
        }

        private void OnEngineStateChanged(object sender, EngineState state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnEngineStateChanged(sender, state)));
                return;
            }

            var stateText = state.ToString();
            _statusValueLabel.Text = stateText;
            
            var isRunning = state == EngineState.Running;
            _startButton.Enabled = !isRunning;
            _stopButton.Enabled = isRunning;
            _statusValueLabel.ForeColor = isRunning ? Color.Green : Color.Gray;
        }

        /// <summary>
        /// 格式化标签值
        /// 如果值是数组，返回格式化的数组字符串；否则返回原值的字符串表示
        /// 格式示例：
        /// - 单个值: "11"
        /// - 数组: "[11, 22, 33, 44, 55]" （每行最多5个值，超过则换行）
        /// </summary>
        private string FormatTagValue(object value)
        {
            if (value == null)
                return "[NULL]";

            // 检查是否为数组
            if (value is Array array)
            {
                var items = new System.Collections.Generic.List<string>();
                for (int i = 0; i < array.Length; i++)
                {
                    items.Add(array.GetValue(i)?.ToString() ?? "[NULL]");
                }

                // 如果数组很长，每行显示不超过5个值并换行
                if (items.Count > 5)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append("[");
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(", ");
                            // 每5个值换一行
                            if ((i + 1) % 5 == 0 && i < items.Count - 1)
                            {
                                sb.Append("\n  ");
                            }
                        }
                        sb.Append(items[i]);
                    }
                    sb.Append("]");
                    return sb.ToString();
                }
                else
                {
                    return "[" + string.Join(", ", items) + "]";
                }
            }

            return value.ToString() ?? "[NULL]";
        }


        private void UpdateStatisticsUI()
        {
            if (IsDisposed || _dataPointsValueLabel.IsDisposed)
                return;

            if (_dataPointsValueLabel.InvokeRequired)
            {
                try
                {
                    _dataPointsValueLabel.BeginInvoke(new Action(UpdateStatisticsUI));
                }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
                return;
            }

            _dataPointsValueLabel.Text = _dataPointsProcessed.ToString("N0");
            if (_lastCollectionTime.HasValue)
            {
                _lastCollectionValueLabel.Text = _lastCollectionTime.Value.ToString("HH:mm:ss");
            }
        }

        private void UpdateDeviceStatus(string deviceId, string state)
        {
            if (IsDisposed || _devicesListView.IsDisposed)
                return;

            if (_devicesListView.InvokeRequired)
            {
                try
                {
                    _devicesListView.BeginInvoke(new Action(() => UpdateDeviceStatus(deviceId, state)));
                }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
                return;
            }

            foreach (ListViewItem item in _devicesListView.Items)
            {
                var device = item.Tag as DeviceConfig;
                if (device != null && device.Id.ToString() == deviceId)
                {
                    item.SubItems[3].Text = state;
                    break;
                }
            }
        }

        private void InitializeText()
        {
            // 窗体标题
            this.Text = L.T("MainForm_Title");
            
            // GroupBox 标题
            _nodeInfoGroup.Text = L.T("MainForm_Group_NodeInfo");
            _statisticsGroup.Text = L.T("MainForm_Group_Statistics");
            _devicesGroup.Text = L.T("MainForm_Group_Devices");
            _tasksGroup.Text = L.T("MainForm_Group_Tasks");
            _logGroup.Text = L.T("MainForm_Group_Log");
            
            // 按钮
            _startButton.Text = L.T("MainForm_Button_Start");
            _stopButton.Text = L.T("MainForm_Button_Stop");
            _refreshButton.Text = L.T("MainForm_Button_Refresh");
            
            // 状态栏
            _connectionStatusLabel.Text = L.T("MainForm_Status_Disconnected");
        }

        private void InitializeMenus()
        {
            var fileMenu = new ToolStripMenuItem(L.T("MainForm_Menu_File"));
            fileMenu.DropDownItems.Add(L.T("MainForm_Menu_Settings"), null, (s, e) => ShowSettings());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(L.T("MainForm_Menu_Exit"), null, (s, e) => this.Close());
            _menuStrip.Items.Add(fileMenu);

            var taskMenu = new ToolStripMenuItem(L.T("MainForm_Menu_Task"));
            taskMenu.DropDownItems.Add(L.T("MainForm_Menu_RefreshConfig"), null, async (s, e) => await RefreshConfig());
            taskMenu.DropDownItems.Add(L.T("MainForm_Menu_StartCollection"), null, async (s, e) => await StartCollection());
            taskMenu.DropDownItems.Add(L.T("MainForm_Menu_StopCollection"), null, async (s, e) => await StopCollection());
            _menuStrip.Items.Add(taskMenu);

            var helpMenu = new ToolStripMenuItem(L.T("MainForm_Menu_Help"));
            helpMenu.DropDownItems.Add(L.T("MainForm_Menu_About"), null, (s, e) => ShowAbout());
            _menuStrip.Items.Add(helpMenu);
        }

        private void InitializeListViews()
        {
            // 初始化设备列表
            _devicesListView.Columns.Add(L.T("MainForm_Col_DeviceName"), 150);
            _devicesListView.Columns.Add(L.T("MainForm_Col_Protocol"), 80);
            _devicesListView.Columns.Add(L.T("MainForm_Col_IpAddress"), 100);
            _devicesListView.Columns.Add(L.T("MainForm_Col_Status"), 60);
            _devicesListView.DoubleClick += DevicesListView_DoubleClick;

            // 初始化任务列表
            _tasksListView.Columns.Add(L.T("MainForm_Col_TaskName"), 150);
            _tasksListView.Columns.Add(L.T("MainForm_Col_TaskCode"), 100);
            _tasksListView.Columns.Add(L.T("MainForm_Col_CollectInterval"), 80);
            _tasksListView.Columns.Add(L.T("MainForm_Col_Status"), 60);
            _tasksListView.DoubleClick += TasksListView_DoubleClick;
        }

        private void InitializeLabels()
        {
            int y = 25;
            int labelWidth = 80;
            int valueWidth = 150;

            AddLabelPair(_nodeInfoGroup, L.T("MainForm_Label_NodeId"), ref _nodeIdValueLabel, 15, y, labelWidth, valueWidth);
            AddLabelPair(_nodeInfoGroup, L.T("MainForm_Label_Platform"), ref _platformValueLabel, 220, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_nodeInfoGroup, L.T("MainForm_Label_NodeName"), ref _nodeNameValueLabel, 15, y, labelWidth, valueWidth);
            AddLabelPair(_nodeInfoGroup, L.T("MainForm_Label_Status"), ref _statusValueLabel, 220, y, labelWidth, valueWidth);

            y = 25;
            AddLabelPair(_statisticsGroup, L.T("MainForm_Label_Devices"), ref _deviceCountValueLabel, 15, y, labelWidth, 100);
            AddLabelPair(_statisticsGroup, L.T("MainForm_Label_Tasks"), ref _taskCountValueLabel, 220, y, labelWidth, 100);
            y += 30;
            AddLabelPair(_statisticsGroup, L.T("MainForm_Label_DataPoints"), ref _dataPointsValueLabel, 15, y, labelWidth, 100);
            AddLabelPair(_statisticsGroup, L.T("MainForm_Label_LastCollection"), ref _lastCollectionValueLabel, 220, y, labelWidth, 180);
        }

        private void InitializeEvents()
        {
            _startButton.Click += async (s, e) => await StartCollection();
            _stopButton.Click += async (s, e) => await StopCollection();
            _refreshButton.Click += async (s, e) =>
            {
                // 禁止在采集过程中刷新配置
                if (_collectionEngine.State == EngineState.Running)
                {
                    MessageBox.Show(
                        L.T("MainForm_Msg_CannotRefreshWhileCollecting"),
                        L.T("Msg_Warning"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                await RefreshConfig();
            };

            _statusTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _statusTimer.Tick += StatusTimer_Tick;

            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;
        }

        private void AddLabelPair(Control parent, string labelText, ref Label valueLabel, int x, int y, int labelWidth, int valueWidth)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(labelWidth, 20)
            };
            parent.Controls.Add(label);

            valueLabel = new Label
            {
                Text = "-",
                Location = new Point(x + labelWidth, y),
                Size = new Size(valueWidth, 20),
                ForeColor = Color.Blue
            };
            parent.Controls.Add(valueLabel);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            AppendLog(L.T("Log_MainFormLoaded"), LogLevel.Info);
            UpdateNodeInfo();
            
            _connectionStatusLabel.Text = L.T("MainForm_Status_Polling");
            _connectionStatusLabel.ForeColor = Color.Orange;
            AppendLog(L.T("Log_SignalRDisabled"), LogLevel.Warn);
            
            await RefreshConfig();
            _statusTimer.Start();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_collectionEngine.State == EngineState.Running)
            {
                var result = MessageBox.Show(
                    L.T("MainForm_Msg_ConfirmExit"),
                    L.T("MainForm_Msg_ConfirmExitTitle"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    _collectionEngine.StopAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "停止采集引擎失败");
                }
            }

            _statusTimer?.Stop();
            _statusTimer?.Dispose();
        }

        private int _configPollCounter = 0;
        
        private async void StatusTimer_Tick(object sender, EventArgs e)
        {
            // 心跳始终发送，无论是否在采集中
            await SendHeartbeat();
            UpdateStatistics();
            
            // 只有在非采集状态下才进行配置轮询
            if (_collectionEngine.State != EngineState.Running)
            {
                _configPollCounter++;
                if (_configPollCounter >= 6)
                {
                    _configPollCounter = 0;
                    await CheckConfigUpdate();
                }
            }
        }
        
        private async Task CheckConfigUpdate()
        {
            try
            {
                await RefreshConfig();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "配置检查失败");
            }
        }

        private void UpdateNodeInfo()
        {
            _nodeIdValueLabel.Text = _appService.CurrentNodeId ?? "-";
            _nodeNameValueLabel.Text = _appService.CurrentNodeName ?? "-";
            _platformValueLabel.Text = "NET45";
            _statusValueLabel.Text = L.T("MainForm_Status_Online");
            _statusValueLabel.ForeColor = Color.Green;
        }

        private void UpdateStatistics()
        {
            _dataPointsValueLabel.Text = _dataPointsProcessed.ToString("N0");
            _lastCollectionValueLabel.Text = _lastCollectionTime?.ToString("HH:mm:ss") ?? "-";
            
            var isRunning = _collectionEngine.State == EngineState.Running;
            var runningCount = isRunning ? 1 : 0;
            var taskCount = _currentConfig?.Tasks?.Count ?? 0;
            _taskStatusLabel.Text = L.T("MainForm_TaskStatus", runningCount, taskCount);
        }

        private async Task RefreshConfig()
        {
            // 检查是否在采集过程中
            if (_collectionEngine.State == EngineState.Running)
            {
                AppendLog(L.T("Log_CannotRefreshConfigDuringCollection"), LogLevel.Warn);
                return;
            }

            // 获取互斥锁，确保配置刷新不会与采集并发进行
            if (!await _configRefreshLock.WaitAsync(1000))
            {
                AppendLog(L.T("Log_ConfigRefreshTimeout"), LogLevel.Warn);
                return;
            }

            try
            {
                // 再次检查采集状态，防止在等待锁期间采集已启动
                if (_collectionEngine.State == EngineState.Running)
                {
                    AppendLog(L.T("Log_CannotRefreshConfigDuringCollection"), LogLevel.Warn);
                    return;
                }

                AppendLog(L.T("Log_RefreshingConfig"), LogLevel.Info);
                var nodeId = _appService.CurrentNodeId;
                var response = await _adminApiClient.GetNodeConfigAsync(nodeId);
                
                if (response.IsSuccess && response.Data != null)
                {
                    _currentConfig = response.Data;
                    _deviceCountValueLabel.Text = _currentConfig.Devices?.Count.ToString() ?? "0";
                    _taskCountValueLabel.Text = _currentConfig.Tasks?.Count.ToString() ?? "0";
                    
                    // 更新设备和任务列表
                    RefreshDevicesList(_currentConfig.Devices);
                    RefreshTasksList(_currentConfig.Tasks);
                    
                    AppendLog(L.T("Log_ConfigRefreshSuccess") + $": {_currentConfig.Devices?.Count ?? 0} devices, {_currentConfig.Tasks?.Count ?? 0} tasks", LogLevel.Info);
                }
                else
                {
                    AppendLog(L.T("Log_ConfigRefreshFailed", response.Msg ?? "未知错误"), LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "刷新配置失败");
                AppendLog(L.T("Log_ConfigRefreshFailed", ex.Message), LogLevel.Error);
            }
            finally
            {
                _configRefreshLock.Release();
            }
        }

        private void RefreshDevicesList(List<DeviceConfig> devices)
        {
            _devicesListView.Items.Clear();
            if (devices == null) return;

            foreach (var device in devices)
            {
                var item = new ListViewItem(device.DeviceName ?? "-");
                item.SubItems.Add(device.ProtocolType ?? "-");
                
                // 从连接配置中获取 IP 地址
                var ipAddress = !string.IsNullOrEmpty(device.Connection?.IpAddress)
                    ? (device.Connection.Port > 0 ? $"{device.Connection.IpAddress}:{device.Connection.Port}" : device.Connection.IpAddress)
                    : "-";
                item.SubItems.Add(ipAddress);
                
                item.SubItems.Add(device.Enabled ? L.T("Common_Enabled") : L.T("Common_Disabled"));
                item.Tag = device;
                item.ForeColor = device.Enabled ? Color.Black : Color.Gray;
                _devicesListView.Items.Add(item);
            }
        }
        
        private void RefreshTasksList(List<TaskConfig> tasks)
        {
            _tasksListView.Items.Clear();
            if (tasks == null) return;

            foreach (var task in tasks)
            {
                var item = new ListViewItem(task.Name ?? "-");
                item.SubItems.Add(task.Code ?? "-");
                item.SubItems.Add(task.DefaultInterval.HasValue ? $"{task.DefaultInterval}ms" : "-");
                item.SubItems.Add(task.IsEnabled ? L.T("Common_Enabled") : L.T("Common_Disabled"));
                item.Tag = task;
                item.ForeColor = task.IsEnabled ? Color.Black : Color.Gray;
                _tasksListView.Items.Add(item);
            }
        }

        private void DevicesListView_DoubleClick(object sender, EventArgs e)
        {
            if (_devicesListView.SelectedItems.Count == 0) return;

            var selectedItem = _devicesListView.SelectedItems[0];
            var device = selectedItem.Tag as DeviceConfig;
            if (device == null) return;

            using (var detailForm = new DeviceDetailForm(device))
            {
                detailForm.ShowDialog(this);
            }
        }

        private void TasksListView_DoubleClick(object sender, EventArgs e)
        {
            if (_tasksListView.SelectedItems.Count == 0) return;

            var selectedItem = _tasksListView.SelectedItems[0];
            var task = selectedItem.Tag as TaskConfig;
            if (task == null) return;

            // 获取任务关联的设备
            var taskDevices = _currentConfig?.Devices?
                .Where(d => task.DeviceIds != null && task.DeviceIds.Contains(d.Id))
                .ToList() ?? new List<DeviceConfig>();

            using (var detailForm = new TaskDetailForm(task, taskDevices))
            {
                detailForm.ShowDialog(this);
            }
        }

        private async Task StartCollection()
        {
            try
            {
                _startButton.Enabled = false;
                
                // 获取互斥锁，防止在采集启动期间进行配置刷新
                if (!await _configRefreshLock.WaitAsync(5000))
                {
                    AppendLog(L.T("Log_CannotAcquireConfigLock"), LogLevel.Warn);
                    _startButton.Enabled = true;
                    return;
                }

                try
                {
                    AppendLog(L.T("MainForm_Log_StartingEngine"), LogLevel.Info);

                    if (_currentConfig == null)
                    {
                        AppendLog(L.T("MainForm_Log_NoDevicesToCollect"), LogLevel.Warn);
                        _startButton.Enabled = true;
                        return;
                    }

                    // 检查是否有启用的设备
                    var enabledDevices = _currentConfig.Devices?.Where(d => d.Enabled).ToList();
                    if (enabledDevices == null || enabledDevices.Count == 0)
                    {
                        AppendLog(L.T("MainForm_Log_NoDevicesToCollect"), LogLevel.Warn);
                        _startButton.Enabled = true;
                        return;
                    }

                    // 加载配置并启动引擎
                    await _collectionEngine.LoadConfigAsync(_currentConfig);
                    await _collectionEngine.StartAsync();

                    _lastCollectionTime = DateTime.Now;
                    _dataPointsProcessed = 0;

                    _stopButton.Enabled = true;
                    _statusValueLabel.Text = L.T("MainForm_Status_Collecting");
                    _statusValueLabel.ForeColor = Color.Green;
                    AppendLog(L.T("MainForm_Log_EngineStarted"), LogLevel.Info);
                }
                finally
                {
                    _configRefreshLock.Release();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "启动采集失败");
                AppendLog(L.T("MainForm_Log_EngineStartFailed", ex.Message), LogLevel.Error);
                _startButton.Enabled = true;
            }
        }

        private async Task StopCollection()
        {
            try
            {
                _stopButton.Enabled = false;
                AppendLog(L.T("MainForm_Log_StoppingEngine"), LogLevel.Info);

                await _collectionEngine.StopAsync();

                _startButton.Enabled = true;
                _statusValueLabel.Text = L.T("MainForm_Status_Stopped");
                _statusValueLabel.ForeColor = Color.Gray;
                AppendLog(L.T("MainForm_Log_EngineStopped"), LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "停止采集失败");
                AppendLog(L.T("MainForm_Log_EngineStopFailed", ex.Message), LogLevel.Error);
                _stopButton.Enabled = true;
            }
        }

        private async Task SendHeartbeat()
        {
            try
            {
                var nodeId = _appService.GetSetting("NodeId");
                if (string.IsNullOrEmpty(nodeId)) return;

                var request = new HeartbeatRequest
                {
                    Status = _collectionEngine.State == EngineState.Running ? "Online" : "Idle",
                    RunningTaskCount = _currentConfig?.Tasks?.Count(t => t.IsEnabled) ?? 0,
                    TotalCollectionCount = _dataPointsProcessed
                };

                await _adminApiClient.SendHeartbeatAsync(nodeId, request);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "心跳发送失败");
            }
        }

        private void ShowSettings()
        {
            using (var settingsForm = new SettingsForm())
            {
                settingsForm.ShowDialog(this);
            }
        }

        private void ShowAbout()
        {
            using (var aboutForm = new AboutForm())
            {
                aboutForm.ShowDialog(this);
            }
        }

        private void AppendLog(string message, LogLevel level)
        {
            // 防止窗口关闭/控件释放后仍尝试写入
            if (IsDisposed || _logTextBox.IsDisposed)
                return;

            if (_logTextBox.InvokeRequired)
            {
                try
                {
                    _logTextBox.BeginInvoke(new Action(() => AppendLog(message, level)));
                }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
                return;
            }

            var time = DateTime.Now.ToString("HH:mm:ss");
            var levelStr = level.ToString().ToUpper().PadRight(5);
            var color = GetLogColor(level);

            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.SelectionColor = color;
            _logTextBox.AppendText($"[{time}] [{levelStr}] {message}\n");
            _logTextBox.ScrollToCaret();

            if (_logTextBox.Lines.Length > 500)
            {
                var lines = _logTextBox.Lines;
                var newLines = new string[lines.Length - 100];
                Array.Copy(lines, 100, newLines, 0, newLines.Length);
                _logTextBox.Lines = newLines;
            }
        }

        private Color GetLogColor(LogLevel level)
        {
            switch (level.Name)
            {
                case "Error":
                case "Fatal":
                    return Color.Red;
                case "Warn":
                    return Color.Yellow;
                case "Info":
                    return Color.LightGreen;
                default:
                    return Color.White;
            }
        }
    }
}
