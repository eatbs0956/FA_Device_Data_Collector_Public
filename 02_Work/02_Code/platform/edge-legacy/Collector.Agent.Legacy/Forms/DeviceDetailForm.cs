using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Collector.Agent.Legacy.Services;
using Collector.Core.Drivers;
using Collector.Core.Engine;
using Collector.Core.Models;
using NLog;

namespace Collector.Agent.Legacy.Forms
{
    /// <summary>
    /// 设备详情对话框
    /// </summary>
    public partial class DeviceDetailForm : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly DeviceConfig _device;
        private readonly ICollectionEngine _collectionEngine;
        
        private Label _deviceNameValueLabel;
        private Label _protocolValueLabel;
        private Label _ipValueLabel;
        private Label _portValueLabel;
        private Label _statusValueLabel;

        public DeviceDetailForm(DeviceConfig device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _collectionEngine = ServiceLocator.GetService<ICollectionEngine>();
            
            InitializeComponent();
            InitializeText();
            InitializeBasicInfoTab();
            InitializeTagsTab();
            InitializeEvents();
            LoadDeviceInfo();
            LoadTags();
        }

        private void InitializeText()
        {
            this.Text = L.T("DeviceDetailForm_Title");
            _basicInfoTab.Text = L.T("DeviceDetailForm_Tab_Basic");
            _tagsTab.Text = L.T("DeviceDetailForm_Tab_Tags");
            _testConnectionButton.Text = L.T("DeviceDetailForm_Button_TestConnection");
            _testCollectionButton.Text = L.T("DeviceDetailForm_Button_TestCollection");
            _closeButton.Text = L.T("DeviceDetailForm_Button_Close");
        }

        private void InitializeEvents()
        {
            _testConnectionButton.Click += async (s, e) => await TestConnection();
            _testCollectionButton.Click += async (s, e) => await TestCollection();
        }

        private void InitializeBasicInfoTab()
        {
            int y = 20;
            int labelWidth = 100;
            int valueWidth = 300;

            AddLabelPair(_basicInfoTab, L.T("DeviceDetailForm_Label_DeviceName"), ref _deviceNameValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_basicInfoTab, L.T("DeviceDetailForm_Label_Protocol"), ref _protocolValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_basicInfoTab, L.T("DeviceDetailForm_Label_IP"), ref _ipValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_basicInfoTab, L.T("DeviceDetailForm_Label_Port"), ref _portValueLabel, 20, y, labelWidth, valueWidth);
            y += 30;
            AddLabelPair(_basicInfoTab, L.T("DeviceDetailForm_Label_Status"), ref _statusValueLabel, 20, y, labelWidth, valueWidth);
        }

        private void InitializeTagsTab()
        {
            _tagsListView.Columns.Add(L.T("DeviceDetailForm_Col_TagName"), 150);
            _tagsListView.Columns.Add(L.T("DeviceDetailForm_Col_Address"), 150);
            _tagsListView.Columns.Add(L.T("DeviceDetailForm_Col_DataType"), 100);
            _tagsListView.Columns.Add(L.T("DeviceDetailForm_Col_Value"), 100);
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

        private void LoadDeviceInfo()
        {
            _deviceNameValueLabel.Text = _device.DeviceName ?? "-";
            _protocolValueLabel.Text = _device.ProtocolType ?? "-";
            
            // 从 ConnectionConfig 中获取 IP 和端口
            var ip = !string.IsNullOrEmpty(_device.Connection?.IpAddress) ? _device.Connection.IpAddress : "-";
            var port = _device.Connection != null && _device.Connection.Port > 0 ? _device.Connection.Port.ToString() : "-";
            
            _ipValueLabel.Text = ip;
            _portValueLabel.Text = port;
            _statusValueLabel.Text = L.T("DeviceDetailForm_Status_Disconnected");
            _statusValueLabel.ForeColor = Color.Red;
        }

        private void LoadTags()
        {
            _tagsListView.Items.Clear();

            if (_device.Tags == null || _device.Tags.Count == 0)
                return;

            foreach (var tag in _device.Tags)
            {
                var item = new ListViewItem(tag.TagName ?? "-");
                item.SubItems.Add(FormatTagAddress(tag.TagAddress));
                item.SubItems.Add(tag.DataType.ToString());
                item.SubItems.Add("-"); // 当前值默认为空
                item.Tag = tag;
                _tagsListView.Items.Add(item);
            }
        }

        /// <summary>
        /// 格式化标签地址显示（解析 JSON 格式）
        /// </summary>
        private string FormatTagAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                return "-";
                
            // 尝试解析 JSON 格式
            if (address.TrimStart().StartsWith("{"))
            {
                try
                {
                    var parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(address);
                    if (parsed != null && parsed.ContainsKey("functionCode"))
                    {
                        // Modbus 地址格式：FC03 Addr:0 Qty:1（不显示 slaveId，因为它在设备级别配置）
                        var fc = parsed.ContainsKey("functionCode") ? parsed["functionCode"]?.ToString() : "03";
                        var addr = parsed.ContainsKey("address") ? parsed["address"]?.ToString() : "0";
                        var qty = parsed.ContainsKey("quantity") ? parsed["quantity"]?.ToString() : "1";
                        return $"FC{fc} Addr:{addr} Qty:{qty}";
                    }
                }
                catch
                {
                    // 解析失败，返回原始地址
                }
            }
            
            return address;
        }

