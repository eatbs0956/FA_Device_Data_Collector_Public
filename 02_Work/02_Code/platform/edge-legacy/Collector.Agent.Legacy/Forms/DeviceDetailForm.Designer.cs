namespace Collector.Agent.Legacy.Forms
{
    partial class DeviceDetailForm
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
            this._basicInfoTab = new System.Windows.Forms.TabPage();
            this._tagsTab = new System.Windows.Forms.TabPage();
            this._tagsListView = new System.Windows.Forms.ListView();
            this._testConnectionButton = new System.Windows.Forms.Button();
            this._testCollectionButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._tabControl.SuspendLayout();
            this._tagsTab.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._basicInfoTab);
            this._tabControl.Controls.Add(this._tagsTab);
            this._tabControl.Location = new System.Drawing.Point(12, 12);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(560, 380);
            this._tabControl.TabIndex = 0;
            // 
            // _basicInfoTab
            // 
            this._basicInfoTab.Location = new System.Drawing.Point(4, 22);
            this._basicInfoTab.Name = "_basicInfoTab";
            this._basicInfoTab.Padding = new System.Windows.Forms.Padding(3);
            this._basicInfoTab.Size = new System.Drawing.Size(552, 354);
            this._basicInfoTab.TabIndex = 0;
            this._basicInfoTab.Text = "基本信息";
            this._basicInfoTab.UseVisualStyleBackColor = true;
            // 
            // _tagsTab
            // 
            this._tagsTab.Controls.Add(this._tagsListView);
            this._tagsTab.Location = new System.Drawing.Point(4, 22);
            this._tagsTab.Name = "_tagsTab";
            this._tagsTab.Padding = new System.Windows.Forms.Padding(3);
            this._tagsTab.Size = new System.Drawing.Size(552, 354);
            this._tagsTab.TabIndex = 1;
            this._tagsTab.Text = "标签点";
            this._tagsTab.UseVisualStyleBackColor = true;
            // 
            // _tagsListView
            // 
            this._tagsListView.FullRowSelect = true;
            this._tagsListView.GridLines = true;
            this._tagsListView.HideSelection = false;
            this._tagsListView.Location = new System.Drawing.Point(10, 10);
            this._tagsListView.Name = "_tagsListView";
            this._tagsListView.Size = new System.Drawing.Size(530, 320);
            this._tagsListView.TabIndex = 0;
            this._tagsListView.UseCompatibleStateImageBehavior = false;
            this._tagsListView.View = System.Windows.Forms.View.Details;
            // 
            // _testConnectionButton
            // 
            this._testConnectionButton.Location = new System.Drawing.Point(12, 405);
            this._testConnectionButton.Name = "_testConnectionButton";
            this._testConnectionButton.Size = new System.Drawing.Size(140, 30);
            this._testConnectionButton.TabIndex = 1;
            this._testConnectionButton.Text = "测试连接";
            this._testConnectionButton.UseVisualStyleBackColor = true;
            // 
            // _testCollectionButton
            // 
            this._testCollectionButton.Location = new System.Drawing.Point(158, 405);
            this._testCollectionButton.Name = "_testCollectionButton";
            this._testCollectionButton.Size = new System.Drawing.Size(140, 30);
            this._testCollectionButton.TabIndex = 2;
            this._testCollectionButton.Text = "测试采集";
            this._testCollectionButton.UseVisualStyleBackColor = true;
            // 
            // _closeButton
            // 
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._closeButton.Location = new System.Drawing.Point(487, 405);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(85, 30);
            this._closeButton.TabIndex = 3;
            this._closeButton.Text = "关闭";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._statusLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 478);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(600, 22);
            this._statusStrip.TabIndex = 4;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _statusLabel
            // 
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // DeviceDetailForm
            // 
            this.AcceptButton = this._closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Controls.Add(this._statusStrip);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._testCollectionButton);
            this.Controls.Add(this._testConnectionButton);
            this.Controls.Add(this._tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeviceDetailForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设备详情";
            this._tabControl.ResumeLayout(false);
            this._tagsTab.ResumeLayout(false);
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _basicInfoTab;
        private System.Windows.Forms.TabPage _tagsTab;
        private System.Windows.Forms.ListView _tagsListView;
        private System.Windows.Forms.Button _testConnectionButton;
        private System.Windows.Forms.Button _testCollectionButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
    }
}
