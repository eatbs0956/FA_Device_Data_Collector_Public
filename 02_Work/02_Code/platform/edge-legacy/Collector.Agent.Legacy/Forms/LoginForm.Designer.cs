namespace Collector.Agent.Legacy.Forms
{
    partial class LoginForm
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
            this._titleLabel = new System.Windows.Forms.Label();
            this._gatewayLabel = new System.Windows.Forms.Label();
            this._gatewayTextBox = new System.Windows.Forms.TextBox();
            this._nodeIdLabel = new System.Windows.Forms.Label();
            this._nodeIdTextBox = new System.Windows.Forms.TextBox();
            this._nodeNameLabel = new System.Windows.Forms.Label();
            this._nodeNameTextBox = new System.Windows.Forms.TextBox();
            this._usernameLabel = new System.Windows.Forms.Label();
            this._usernameTextBox = new System.Windows.Forms.TextBox();
            this._passwordLabel = new System.Windows.Forms.Label();
            this._passwordTextBox = new System.Windows.Forms.TextBox();
            this._showPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this._loginButton = new System.Windows.Forms.Button();
            this._settingsButton = new System.Windows.Forms.Button();
            this._statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _titleLabel
            // 
            this._titleLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._titleLabel.Location = new System.Drawing.Point(30, 20);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new System.Drawing.Size(350, 35);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "工业数据采集器";
            this._titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _gatewayLabel
            // 
            this._gatewayLabel.AutoSize = true;
            this._gatewayLabel.Location = new System.Drawing.Point(30, 73);
            this._gatewayLabel.Name = "_gatewayLabel";
            this._gatewayLabel.Size = new System.Drawing.Size(100, 23);
            this._gatewayLabel.TabIndex = 1;
            this._gatewayLabel.Text = "网关地址:";
            // 
            // _gatewayTextBox
            // 
            this._gatewayTextBox.Location = new System.Drawing.Point(130, 70);
            this._gatewayTextBox.Name = "_gatewayTextBox";
            this._gatewayTextBox.Size = new System.Drawing.Size(260, 21);
            this._gatewayTextBox.TabIndex = 2;
            this._gatewayTextBox.Text = "http://localhost:60620";
            // 
            // _nodeIdLabel
            // 
            this._nodeIdLabel.AutoSize = true;
            this._nodeIdLabel.Location = new System.Drawing.Point(30, 108);
            this._nodeIdLabel.Name = "_nodeIdLabel";
            this._nodeIdLabel.Size = new System.Drawing.Size(100, 23);
            this._nodeIdLabel.TabIndex = 3;
            this._nodeIdLabel.Text = "节点ID:";
            // 
            // _nodeIdTextBox
            // 
            this._nodeIdTextBox.Location = new System.Drawing.Point(130, 105);
            this._nodeIdTextBox.Name = "_nodeIdTextBox";
            this._nodeIdTextBox.Size = new System.Drawing.Size(260, 21);
            this._nodeIdTextBox.TabIndex = 4;
            // 
            // _nodeNameLabel
            // 
            this._nodeNameLabel.AutoSize = true;
            this._nodeNameLabel.Location = new System.Drawing.Point(30, 143);
            this._nodeNameLabel.Name = "_nodeNameLabel";
            this._nodeNameLabel.Size = new System.Drawing.Size(100, 23);
            this._nodeNameLabel.TabIndex = 5;
            this._nodeNameLabel.Text = "节点名称:";
            // 
            // _nodeNameTextBox
            // 
            this._nodeNameTextBox.Location = new System.Drawing.Point(130, 140);
            this._nodeNameTextBox.Name = "_nodeNameTextBox";
            this._nodeNameTextBox.Size = new System.Drawing.Size(260, 21);
            this._nodeNameTextBox.TabIndex = 6;
            // 
            // _usernameLabel
            // 
            this._usernameLabel.AutoSize = true;
            this._usernameLabel.Location = new System.Drawing.Point(30, 218);
            this._usernameLabel.Name = "_usernameLabel";
            this._usernameLabel.Size = new System.Drawing.Size(100, 23);
            this._usernameLabel.TabIndex = 7;
            this._usernameLabel.Text = "用户名:";
            // 
            // _usernameTextBox
            // 
            this._usernameTextBox.Location = new System.Drawing.Point(130, 215);
            this._usernameTextBox.Name = "_usernameTextBox";
            this._usernameTextBox.Size = new System.Drawing.Size(260, 21);
            this._usernameTextBox.TabIndex = 8;
            // 
            // _passwordLabel
            // 
            this._passwordLabel.AutoSize = true;
            this._passwordLabel.Location = new System.Drawing.Point(30, 253);
            this._passwordLabel.Name = "_passwordLabel";
            this._passwordLabel.Size = new System.Drawing.Size(100, 23);
            this._passwordLabel.TabIndex = 9;
            this._passwordLabel.Text = "密码:";
            // 
            // _passwordTextBox
            // 
            this._passwordTextBox.Location = new System.Drawing.Point(130, 250);
            this._passwordTextBox.Name = "_passwordTextBox";
            this._passwordTextBox.PasswordChar = '●';
            this._passwordTextBox.Size = new System.Drawing.Size(260, 21);
            this._passwordTextBox.TabIndex = 10;
            // 
            // _showPasswordCheckBox
            // 
            this._showPasswordCheckBox.AutoSize = true;
            this._showPasswordCheckBox.Location = new System.Drawing.Point(130, 280);
            this._showPasswordCheckBox.Name = "_showPasswordCheckBox";
            this._showPasswordCheckBox.Size = new System.Drawing.Size(100, 20);
            this._showPasswordCheckBox.TabIndex = 11;
            this._showPasswordCheckBox.Text = "显示密码";
            this._showPasswordCheckBox.UseVisualStyleBackColor = true;
            this._showPasswordCheckBox.CheckedChanged += new System.EventHandler(this.ShowPasswordCheckBox_CheckedChanged);
            // 
            // _loginButton
            // 
            this._loginButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this._loginButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._loginButton.ForeColor = System.Drawing.Color.White;
            this._loginButton.Location = new System.Drawing.Point(80, 355);
            this._loginButton.Name = "_loginButton";
            this._loginButton.Size = new System.Drawing.Size(120, 35);
            this._loginButton.TabIndex = 12;
            this._loginButton.Text = "登录并启动";
            this._loginButton.UseVisualStyleBackColor = false;
            this._loginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // _settingsButton
            // 
            this._settingsButton.Location = new System.Drawing.Point(220, 355);
            this._settingsButton.Name = "_settingsButton";
            this._settingsButton.Size = new System.Drawing.Size(100, 35);
            this._settingsButton.TabIndex = 13;
            this._settingsButton.Text = "高级设置";
            this._settingsButton.UseVisualStyleBackColor = true;
            this._settingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // _statusLabel
            // 
            this._statusLabel.ForeColor = System.Drawing.Color.Red;
            this._statusLabel.Location = new System.Drawing.Point(30, 320);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(350, 23);
            this._statusLabel.TabIndex = 14;
            this._statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LoginForm
            // 
            this.AcceptButton = this._loginButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 480);
            this.Controls.Add(this._statusLabel);
            this.Controls.Add(this._settingsButton);
            this.Controls.Add(this._loginButton);
            this.Controls.Add(this._showPasswordCheckBox);
            this.Controls.Add(this._passwordTextBox);
            this.Controls.Add(this._passwordLabel);
            this.Controls.Add(this._usernameTextBox);
            this.Controls.Add(this._usernameLabel);
            this.Controls.Add(this._nodeNameTextBox);
            this.Controls.Add(this._nodeNameLabel);
            this.Controls.Add(this._nodeIdTextBox);
            this.Controls.Add(this._nodeIdLabel);
            this.Controls.Add(this._gatewayTextBox);
            this.Controls.Add(this._gatewayLabel);
            this.Controls.Add(this._titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "工业数据采集器 - 登录";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.Label _gatewayLabel;
        private System.Windows.Forms.TextBox _gatewayTextBox;
        private System.Windows.Forms.Label _nodeIdLabel;
        private System.Windows.Forms.TextBox _nodeIdTextBox;
        private System.Windows.Forms.Label _nodeNameLabel;
        private System.Windows.Forms.TextBox _nodeNameTextBox;
        private System.Windows.Forms.Label _usernameLabel;
        private System.Windows.Forms.TextBox _usernameTextBox;
        private System.Windows.Forms.Label _passwordLabel;
        private System.Windows.Forms.TextBox _passwordTextBox;
        private System.Windows.Forms.CheckBox _showPasswordCheckBox;
        private System.Windows.Forms.Button _loginButton;
        private System.Windows.Forms.Button _settingsButton;
        private System.Windows.Forms.Label _statusLabel;
    }
}
