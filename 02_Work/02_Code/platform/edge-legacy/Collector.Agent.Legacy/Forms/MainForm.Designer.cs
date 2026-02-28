namespace Collector.Agent.Legacy.Forms
{
    partial class MainForm
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
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._connectionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._nodeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._taskStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._mainPanel = new System.Windows.Forms.Panel();
            this._nodeInfoGroup = new System.Windows.Forms.GroupBox();
            this._statisticsGroup = new System.Windows.Forms.GroupBox();
            this._devicesGroup = new System.Windows.Forms.GroupBox();
            this._devicesListView = new System.Windows.Forms.ListView();
            this._tasksGroup = new System.Windows.Forms.GroupBox();
            this._tasksListView = new System.Windows.Forms.ListView();
            this._logGroup = new System.Windows.Forms.GroupBox();
            this._logTextBox = new System.Windows.Forms.RichTextBox();
            this._startButton = new System.Windows.Forms.Button();
            this._stopButton = new System.Windows.Forms.Button();
            this._refreshButton = new System.Windows.Forms.Button();
            this._statusStrip.SuspendLayout();
            this._mainPanel.SuspendLayout();
            this._devicesGroup.SuspendLayout();
            this._tasksGroup.SuspendLayout();
            this._logGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // _menuStrip
            // 
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(900, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._connectionStatusLabel,
            this._nodeStatusLabel,
            this._taskStatusLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 628);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(900, 22);
            this._statusStrip.TabIndex = 1;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _connectionStatusLabel
            // 
            this._connectionStatusLabel.Name = "_connectionStatusLabel";
            this._connectionStatusLabel.Size = new System.Drawing.Size(80, 17);
            this._connectionStatusLabel.Text = "连接状态: 未连接";
            // 
            // _nodeStatusLabel
            // 
            this._nodeStatusLabel.Name = "_nodeStatusLabel";
            this._nodeStatusLabel.Size = new System.Drawing.Size(0, 17);
            this._nodeStatusLabel.Spring = true;
            // 
            // _taskStatusLabel
            // 
            this._taskStatusLabel.Name = "_taskStatusLabel";
            this._taskStatusLabel.Size = new System.Drawing.Size(48, 17);
            this._taskStatusLabel.Text = "任务: 0/0";
            // 
            // _mainPanel
            // 
            this._mainPanel.Controls.Add(this._logGroup);
            this._mainPanel.Controls.Add(this._tasksGroup);
            this._mainPanel.Controls.Add(this._devicesGroup);
            this._mainPanel.Controls.Add(this._refreshButton);
            this._mainPanel.Controls.Add(this._stopButton);
            this._mainPanel.Controls.Add(this._startButton);
            this._mainPanel.Controls.Add(this._statisticsGroup);
            this._mainPanel.Controls.Add(this._nodeInfoGroup);
            this._mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainPanel.Location = new System.Drawing.Point(0, 24);
            this._mainPanel.Name = "_mainPanel";
            this._mainPanel.Padding = new System.Windows.Forms.Padding(10);
            this._mainPanel.Size = new System.Drawing.Size(900, 604);
            this._mainPanel.TabIndex = 2;
            // 
            // _nodeInfoGroup
            // 
            this._nodeInfoGroup.Location = new System.Drawing.Point(10, 5);
            this._nodeInfoGroup.Name = "_nodeInfoGroup";
            this._nodeInfoGroup.Size = new System.Drawing.Size(420, 130);
            this._nodeInfoGroup.TabIndex = 0;
            this._nodeInfoGroup.TabStop = false;
            this._nodeInfoGroup.Text = "节点信息";
            // 
            // _statisticsGroup
            // 
            this._statisticsGroup.Location = new System.Drawing.Point(440, 5);
            this._statisticsGroup.Name = "_statisticsGroup";
            this._statisticsGroup.Size = new System.Drawing.Size(420, 130);
            this._statisticsGroup.TabIndex = 1;
            this._statisticsGroup.TabStop = false;
            this._statisticsGroup.Text = "采集统计";
            // 
            // _devicesGroup
            // 
            this._devicesGroup.Controls.Add(this._devicesListView);
            this._devicesGroup.Location = new System.Drawing.Point(10, 185);
            this._devicesGroup.Name = "_devicesGroup";
            this._devicesGroup.Size = new System.Drawing.Size(420, 200);
            this._devicesGroup.TabIndex = 6;
            this._devicesGroup.TabStop = false;
            this._devicesGroup.Text = "设备列表";
            // 
            // _devicesListView
            // 
            this._devicesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._devicesListView.FullRowSelect = true;
            this._devicesListView.HideSelection = false;
            this._devicesListView.Location = new System.Drawing.Point(10, 20);
            this._devicesListView.Name = "_devicesListView";
            this._devicesListView.Size = new System.Drawing.Size(400, 170);
            this._devicesListView.TabIndex = 0;
            this._devicesListView.UseCompatibleStateImageBehavior = false;
            this._devicesListView.View = System.Windows.Forms.View.Details;
            // 
            // _tasksGroup
            // 
            this._tasksGroup.Controls.Add(this._tasksListView);
            this._tasksGroup.Location = new System.Drawing.Point(440, 185);
            this._tasksGroup.Name = "_tasksGroup";
            this._tasksGroup.Size = new System.Drawing.Size(420, 200);
            this._tasksGroup.TabIndex = 7;
            this._tasksGroup.TabStop = false;
            this._tasksGroup.Text = "任务列表";
            // 
            // _tasksListView
            // 
            this._tasksListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tasksListView.FullRowSelect = true;
            this._tasksListView.HideSelection = false;
            this._tasksListView.Location = new System.Drawing.Point(10, 20);
            this._tasksListView.Name = "_tasksListView";
            this._tasksListView.Size = new System.Drawing.Size(400, 170);
            this._tasksListView.TabIndex = 0;
            this._tasksListView.UseCompatibleStateImageBehavior = false;
            this._tasksListView.View = System.Windows.Forms.View.Details;
            // 
            // _logGroup
            // 
            this._logGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._logGroup.Controls.Add(this._logTextBox);
            this._logGroup.Location = new System.Drawing.Point(10, 395);
            this._logGroup.Name = "_logGroup";
            this._logGroup.Size = new System.Drawing.Size(850, 200);
            this._logGroup.TabIndex = 5;
            this._logGroup.TabStop = false;
            this._logGroup.Text = "运行日志";
            // 
            // _logTextBox
            // 
            this._logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._logTextBox.BackColor = System.Drawing.Color.Black;
            this._logTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._logTextBox.ForeColor = System.Drawing.Color.LightGreen;
            this._logTextBox.Location = new System.Drawing.Point(10, 20);
            this._logTextBox.Name = "_logTextBox";
            this._logTextBox.ReadOnly = true;
            this._logTextBox.Size = new System.Drawing.Size(830, 170);
            this._logTextBox.TabIndex = 0;
            this._logTextBox.Text = "";
            // 
            // _startButton
            // 
            this._startButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._startButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._startButton.ForeColor = System.Drawing.Color.White;
            this._startButton.Location = new System.Drawing.Point(10, 145);
            this._startButton.Name = "_startButton";
            this._startButton.Size = new System.Drawing.Size(100, 30);
            this._startButton.TabIndex = 2;
            this._startButton.Text = "启动采集";
            this._startButton.UseVisualStyleBackColor = false;
            // 
            // _stopButton
            // 
            this._stopButton.Enabled = false;
            this._stopButton.Location = new System.Drawing.Point(120, 145);
            this._stopButton.Name = "_stopButton";
            this._stopButton.Size = new System.Drawing.Size(100, 30);
            this._stopButton.TabIndex = 3;
            this._stopButton.Text = "停止采集";
            this._stopButton.UseVisualStyleBackColor = true;
            // 
            // _refreshButton
            // 
            this._refreshButton.Location = new System.Drawing.Point(230, 145);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new System.Drawing.Size(100, 30);
            this._refreshButton.TabIndex = 4;
            this._refreshButton.Text = "刷新配置";
            this._refreshButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.Controls.Add(this._mainPanel);
            this.Controls.Add(this._statusStrip);
            this.Controls.Add(this._menuStrip);
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "工业数据采集器";
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this._mainPanel.ResumeLayout(false);
            this._devicesGroup.ResumeLayout(false);
            this._tasksGroup.ResumeLayout(false);
            this._logGroup.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _connectionStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel _nodeStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel _taskStatusLabel;
        private System.Windows.Forms.Panel _mainPanel;
        private System.Windows.Forms.GroupBox _nodeInfoGroup;
        private System.Windows.Forms.GroupBox _statisticsGroup;
        private System.Windows.Forms.GroupBox _devicesGroup;
        private System.Windows.Forms.ListView _devicesListView;
        private System.Windows.Forms.GroupBox _tasksGroup;
        private System.Windows.Forms.ListView _tasksListView;
        private System.Windows.Forms.GroupBox _logGroup;
        private System.Windows.Forms.RichTextBox _logTextBox;
        private System.Windows.Forms.Button _startButton;
        private System.Windows.Forms.Button _stopButton;
        private System.Windows.Forms.Button _refreshButton;
    }
}
