using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Collector.Agent.Legacy.Services;
using Collector.Core.Models;

namespace Collector.Agent.Legacy.Forms
{
    public partial class TaskDetailForm : Form
    {
        private readonly TaskConfig _task;
        private readonly List<DeviceConfig> _devices;
        
        private Label _taskNameValueLabel;
        private Label _taskCodeValueLabel;
        private Label _taskTypeValueLabel;
        private Label _intervalValueLabel;
        private Label _priorityValueLabel;
        private Label _statusValueLabel;

        public TaskDetailForm(TaskConfig task, List<DeviceConfig> devices)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _devices = devices ?? new List<DeviceConfig>();
            
            InitializeComponent();
            InitializeText();
            InitializeConfigTab();
            InitializeDevicesTab();
            LoadTaskInfo();
            LoadDevices();
        }

        private void InitializeText()
        {
            this.Text = L.T("TaskDetailForm_Title");
            _configTab.Text = L.T("TaskDetailForm_Tab_Config");
            _devicesTab.Text = L.T("TaskDetailForm_Tab_Devices");
            _closeButton.Text = L.T("TaskDetailForm_Button_Close");
        }

        private void InitializeConfigTab()
        {
            int y = 20;
            int labelWidth = 100;
            int valueWidth = 350;

            AddLabelPair(_configTab, L.T("TaskDetailForm_Label_TaskName"), ref _taskNameValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_configTab, L.T("TaskDetailForm_Label_TaskCode"), ref _taskCodeValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_configTab, L.T("TaskDetailForm_Label_TaskType"), ref _taskTypeValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_configTab, L.T("TaskDetailForm_Label_Interval"), ref _intervalValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_configTab, L.T("TaskDetailForm_Label_Priority"), ref _priorityValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_configTab, L.T("TaskDetailForm_Label_Status"), ref _statusValueLabel, 20, y, labelWidth, valueWidth);
        }

        private void InitializeDevicesTab()
        {
            _devicesListView.Columns.Add(L.T("TaskDetailForm_Col_DeviceName"), 200);
            _devicesListView.Columns.Add(L.T("TaskDetailForm_Col_Protocol"), 120);
            _devicesListView.Columns.Add(L.T("TaskDetailForm_Col_Status"), 120);
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

        private void LoadTaskInfo()
        {
            _taskNameValueLabel.Text = _task.Name ?? "-";
            _taskCodeValueLabel.Text = _task.Code ?? "-";
            _taskTypeValueLabel.Text = GetTaskTypeDisplayText(_task.TaskType);
            _intervalValueLabel.Text = _task.DefaultInterval.HasValue && _task.DefaultInterval > 0 ? $"{_task.DefaultInterval} ms" : "-";
            _priorityValueLabel.Text = _task.Priority.ToString();
            _statusValueLabel.Text = _task.IsEnabled ? L.T("Common_Enabled") : L.T("Common_Disabled");
            _statusValueLabel.ForeColor = _task.IsEnabled ? Color.Green : Color.Gray;
        }

        /// <summary>
        /// 获取任务类型显示文本（翻译）
        /// </summary>
        private string GetTaskTypeDisplayText(string taskType)
        {
            if (string.IsNullOrEmpty(taskType))
                return "-";

            // 根据任务类型返回翻译后的文本
            switch (taskType.ToLower())
            {
                case "periodic":
                    return L.T("TaskType_Periodic");
                case "scheduled":
                case "cron":
                    return L.T("TaskType_Scheduled");
                case "event":
                    return L.T("TaskType_Event");
                case "hybrid":
                    return L.T("TaskType_Hybrid");
                default:
                    // 如果类型未知，返回原始值
                    return taskType;
            }
        }

        private void LoadDevices()
        {
            _devicesListView.Items.Clear();

            if (_devices == null || _devices.Count == 0)
                return;

            foreach (var device in _devices)
            {
                var item = new ListViewItem(device.DeviceName ?? "-");
                item.SubItems.Add(device.ProtocolType ?? "-");
                item.SubItems.Add(device.Enabled ? L.T("Common_Enabled") : L.T("Common_Disabled"));
                item.Tag = device;
                _devicesListView.Items.Add(item);
            }
        }
    }
}