        /// <summary>
        /// 格式化标签值用于显示
        /// 如果值是数组，显示为格式化的数组字符串；否则返回原值的字符串表示
        /// 数组显示规则：若超过5个元素，则换行显示
        /// </summary>
        private string FormatTagValueForDisplay(object value)
        {
            if (value == null)
                return "-";

            // 检查是否为数组
            if (value is Array array)
            {
                var items = new System.Collections.Generic.List<string>();
                for (int i = 0; i < array.Length; i++)
                {
                    items.Add(array.GetValue(i)?.ToString() ?? "[NULL]");
                }

                // 如果数组较短，单行显示
                if (items.Count <= 5)
                {
                    return "[" + string.Join(", ", items) + "]";
                }

                // 对于较长的数组，使用简洁格式：显示前5个值和总数
                var displayItems = items.GetRange(0, Math.Min(5, items.Count));
                return "[" + string.Join(", ", displayItems) + (items.Count > 5 ? $", ... ({items.Count} total)" : "") + "]";
            }

            return value.ToString() ?? "-";
        }

        private async Task TestConnection()
        {
            _testConnectionButton.Enabled = false;
            _statusLabel.Text = L.T("DeviceDetailForm_Msg_TestingConnection");
            
            try
            {
                var result = await _collectionEngine.TestDeviceConnectionAsync(_device);
                
                if (result.Success)
                {
                    _statusValueLabel.Text = L.T("DeviceDetailForm_Status_Connected");
                    _statusValueLabel.ForeColor = Color.Green;
                    _statusLabel.Text = L.T("DeviceDetailForm_Msg_ConnectionSuccess");
                    
                    MessageBox.Show(
                        L.T("DeviceDetailForm_Msg_ConnectionSuccess"),
                        L.T("Msg_Info"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    _statusValueLabel.Text = L.T("DeviceDetailForm_Status_Disconnected");
                    _statusValueLabel.ForeColor = Color.Red;
                    _statusLabel.Text = L.T("DeviceDetailForm_Msg_ConnectionFailed", result.ErrorMessage);
                    
                    MessageBox.Show(
                        L.T("DeviceDetailForm_Msg_ConnectionFailed", result.ErrorMessage),
                        L.T("Msg_Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "连接测试失败");
                _statusValueLabel.Text = L.T("DeviceDetailForm_Status_Disconnected");
                _statusValueLabel.ForeColor = Color.Red;
                _statusLabel.Text = L.T("DeviceDetailForm_Msg_ConnectionFailed", ex.Message);
                
                MessageBox.Show(
                    L.T("DeviceDetailForm_Msg_ConnectionFailed", ex.Message),
                    L.T("Msg_Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _testConnectionButton.Enabled = true;
            }
        }

        private async Task TestCollection()
        {
            _testCollectionButton.Enabled = false;
            _statusLabel.Text = L.T("DeviceDetailForm_Msg_Collecting");
            
            try
            {
                Logger.Info("开始单次采集测试，设备: {0}", _device.DeviceName);
                
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var tagResults = await _collectionEngine.ReadDeviceTagsAsync(_device);
                sw.Stop();
                
                Logger.Info("采集完成，结果数量: {0}", tagResults?.Count ?? 0);
                
                if (tagResults != null && tagResults.Count > 0)
                {
                    // 确保在 UI 线程更新 ListView
                    if (_tagsListView.InvokeRequired)
                    {
                        _tagsListView.Invoke(new Action(() => UpdateTagValues(tagResults)));
                    }
                    else
                    {
                        UpdateTagValues(tagResults);
                    }
                    
                    var successCount = tagResults.Count(r => r.Success);
                    _statusLabel.Text = L.T("DeviceDetailForm_Msg_CollectionSuccess", successCount);
                    
                    MessageBox.Show(
                        string.Format(L.T("DeviceDetailForm_Msg_CollectionComplete"), successCount, tagResults.Count, sw.ElapsedMilliseconds),
                        L.T("Msg_Info"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    _statusLabel.Text = L.T("DeviceDetailForm_Msg_CollectionFailed", "无数据返回");
                    
                    MessageBox.Show(
                        L.T("DeviceDetailForm_Msg_CollectionFailed", "无数据返回"),
                        L.T("Msg_Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "单次采集失败");
                _statusLabel.Text = L.T("DeviceDetailForm_Msg_CollectionFailed", ex.Message);
                
                MessageBox.Show(
                    L.T("DeviceDetailForm_Msg_CollectionFailed", ex.Message),
                    L.T("Msg_Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _testCollectionButton.Enabled = true;
            }
        }

        /// <summary>
        /// 更新标签的当前值（必须在 UI 线程调用）
        /// </summary>
        private void UpdateTagValues(List<TagReadResult> tagResults)
        {
            Logger.Debug("开始更新 ListView，标签结果数: {0}, ListView 项数: {1}", 
                tagResults.Count, _tagsListView.Items.Count);
            
            foreach (var tagResult in tagResults)
            {
                var updated = false;
                
                foreach (ListViewItem item in _tagsListView.Items)
                {
                    var tag = item.Tag as TagConfig;
                    if (tag != null)
                    {
                        var matched = (!string.IsNullOrEmpty(tag.TagName) && tag.TagName == tagResult.TagName) ||
                                     (!string.IsNullOrEmpty(tag.TagId) && tag.TagId == tagResult.TagId);
                        
                        if (matched)
                        {
                            var displayValue = tagResult.Success ? 
                                FormatTagValueForDisplay(tagResult.Value) : 
                                $"{L.T("DeviceDetailForm_Col_Error")}: {tagResult.ErrorMessage}";
                            
                            item.SubItems[3].Text = displayValue;
                            
                            Logger.Debug("更新标签 [{0}] 值: {1}", tagResult.TagName, displayValue);
                            updated = true;
                            break;
                        }
                    }
                }
                
                if (!updated)
                {
                    Logger.Warn("未找到匹配的标签项: {0} (TagId: {1})", 
                        tagResult.TagName, tagResult.TagId);
                }
            }
            
            // 强制刷新 ListView
            _tagsListView.Refresh();
        }

        // 保留此方法作为后备（如果协议不支持时使用模拟数据）
        private string GenerateMockValue(string dataType)
        {
            var random = new Random();
            switch (dataType?.ToLower())
            {
                case "boolean":
                case "bool":
                    return random.Next(2) == 0 ? "false" : "true";
                case "int16":
                case "int32":
                case "int64":
                case "uint16":
                case "uint32":
                case "uint64":
                    return random.Next(0, 1000).ToString();
                case "float":
                case "double":
                    return (random.NextDouble() * 100).ToString("F2");
                case "string":
                    return $"Value_{random.Next(100)}";
                default:
                    return random.Next(0, 100).ToString();
            }
        }
    }
}
