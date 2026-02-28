namespace Collector.Agent.Legacy.Forms
{
    partial class VersionHistoryForm
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
            this._historyTextBox = new System.Windows.Forms.TextBox();
            this._closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _historyTextBox
            // 
            this._historyTextBox.BackColor = System.Drawing.Color.White;
            this._historyTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._historyTextBox.Location = new System.Drawing.Point(12, 12);
            this._historyTextBox.Multiline = true;
            this._historyTextBox.Name = "_historyTextBox";
            this._historyTextBox.ReadOnly = true;
            this._historyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._historyTextBox.Size = new System.Drawing.Size(460, 300);
            this._historyTextBox.TabIndex = 0;
            // 
            // _closeButton
            // 
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._closeButton.Location = new System.Drawing.Point(397, 325);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(75, 25);
            this._closeButton.TabIndex = 1;
            this._closeButton.Text = "关闭";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // VersionHistoryForm
            // 
            this.AcceptButton = this._closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._historyTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VersionHistoryForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "版本履历";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _historyTextBox;
        private System.Windows.Forms.Button _closeButton;
    }
}
