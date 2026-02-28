namespace Collector.Agent.Legacy.Forms
{
    partial class AboutForm
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
            this._appNameLabel = new System.Windows.Forms.Label();
            this._versionLabel = new System.Windows.Forms.LinkLabel();
            this._platformLabel = new System.Windows.Forms.Label();
            this._descriptionLabel = new System.Windows.Forms.Label();
            this._copyrightLabel = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _appNameLabel
            // 
            this._appNameLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._appNameLabel.Location = new System.Drawing.Point(20, 20);
            this._appNameLabel.Name = "_appNameLabel";
            this._appNameLabel.Size = new System.Drawing.Size(300, 25);
            this._appNameLabel.TabIndex = 0;
            this._appNameLabel.Text = "工业数据采集器";
            this._appNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _versionLabel
            // 
            this._versionLabel.Location = new System.Drawing.Point(20, 55);
            this._versionLabel.Name = "_versionLabel";
            this._versionLabel.Size = new System.Drawing.Size(300, 20);
            this._versionLabel.TabIndex = 1;
            this._versionLabel.TabStop = true;
            this._versionLabel.Text = "版本: 1.0.0";
            this._versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._versionLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.VersionLabel_LinkClicked);
            // 
            // _platformLabel
            // 
            this._platformLabel.ForeColor = System.Drawing.Color.Gray;
            this._platformLabel.Location = new System.Drawing.Point(20, 80);
            this._platformLabel.Name = "_platformLabel";
            this._platformLabel.Size = new System.Drawing.Size(300, 20);
            this._platformLabel.TabIndex = 2;
            this._platformLabel.Text = ".NET Framework 4.7.2";
            this._platformLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _descriptionLabel
            // 
            this._descriptionLabel.Location = new System.Drawing.Point(20, 110);
            this._descriptionLabel.Name = "_descriptionLabel";
            this._descriptionLabel.Size = new System.Drawing.Size(300, 40);
            this._descriptionLabel.TabIndex = 3;
            this._descriptionLabel.Text = "工业设备数据采集边缘端程序";
            this._descriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _copyrightLabel
            // 
            this._copyrightLabel.ForeColor = System.Drawing.Color.Gray;
            this._copyrightLabel.Location = new System.Drawing.Point(20, 150);
            this._copyrightLabel.Name = "_copyrightLabel";
            this._copyrightLabel.Size = new System.Drawing.Size(300, 20);
            this._copyrightLabel.TabIndex = 4;
            this._copyrightLabel.Text = "Copyright ? 2024";
            this._copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(130, 180);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(80, 28);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "确定";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // AboutForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 250);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._copyrightLabel);
            this.Controls.Add(this._descriptionLabel);
            this.Controls.Add(this._platformLabel);
            this.Controls.Add(this._versionLabel);
            this.Controls.Add(this._appNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "关于";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _appNameLabel;
        private System.Windows.Forms.LinkLabel _versionLabel;
        private System.Windows.Forms.Label _platformLabel;
        private System.Windows.Forms.Label _descriptionLabel;
        private System.Windows.Forms.Label _copyrightLabel;
        private System.Windows.Forms.Button _okButton;
    }
}
