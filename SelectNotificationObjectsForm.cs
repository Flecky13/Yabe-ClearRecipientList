using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.BACnet;
using System.Linq;
using System.Windows.Forms;
using Yabe;

namespace ClearRecipientList
{
    public partial class SelectNotificationObjectsForm : Form
    {
        private YabeMainDialog _yabeFrm;
        private List<BACnetDevice> _selectedDevices;
        private ListView _lvObjects;
        private CheckBox _chkSelectAll;
        private bool _isUpdatingCheckboxes = false;
        public List<ObjectSelection> SelectedObjects { get; private set; }

        public SelectNotificationObjectsForm(YabeMainDialog yabeFrm, List<BACnetDevice> selectedDevices)
        {
            _yabeFrm = yabeFrm;
            _selectedDevices = selectedDevices;
            SelectedObjects = new List<ObjectSelection>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Notification Class Objekte auswählen";
            this.Size = new Size(600, 500);
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
            Label lblObjects = new Label
            {
                Text = "Verfügbare Notification Class Objekte (Haken = Auswahl):",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainLayout.Controls.Add(lblObjects, 0, 0);

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

            // ListView for better display
            _lvObjects = new ListView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };

            _lvObjects.Columns.Add("Geräte", 150);
            _lvObjects.Columns.Add("Objekt ID", 100);
            _lvObjects.Columns.Add("Name", 250);
            _lvObjects.ItemChecked += (s, e) => {
                if (_isUpdatingCheckboxes) return;

                // Update "Select All" checkbox based on item states
                if (_lvObjects.Items.Count > 0)
                {
                    bool allChecked = _lvObjects.Items.Cast<ListViewItem>().All(item => item.Checked);
                    if (_chkSelectAll.Checked != allChecked)
                    {
                        _isUpdatingCheckboxes = true;
                        _chkSelectAll.Checked = allChecked;
                        _isUpdatingCheckboxes = false;
                    }
                }
            };

            mainLayout.Controls.Add(_lvObjects, 0, 2);

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
                Text = "Starten",
                DialogResult = DialogResult.OK,
                Width = 100,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnOK.Click += (s, e) => {
                SelectedObjects.Clear();
                foreach (ListViewItem item in _lvObjects.CheckedItems)
                {
                    SelectedObjects.Add((ObjectSelection)item.Tag);
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

            // Load notification class objects from selected devices
            LoadNotificationObjects();
        }

        private void LoadNotificationObjects()
        {
            foreach (var device in _selectedDevices)
            {
                try
                {
                    // Get all objects from the device
                    List<BacnetObjectId> objectList;
                    uint count;
                    if (!device.ReadObjectList(out objectList, out count))
                    {
                        System.Diagnostics.Trace.WriteLine($"Fehler beim Laden der Objektliste von {device.deviceName}");
                        continue;
                    }

                    if (objectList == null) continue;

                    // Filter for NOTIFICATION_CLASS objects
                    var notificationObjects = objectList
                        .Where(objId => objId.type == BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS)
                        .ToList();

                    foreach (var objId in notificationObjects)
                    {
                        try
                        {
                            string objName = device.GetObjectName(objId);
                            if (string.IsNullOrEmpty(objName))
                                objName = objId.ToString();

                            ListViewItem item = new ListViewItem(new[]
                            {
                                device.deviceName,
                                objId.ToString(),
                                objName
                            });

                            item.Tag = new ObjectSelection
                            {
                                Device = device,
                                ObjectId = objId,
                                ObjectName = objName
                            };
                            item.Checked = true; // Select all by default

                            _lvObjects.Items.Add(item);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine($"Fehler beim Laden des Objekts: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Fehler beim Laden der Objekte von {device.deviceName}: {ex.Message}");
                    MessageBox.Show(
                        $"Fehler beim Laden der Objekte von {device.deviceName}:\n{ex.Message}",
                        "Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCheckboxes) return;

            _isUpdatingCheckboxes = true;
            foreach (ListViewItem item in _lvObjects.Items)
            {
                item.Checked = _chkSelectAll.Checked;
            }
            _isUpdatingCheckboxes = false;
        }
    }
}
