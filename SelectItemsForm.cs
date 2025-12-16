using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.BACnet;
using System.Linq;
using System.Windows.Forms;
using Yabe;

namespace ClearRecipientList
{
    public partial class SelectItemsForm : Form
    {
        private YabeMainDialog _yabeFrm;
        private List<BACnetDevice> _allDevices;
        private List<ObjectSelection> _allObjects;
        private List<ListViewItem> _allObjectItems; // Cache für ListViewItems
        private ListView _lvDevices;
        private ListView _lvObjects;
        private TextBox _txtFilterObjects;
        private CheckBox _chkSelectAllDevices;
        private CheckBox _chkSelectAllObjects;
        private ProgressBar _progressBar;
        private Label _lblStatus;
        private bool _isUpdatingCheckboxes = false;
        private bool _isLoadingDevices = false;

        public List<BACnetDevice> SelectedDevices { get; private set; }
        public List<ObjectSelection> SelectedObjects { get; private set; }

        public SelectItemsForm(YabeMainDialog yabeFrm)
        {
            _yabeFrm = yabeFrm;
            SelectedDevices = new List<BACnetDevice>();
            SelectedObjects = new List<ObjectSelection>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Geräte und Notification Class Objekte auswählen";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false;

            // Main layout with 2 columns for left and right panels
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // LEFT PANEL - Devices
            TableLayoutPanel leftPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0, 0, 5, 0)
            };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label lblDevices = new Label
            {
                Text = "Verfügbare Geräte:",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5),
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };
            leftPanel.Controls.Add(lblDevices, 0, 0);

            _chkSelectAllDevices = new CheckBox
            {
                Text = "Alle auswählen",
                AutoSize = true,
                Checked = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            _chkSelectAllDevices.CheckedChanged += (s, e) => ChkSelectAllDevices_CheckedChanged();
            leftPanel.Controls.Add(_chkSelectAllDevices, 0, 1);

            _lvDevices = new ListView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };
            _lvDevices.Columns.Add("Device ID", 70);
            _lvDevices.Columns.Add("Name", 150);
            _lvDevices.Columns.Add("Description", 200);
            _lvDevices.Columns.Add("Adresse", 120);
            _lvDevices.ItemChecked += (s, e) => LvDevices_ItemChecked();
            leftPanel.Controls.Add(_lvDevices, 0, 2);

            // RIGHT PANEL - Objects
            TableLayoutPanel rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(5, 0, 0, 0)
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label lblObjects = new Label
            {
                Text = "Notification Class Objekte:",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5),
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblObjects, 0, 0);

            // Filter TextBox
            _txtFilterObjects = new TextBox
            {
                Dock = DockStyle.Fill,
                Height = 25,
                Margin = new Padding(0, 0, 0, 10)
            };
            _txtFilterObjects.TextChanged += (s, e) => ApplyFilterToObjects();
            rightPanel.Controls.Add(_txtFilterObjects, 0, 1);

            _chkSelectAllObjects = new CheckBox
            {
                Text = "Alle auswählen",
                AutoSize = true,
                Checked = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            _chkSelectAllObjects.CheckedChanged += (s, e) => ChkSelectAllObjects_CheckedChanged();
            rightPanel.Controls.Add(_chkSelectAllObjects, 0, 2);

            _lvObjects = new ListView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };
            _lvObjects.Columns.Add("Gerät", 150);
            _lvObjects.Columns.Add("Objekt ID", 100);
            _lvObjects.Columns.Add("Name", 250);
            _lvObjects.ItemChecked += (s, e) => LvObjects_ItemChecked();
            rightPanel.Controls.Add(_lvObjects, 0, 3);

            // Button Panel
            FlowLayoutPanel btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0)
            };

            Button btnStart = new Button
            {
                Text = "Starten",
                DialogResult = DialogResult.OK,
                Width = 100,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnStart.Click += (s, e) => {
                SelectedDevices.Clear();
                foreach (ListViewItem item in _lvDevices.CheckedItems)
                {
                    SelectedDevices.Add((BACnetDevice)item.Tag);
                }

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

            btnPanel.Controls.Add(btnStart);
            btnPanel.Controls.Add(btnCancel);

            rightPanel.Controls.Add(btnPanel, 0, 4);

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);

            // Progress Panel (bottom, spans both columns)
            TableLayoutPanel progressPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 10, 0, 0)
            };
            progressPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            progressPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _lblStatus = new Label
            {
                Text = "",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            progressPanel.Controls.Add(_lblStatus, 0, 0);

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Height = 20,
                Visible = false
            };
            progressPanel.Controls.Add(_progressBar, 0, 1);

            mainLayout.Controls.Add(progressPanel, 0, 1);
            mainLayout.SetColumnSpan(progressPanel, 2);

            this.Controls.Add(mainLayout);
            this.AcceptButton = btnStart;
            this.CancelButton = btnCancel;

            // Load devices from Yabe
            _allDevices = new List<BACnetDevice>(_yabeFrm.YabeDiscoveredDevices);

            // Start loading AFTER form is shown
            this.Shown += (s, e) => System.Threading.Tasks.Task.Run(() => LoadDevicesIncrementallyAsync());
        }

        private void LoadDevicesIncrementallyAsync()
        {
            _isLoadingDevices = true;

            this.Invoke((Action)(() =>
            {
                _progressBar.Visible = true;
                _progressBar.Maximum = _allDevices.Count;
                _progressBar.Value = 0;
                _lblStatus.Text = $"Lade Geräte... 0/{_allDevices.Count}";
            }));

            _allObjects = new List<ObjectSelection>();
            _allObjectItems = new List<ListViewItem>();

            for (int i = 0; i < _allDevices.Count; i++)
            {
                var device = _allDevices[i];
                int index = i;

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

                    // Add device to ListView on UI thread
                    string finalName = deviceName;
                    string finalDesc = description;
                    this.Invoke((Action)(() =>
                    {
                        ListViewItem item = new ListViewItem(new[]
                        {
                            device.deviceId.ToString(),
                            finalName,
                            finalDesc,
                            device.BacAdr.ToString()
                        });
                        item.Tag = device;
                        item.Checked = true;
                        _lvDevices.Items.Add(item);

                        _progressBar.Value = index + 1;
                        _lblStatus.Text = $"Lade Geräte... {index + 1}/{_allDevices.Count}";
                    }));

                    // Load NC objects for this device immediately
                    LoadNotificationClassObjects(device);
                }
                catch (Exception ex)
                {
                    this.Invoke((Action)(() =>
                    {
                        ListViewItem item = new ListViewItem(new[]
                        {
                            device.deviceId.ToString(),
                            "(Fehler)",
                            ex.Message,
                            device.BacAdr.ToString()
                        });
                        item.Tag = device;
                        item.Checked = true;
                        _lvDevices.Items.Add(item);

                        _progressBar.Value = index + 1;
                        _lblStatus.Text = $"Lade Geräte... {index + 1}/{_allDevices.Count}";
                    }));
                    System.Diagnostics.Trace.WriteLine($"Fehler beim Laden von Device {device.deviceId}: {ex.Message}");
                }
            }

            // Update UI after loading complete
            this.Invoke((Action)(() =>
            {
                _isLoadingDevices = false;
                _progressBar.Visible = false;
                _lblStatus.Text = $"Fertig: {_allDevices.Count} Geräte geladen";
            }));
        }

        private void LoadNotificationClassObjects(BACnetDevice device)
        {
            try
            {
                // Get all objects from the device
                List<BacnetObjectId> objectList;
                uint count;
                if (!device.ReadObjectList(out objectList, out count))
                {
                    System.Diagnostics.Trace.WriteLine($"Fehler beim Laden der Objektliste von {device.deviceName}");
                    return;
                }

                if (objectList == null) return;

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

                        ObjectSelection objSelection = new ObjectSelection
                        {
                            Device = device,
                            ObjectId = objId,
                            ObjectName = objName
                        };

                        // ListViewItem direkt erstellen und cachen
                        ListViewItem item = new ListViewItem(new[]
                        {
                            device.deviceName,
                            objId.ToString(),
                            objName
                        });
                        item.Tag = objSelection;
                        item.Checked = true;

                        lock (_allObjects)
                        {
                            _allObjects.Add(objSelection);
                            _allObjectItems.Add(item);
                        }

                        // Sofort zur ListView hinzufügen (auf UI-Thread)
                        this.Invoke((Action)(() =>
                        {
                            _lvObjects.Items.Add(item);
                        }));
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
            }
        }

        private void UpdateObjectCheckboxes()
        {
            if (_lvObjects.InvokeRequired)
            {
                _lvObjects.Invoke((Action)(() => UpdateObjectCheckboxes()));
                return;
            }

            _lvObjects.BeginUpdate();
            _isUpdatingCheckboxes = true;
            try
            {
                // Get checked device IDs
                var checkedDeviceIds = _lvDevices.CheckedItems.Cast<ListViewItem>()
                    .Select(item => ((BACnetDevice)item.Tag).deviceId)
                    .ToHashSet();

                // Update checkboxes für alle NC-Objekte
                foreach (ListViewItem item in _lvObjects.Items)
                {
                    var objSelection = (ObjectSelection)item.Tag;
                    item.Checked = checkedDeviceIds.Contains(objSelection.Device.deviceId);
                }
            }
            finally
            {
                _isUpdatingCheckboxes = false;
                _lvObjects.EndUpdate();
            }
        }

        private void ChkSelectAllDevices_CheckedChanged()
        {
            if (_isUpdatingCheckboxes) return;

            _isUpdatingCheckboxes = true;
            foreach (ListViewItem item in _lvDevices.Items)
            {
                item.Checked = _chkSelectAllDevices.Checked;
            }
            _isUpdatingCheckboxes = false;

            UpdateObjectCheckboxes();
        }

        private void LvDevices_ItemChecked()
        {
            if (_isUpdatingCheckboxes || _isLoadingDevices) return;

            if (_lvDevices.Items.Count > 0)
            {
                bool allChecked = _lvDevices.Items.Cast<ListViewItem>().All(item => item.Checked);
                if (_chkSelectAllDevices.Checked != allChecked)
                {
                    _isUpdatingCheckboxes = true;
                    _chkSelectAllDevices.Checked = allChecked;
                    _isUpdatingCheckboxes = false;
                }
            }

            UpdateObjectCheckboxes();
        }

        private void ChkSelectAllObjects_CheckedChanged()
        {
            if (_isUpdatingCheckboxes) return;

            _isUpdatingCheckboxes = true;
            foreach (ListViewItem item in _lvObjects.Items)
            {
                item.Checked = _chkSelectAllObjects.Checked;
            }
            _isUpdatingCheckboxes = false;
        }

        private void LvObjects_ItemChecked()
        {
            if (_isUpdatingCheckboxes) return;

            // Update "Alle auswählen" checkbox für Objects
            if (_lvObjects.Items.Count > 0)
            {
                bool allChecked = _lvObjects.Items.Cast<ListViewItem>().All(item => item.Checked);
                if (_chkSelectAllObjects.Checked != allChecked)
                {
                    _isUpdatingCheckboxes = true;
                    _chkSelectAllObjects.Checked = allChecked;
                    _isUpdatingCheckboxes = false;
                }
            }

            // Auto-Abwahl von Geräten wenn alle NC abgewählt
            // Gruppiere NC-Objekte nach Device
            var deviceToObjects = new Dictionary<uint, List<ListViewItem>>();
            foreach (ListViewItem item in _lvObjects.Items)
            {
                var objSelection = (ObjectSelection)item.Tag;
                uint deviceId = objSelection.Device.deviceId;
                if (!deviceToObjects.ContainsKey(deviceId))
                    deviceToObjects[deviceId] = new List<ListViewItem>();
                deviceToObjects[deviceId].Add(item);
            }

            _isUpdatingCheckboxes = true;
            foreach (ListViewItem deviceItem in _lvDevices.Items)
            {
                var device = (BACnetDevice)deviceItem.Tag;
                if (deviceToObjects.ContainsKey(device.deviceId))
                {
                    // Prüfe ob alle NC-Objekte dieses Geräts abgewählt sind
                    bool anyChecked = deviceToObjects[device.deviceId].Any(obj => obj.Checked);
                    deviceItem.Checked = anyChecked;
                }
            }
            _isUpdatingCheckboxes = false;
        }

        private void ApplyFilterToObjects()
        {
            if (_lvObjects.InvokeRequired)
            {
                _lvObjects.Invoke((Action)(() => ApplyFilterToObjects()));
                return;
            }

            _lvObjects.BeginUpdate();
            try
            {
                string filterText = _txtFilterObjects.Text.ToLower().Trim();

                // Alle Items durchgehen und Sichtbarkeit setzen
                List<ListViewItem> visibleItems = new List<ListViewItem>();
                lock (_allObjects)
                {
                    foreach (var item in _allObjectItems)
                    {
                        var objSelection = (ObjectSelection)item.Tag;
                        // Nur Text filter - keine Device filter mehr!
                        bool visible = string.IsNullOrEmpty(filterText) ||
                                      objSelection.ObjectName.ToLower().Contains(filterText);

                        if (visible)
                            visibleItems.Add(item);
                    }
                }

                // ListView neu aufbauen mit gefilterten Items
                _lvObjects.Items.Clear();
                _lvObjects.Items.AddRange(visibleItems.ToArray());
            }
            finally
            {
                _lvObjects.EndUpdate();
            }
        }
    }
}
