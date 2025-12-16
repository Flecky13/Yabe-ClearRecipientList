using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClearRecipientList
{
    public partial class ProgressForm : Form
    {
        private Label _lblStatus;
        private ProgressBar _progressBar;

        public ProgressForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Verarbeitung läuft...";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _lblStatus = new Label
            {
                Text = "Verarbeitung läuft...",
                AutoSize = true,
                Dock = DockStyle.Top
            };
            mainLayout.Controls.Add(_lblStatus, 0, 0);

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Margin = new Padding(0, 10, 0, 0)
            };
            mainLayout.Controls.Add(_progressBar, 0, 1);

            this.Controls.Add(mainLayout);
        }

        public void UpdateProgress(int current, int total, string status)
        {
            _lblStatus.Text = status;
            _progressBar.Maximum = total;
            _progressBar.Value = current;
            Application.DoEvents();
        }
    }
}
