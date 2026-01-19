using System;
using System.Reflection;
using System.Windows.Forms;
using System.IO.BACnet;
using Yabe;

namespace ClearRecipientList
{
    public class BulkAddRecipientForm : Form
    {
        private RecipientUserCtrl _recipientCtrl;
        private Button _okButton;
        private Button _cancelButton;

        public BulkAddRecipientForm()
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "1.0.0.0";
            this.Text = $"Empfänger hinzufügen - v{version}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 440;
            this.Height = 520;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // RecipientUserCtrl benötigt einen TabPage-Container, kann aber unabhängig genutzt werden
            var dummyTab = new TabPage("Empfänger");
            _recipientCtrl = new RecipientUserCtrl(dummyTab);
            _recipientCtrl.Dock = DockStyle.Fill;

            panel.Controls.Add(_recipientCtrl, 0, 0);

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };

            _okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 100 };
            _cancelButton = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Width = 100 };

            buttons.Controls.Add(_okButton);
            buttons.Controls.Add(_cancelButton);

            panel.Controls.Add(buttons, 0, 1);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            this.Controls.Add(panel);
        }

        public DeviceReportingRecipient BuildRecipient()
        {
            // Werte aus dem RecipientUserCtrl auslesen und DeviceReportingRecipient erzeugen
            var r = _recipientCtrl;
            uint processId = 0;
            try { processId = Convert.ToUInt32(r.ProcessId.Text); } catch { processId = 0; }

            if (r.adr != null)
            {
                return new DeviceReportingRecipient(
                    r.WeekOfDay,
                    r.fromTime.Value,
                    r.toTime.Value,
                    r.adr,
                    processId,
                    r.AckRequired.Checked,
                    r.EventType
                );
            }
            else
            {
                return new DeviceReportingRecipient(
                    r.WeekOfDay,
                    r.fromTime.Value,
                    r.toTime.Value,
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, r.deviceid),
                    processId,
                    r.AckRequired.Checked,
                    r.EventType
                );
            }
        }
    }
}
