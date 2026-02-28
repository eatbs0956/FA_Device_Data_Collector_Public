namespace Collector.Agent.Legacy.Forms
{
    partial class TaskDetailForm
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
            this._configTab = new System.Windows.Forms.TabPage();
            this._devicesTab = new System.Windows.Forms.TabPage();
            this._devicesListView = new System.Windows.Forms.ListView();
            this._closeButton = new System.Windows.Forms.Button();
            this._tabControl.SuspendLayout();
            this._devicesTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._configTab);
            this._tabControl.Controls.Add(this._devicesTab);
            this._tabControl.Location = new System.Drawing.Point(12, 12);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(510, 350);
            this._tabControl.TabIndex = 0;
            // 
            // _configTab
            // 
            this._configTab.Location = new System.Drawing.Point(4, 22);
            this._configTab.Name = "_configTab";
            this._configTab.Padding = new System.Windows.Forms.Padding(3);
            this._configTab.Size = new System.Drawing.Size(502, 324);
            this._configTab.TabIndex = 0;
            this._configTab.Text = "任务配置";
            this._configTab.UseVisualStyleBackColor = true;
            // 
            // _devicesTab
            // 
            this._devicesTab.Controls.Add(this._devicesListView);
            this._devicesTab.Location = new System.Drawing.Point(4, 22);
            this._devicesTab.Name = "_devicesTab";
            this._devicesTab.Padding = new System.Windows.Forms.Padding(3);
            this._devicesTab.Size = new System.Drawing.Size(502, 324);
            this._devicesTab.TabIndex = 1;
            this._devicesTab.Text = "关联设备";
            this._devicesTab.UseVisualStyleBackColor = true;
            // 
            // _devicesListView
            // 
            this._devicesListView.FullRowSelect = true;
            this._devicesListView.GridLines = true;
            this._devicesListView.Location = new System.Drawing.Point(10, 10);
            this._devicesListView.Name = "_devicesListView";
            this._devicesListView.Size = new System.Drawing.Size(480, 300);
            this._devicesListView.TabIndex = 0;
            this._devicesListView.UseCompatibleStateImageBehavior = false;
            this._devicesListView.View = System.Windows.Forms.View.Details;
            // 
            // _closeButton
            // 
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._closeButton.Location = new System.Drawing.Point(447, 375);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(75, 30);
            this._closeButton.TabIndex = 1;
            this._closeButton.Text = "关闭";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // TaskDetailForm
            // 
            this.AcceptButton = this._closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 450);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TaskDetailForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "任务详情";
            this._tabControl.ResumeLayout(false);
            this._devicesTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _configTab;
        private System.Windows.Forms.TabPage _devicesTab;
        private System.Windows.Forms.ListView _devicesListView;
        private System.Windows.Forms.Button _closeButton;
    }
}
