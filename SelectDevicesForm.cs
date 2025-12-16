using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Yabe;
using System.IO.BACnet;

namespace ClearRecipientList
{
    public partial class SelectDevicesForm : Form
    {
        private YabeMainDialog _yabeFrm;
        private List<BACnetDevice> _allDevices;
        private ListView _lvDevices;
        private CheckBox _chkSelectAll;
        private bool _isUpdatingCheckboxes = false;
        public List<BACnetDevice> SelectedDevices { get; private set; }

        public SelectDevicesForm(YabeMainDialog yabeFrm)
        {
            _yabeFrm = yabeFrm;
            SelectedDevices = new List<BACnetDevice>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Geräte auswählen";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false;

            // Layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Label
            Label lblDevices = new Label
            {
                Text = "Verfügbare Geräte (Haken = Auswahl):",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainLayout.Controls.Add(lblDevices, 0, 0);

            // Select All Checkbox
            _chkSelectAll = new CheckBox
            {
                Text = "Alle auswählen",
                AutoSize = true,
                Checked = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            _chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;
            mainLayout.Controls.Add(_chkSelectAll, 0, 1);

            // ListView for devices with checkboxes
            _lvDevices = new ListView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };
            _lvDevices.Columns.Add("Device ID", 80);
            _lvDevices.Columns.Add("Name", 200);
            _lvDevices.Columns.Add("Description", 250);
            _lvDevices.Columns.Add("Adresse", 150);
            _lvDevices.ItemChecked += (s, e) => {
                if (_isUpdatingCheckboxes) return;

                // Update "Select All" checkbox based on item states
                if (_lvDevices.Items.Count > 0)
                {
                    bool allChecked = _lvDevices.Items.Cast<ListViewItem>().All(item => item.Checked);
                    if (_chkSelectAll.Checked != allChecked)
                    {
                        _isUpdatingCheckboxes = true;
                        _chkSelectAll.Checked = allChecked;
                        _isUpdatingCheckboxes = false;
                    }
                }
            };
            mainLayout.Controls.Add(_lvDevices, 0, 2);

            // Button Panel
            FlowLayoutPanel btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0)
            };

            Button btnOK = new Button
            {
                Text = "Weiter",
                DialogResult = DialogResult.OK,
                Width = 100,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnOK.Click += (s, e) => {
                SelectedDevices.Clear();
                foreach (ListViewItem item in _lvDevices.CheckedItems)
                {
                    SelectedDevices.Add((BACnetDevice)item.Tag);
                }
            };

            Button btnCancel = new Button
            {
                Text = "Abbrechen",
                DialogResult = DialogResult.Cancel,
                Width = 100
            };

            btnPanel.Controls.Add(btnOK);
            btnPanel.Controls.Add(btnCancel);

            mainLayout.Controls.Add(btnPanel, 0, 3);

            this.Controls.Add(mainLayout);
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Load devices from Yabe
            _allDevices = new List<BACnetDevice>(_yabeFrm.YabeDiscoveredDevices);

            // Start loading device details in background
            this.Load += (s, e) => LoadDeviceDetails();
        }

        private void LoadDeviceDetails()
        {
            Application.UseWaitCursor = true;
            _lvDevices.Items.Clear();

            foreach (var device in _allDevices)
            {
                // Add device with placeholder text
                ListViewItem item = new ListViewItem(new[]
                {
                    device.deviceId.ToString(),
                    "Lade...",
                    "Lade...",
                    device.BacAdr.ToString()
                });
                item.Tag = device;
                item.Checked = true; // Select all by default
                _lvDevices.Items.Add(item);

                Application.DoEvents(); // Keep UI responsive
            }

            // Now read the actual names and descriptions
            for (int i = 0; i < _allDevices.Count; i++)
            {
                var device = _allDevices[i];
                var item = _lvDevices.Items[i];

                try
                {
                    var deviceObjectId = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device.deviceId);

                    // Read device name
                    string deviceName = device.ReadObjectName(deviceObjectId);
                    if (string.IsNullOrEmpty(deviceName))
                        deviceName = "(unbekannt)";

                    // Read device description
                    string description = "";
                    IList<BacnetValue> descValue;
                    if (device.ReadPropertyRequest(deviceObjectId, BacnetPropertyIds.PROP_DESCRIPTION, out descValue))
                    {
                        if (descValue != null && descValue.Count > 0)
                            description = descValue[0].Value?.ToString() ?? "";
                    }

                    // Update the list item
                    item.SubItems[1].Text = deviceName;
                    item.SubItems[2].Text = description;
                }
                catch (Exception ex)
                {
                    item.SubItems[1].Text = "(Fehler)";
                    item.SubItems[2].Text = ex.Message;
                    System.Diagnostics.Trace.WriteLine($"Fehler beim Laden von Device {device.deviceId}: {ex.Message}");
                }

                Application.DoEvents(); // Keep UI responsive
            }

            Application.UseWaitCursor = false;
        }

        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCheckboxes) return;

            _isUpdatingCheckboxes = true;
            foreach (ListViewItem item in _lvDevices.Items)
            {
                item.Checked = _chkSelectAll.Checked;
            }
            _isUpdatingCheckboxes = false;
        }
    }
}
