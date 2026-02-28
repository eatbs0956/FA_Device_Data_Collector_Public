using System;
using System.Windows.Forms;
using Collector.Agent.Legacy.Services;

namespace Collector.Agent.Legacy.Forms
{
    public partial class VersionHistoryForm : Form
    {
        public VersionHistoryForm()
        {
            InitializeComponent();
            InitializeText();
            LoadVersionHistory();
        }

        private void InitializeText()
        {
            this.Text = L.T("VersionHistoryForm_Title");
            _closeButton.Text = L.T("Common_Close");
        }

        private void LoadVersionHistory()
        {
            var history = new System.Text.StringBuilder();
            
            history.AppendLine(L.T("VersionHistory_1_0_0"));
            history.AppendLine();

            _historyTextBox.Text = history.ToString();
        }
    }
}
