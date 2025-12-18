using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.BACnet;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yabe;

namespace ClearRecipientList
{
    public partial class SelectItemsForm : Form
    {
        // State: Verhindere Event-Verarbeitung während Init
        private bool _isInitialized = false;
        private bool _isUpdatingUI = false;
        private bool _isLoadingDevices = false;

        // Models
        private readonly YabeMainDialog _yabeFrm;
        private readonly List<BACnetDevice> _allDevices;
        private readonly List<ObjectSelection> _allObjects = new List<ObjectSelection>();
        private readonly List<ListViewItem> _allObjectItems = new List<ListViewItem>();

        // Controls
        private ListView _lvDevices;
        private ListView _lvObjects;
        private TextBox _txtFilterObjects;
        private CheckBox _chkSelectAllDevices;
        private CheckBox _chkSelectAllObjects;
        private ProgressBar _progressBar;
        private Label _lblStatus;

        // Output
        public List<BACnetDevice> SelectedDevices { get; private set; } = new List<BACnetDevice>();
        public List<ObjectSelection> SelectedObjects { get; private set; } = new List<ObjectSelection>();

        public SelectItemsForm(YabeMainDialog yabeFrm)
        {
            _yabeFrm = yabeFrm ?? throw new ArgumentNullException(nameof(yabeFrm));
            var discovered = _yabeFrm.YabeDiscoveredDevices;
            _allDevices = discovered != null ? new List<BACnetDevice>(discovered) : new List<BACnetDevice>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Geräte und Notification Class Objekte auswählen";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;

            // MAIN LAYOUT: 2x Spalten (Devices | Objects) + Progress + Buttons
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Lists
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Progress
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Buttons

            // ================== LEFT PANEL: DEVICES ==================
            var leftPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(0, 0, 5, 0)
            };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Title
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Checkbox
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // Spacer
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // ListView

            var lblDevices = new Label
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
            leftPanel.Controls.Add(_chkSelectAllDevices, 0, 1);

            // Spacer
            leftPanel.Controls.Add(new Label { }, 0, 2);

            _lvDevices = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };
            _lvDevices.Columns.Add("Device ID", 80);
            _lvDevices.Columns.Add("Name", 160);
            _lvDevices.Columns.Add("Description", 200);
            _lvDevices.Columns.Add("Adresse", 140);
            leftPanel.Controls.Add(_lvDevices, 0, 3);

            // ================== RIGHT PANEL: NC OBJECTS ==================
            var rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(5, 0, 0, 0)
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Title
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Checkbox + Filter (combined)
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // Spacer (to match left)
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // ListView

            var lblObjects = new Label
            {
                Text = "Notification Class Objekte:",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5),
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblObjects, 0, 0);

            // Row 1: Checkbox + Filter combined
            var row1Panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0),
                AutoSize = true
            };
            row1Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row1Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row1Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            row1Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _chkSelectAllObjects = new CheckBox
            {
                Text = "Alle auswählen",
                AutoSize = true,
                Checked = true,
                Margin = new Padding(0, 0, 5, 10)
            };
            row1Panel.Controls.Add(_chkSelectAllObjects, 0, 0);

            var lblFilter = new Label
            {
                Text = "Filter:",
                AutoSize = true,
                Margin = new Padding(0, 2, 5, 10)
            };
            row1Panel.Controls.Add(lblFilter, 1, 0);

            _txtFilterObjects = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Height = 20
            };
            row1Panel.Controls.Add(_txtFilterObjects, 1, 1);

            rightPanel.Controls.Add(row1Panel, 0, 1);

            // Spacer (Row 2)
            rightPanel.Controls.Add(new Label { }, 0, 2);

            _lvObjects = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };
            _lvObjects.Columns.Add("Gerät", 160);
            _lvObjects.Columns.Add("Objekt ID", 120);
            _lvObjects.Columns.Add("Name", 260);
            rightPanel.Controls.Add(_lvObjects, 0, 3);

            // Add left and right panels to main layout
            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);

            // ================== PROGRESS BAR ==================
            var progressPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Height = 50
            };
            progressPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            progressPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _lblStatus = new Label
            {
                Text = string.Empty,
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

            // ================== BUTTONS ==================
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Height = 40,
                Margin = new Padding(0, 10, 0, 0)
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));  // Leeren
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));  // Hinzufügen
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // Spacer
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));  // Abbrechen
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));  // Schließen

            var btnClear = new Button
            {
                Text = "Leeren",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 5, 0)
            };
            btnClear.Click += (s, e) => OnClearClicked();
            buttonPanel.Controls.Add(btnClear, 0, 0);

            var btnAddRecipient = new Button
            {
                Text = "Hinzufügen",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 5, 0)
            };
            btnAddRecipient.Click += (s, e) => OnAddRecipientClicked();
            buttonPanel.Controls.Add(btnAddRecipient, 1, 0);

            // Spacer
            buttonPanel.Controls.Add(new Label { }, 2, 0);

            var btnCancel = new Button
            {
                Text = "Abbrechen",
                DialogResult = DialogResult.Cancel,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 5, 0)
            };
            buttonPanel.Controls.Add(btnCancel, 3, 0);

            var btnClose = new Button
            {
                Text = "Schließen",
                Dock = DockStyle.Fill
            };
            btnClose.Click += (s, e) => Close();
            buttonPanel.Controls.Add(btnClose, 4, 0);

            mainLayout.Controls.Add(buttonPanel, 0, 2);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            Controls.Add(mainLayout);
            CancelButton = btnCancel;

            // Register events AFTER initialization
            _chkSelectAllDevices.CheckedChanged += OnChkSelectAllDevices_CheckedChanged;
            _lvDevices.ItemChecked += (s, e) => OnLvDevices_ItemChecked();
            _chkSelectAllObjects.CheckedChanged += OnChkSelectAllObjects_CheckedChanged;
            _lvObjects.ItemChecked += (s, e) => OnLvObjects_ItemChecked();
            _txtFilterObjects.TextChanged += (s, e) => OnTxtFilterObjects_TextChanged();

            _isInitialized = true;

            Shown += async (s, e) => await Task.Run(() => LoadDevicesAsync());
        }

        private void OnClearClicked()
        {
            var selectedObjects = _lvObjects.CheckedItems.Cast<ListViewItem>()
                .Where(it => it?.Tag is ObjectSelection)
                .Select(it => (ObjectSelection)it.Tag)
                .ToList();

            if (selectedObjects.Count == 0)
            {
                MessageBox.Show(this, "Keine Objekte ausgewählt.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, $"Möchten Sie die Recipient Lists von {selectedObjects.Count} Objekt(en) wirklich löschen?", "Bestätigung", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _progressBar.Visible = true;
            _progressBar.Maximum = selectedObjects.Count;
            _progressBar.Value = 0;
            _lblStatus.Text = $"Leere {selectedObjects.Count} NC-Objekt(e)...";
            Application.DoEvents();

            int processed = 0;
            foreach (var objSel in selectedObjects)
            {
                try
                {
                    var emptyRecipientList = new System.IO.BACnet.BacnetValue[0];
                    bool success = objSel.Device.channel.WritePropertyRequest(
                        objSel.Device.BacAdr,
                        objSel.ObjectId,
                        System.IO.BACnet.BacnetPropertyIds.PROP_RECIPIENT_LIST,
                        emptyRecipientList);

                    if (success)
                    {
                        System.Diagnostics.Trace.WriteLine($"Erfolgreich geleert: {objSel.Device.deviceName} - {objSel.ObjectId}");
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine($"Fehler: WriteProperty fehlgeschlagen für {objSel.Device.deviceName} - {objSel.ObjectId}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Fehler beim Löschen: {objSel.Device.deviceName} - {objSel.ObjectId}: {ex.Message}");
                }

                processed++;
                _progressBar.Value = processed;
                _lblStatus.Text = $"Leere... {processed}/{selectedObjects.Count}";
                Application.DoEvents();
            }

            _progressBar.Visible = false;
            _lblStatus.Text = $"Zuletzt {processed} Objekte geleert.";
            MessageBox.Show(this, $"Fertig: {processed} Objekte geleert.",
                "Erfolg",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void LoadDevicesAsync()
        {
            if (_allDevices == null || _allDevices.Count == 0)
                return;

            _isLoadingDevices = true;
            _isUpdatingUI = true;

            try
            {
                Invoke((Action)(() =>
                {
                    _progressBar.Visible = true;
                    _progressBar.Maximum = _allDevices.Count;
                    _progressBar.Value = 0;
                    _lblStatus.Text = $"Lade Geräte...";
                }));

                for (int i = 0; i < _allDevices.Count; i++)
                {
                    var device = _allDevices[i];
                    int idx = i;

                    try
                    {
                        string deviceName = device.ReadObjectName(new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device.deviceId)) ?? "(unbekannt)";
                        string description = "";
                        if (device.ReadPropertyRequest(new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device.deviceId),
                            BacnetPropertyIds.PROP_DESCRIPTION, out IList<BacnetValue> descValue) && descValue?.Count > 0)
                            description = descValue[0].Value?.ToString() ?? "";

                        Invoke((Action)(() =>
                        {
                            var item = new ListViewItem(new[] { device.deviceId.ToString(), deviceName, description, device.BacAdr.ToString() })
                            {
                                Tag = device,
                                Checked = true
                            };
                            _lvDevices.Items.Add(item);
                            _progressBar.Value = idx + 1;
                            _lblStatus.Text = $"Lade Geräte... {idx + 1}/{_allDevices.Count}";
                        }));

                        LoadNotificationObjects(device);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Fehler beim Laden von Device {device.deviceId}: {ex.Message}");
                    }
                }

                Invoke((Action)(() =>
                {
                    _progressBar.Visible = false;
                    _lblStatus.Text = "Fertig";
                }));
            }
            finally
            {
                _isLoadingDevices = false;
                _isUpdatingUI = false;
            }
        }

        private void LoadNotificationObjects(BACnetDevice device)
        {
            try
            {
                if (!device.ReadObjectList(out List<BacnetObjectId> objectList, out uint _) || objectList == null)
                    return;

                var ncObjects = objectList.Where(o => o.type == BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS).ToList();

                foreach (var objId in ncObjects)
                {
                    try
                    {
                        string objName = device.GetObjectName(objId) ?? objId.ToString();
                        var objSelection = new ObjectSelection { Device = device, ObjectId = objId, ObjectName = objName };
                        var item = new ListViewItem(new[] { device.deviceName, objId.ToString(), objName })
                        {
                            Tag = objSelection,
                            Checked = true
                        };

                        lock (_allObjects)
                        {
                            _allObjects.Add(objSelection);
                            _allObjectItems.Add(item);
                        }

                        Invoke((Action)(() => _lvObjects.Items.Add(item)));
                    }
                    catch
                    {
                        // Ignoriere fehlerhafte Objekte
                    }
                }
            }
            catch
            {
                // Ignoriere Device-Fehler
            }
        }

        // EVENT HANDLER - alle mit Guard gegen _isInitialized
        private void OnChkSelectAllDevices_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitialized || _isUpdatingUI)
                return;

            _isUpdatingUI = true;
            try
            {
                foreach (ListViewItem item in _lvDevices.Items)
                    if (item != null)
                        item.Checked = _chkSelectAllDevices.Checked;
            }
            finally { _isUpdatingUI = false; }
        }

        private void OnLvDevices_ItemChecked()
        {
            if (!_isInitialized || _isUpdatingUI || _isLoadingDevices)
                return;

            _isUpdatingUI = true;
            try
            {
                bool allChecked = _lvDevices.Items.Cast<ListViewItem>().Count(li => li?.Checked ?? false) == _lvDevices.Items.Count;
                _chkSelectAllDevices.Checked = allChecked;

                // Device-checkbox bestimmt Sicht der NC-Objekte
                SyncObjectsToDevices();
            }
            finally { _isUpdatingUI = false; }
        }

        private void OnChkSelectAllObjects_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitialized || _isUpdatingUI)
                return;

            _isUpdatingUI = true;
            try
            {
                foreach (ListViewItem item in _lvObjects.Items)
                    if (item != null)
                        item.Checked = _chkSelectAllObjects.Checked;
            }
            finally { _isUpdatingUI = false; }
        }

        private void OnLvObjects_ItemChecked()
        {
            if (!_isInitialized || _isUpdatingUI)
                return;

            _isUpdatingUI = true;
            try
            {
                bool allChecked = _lvObjects.Items.Cast<ListViewItem>().Count(li => li?.Checked ?? false) == _lvObjects.Items.Count;
                _chkSelectAllObjects.Checked = allChecked;

                // Auto-uncheck Devices wenn alle ihre NC-Items unchecked sind
                SyncDevicesToObjects();
            }
            finally { _isUpdatingUI = false; }
        }

        private void OnTxtFilterObjects_TextChanged()
        {
            if (!_isInitialized)
                return;

            string filter = _txtFilterObjects.Text.ToLowerInvariant().Trim();
            _lvObjects.BeginUpdate();
            try
            {
                _lvObjects.Items.Clear();
                foreach (var item in _allObjectItems)
                {
                    if (item?.Tag is ObjectSelection sel && (string.IsNullOrEmpty(filter) || sel.ObjectName.ToLowerInvariant().Contains(filter)))
                        _lvObjects.Items.Add(item);
                }
            }
            finally { _lvObjects.EndUpdate(); }
        }

        private void SyncObjectsToDevices()
        {
            var checkedDeviceIds = _lvDevices.CheckedItems.Cast<ListViewItem>()
                .Where(li => li?.Tag is BACnetDevice)
                .Select(li => ((BACnetDevice)li.Tag).deviceId)
                .ToHashSet();

            foreach (ListViewItem item in _lvObjects.Items)
            {
                if (item?.Tag is ObjectSelection sel && sel.Device != null)
                    item.Checked = checkedDeviceIds.Contains(sel.Device.deviceId);
            }
        }

        private void SyncDevicesToObjects()
        {
            var deviceToHasChecked = new Dictionary<uint, bool>();
            foreach (ListViewItem item in _lvObjects.Items)
            {
                if (item?.Tag is ObjectSelection sel && sel.Device != null)
                {
                    if (!deviceToHasChecked.ContainsKey(sel.Device.deviceId))
                        deviceToHasChecked[sel.Device.deviceId] = item.Checked;
                    else if (item.Checked)
                        deviceToHasChecked[sel.Device.deviceId] = true;
                }
            }

            foreach (ListViewItem deviceItem in _lvDevices.Items)
            {
                if (deviceItem?.Tag is BACnetDevice dev && deviceToHasChecked.TryGetValue(dev.deviceId, out bool hasChecked))
                    deviceItem.Checked = hasChecked;
            }
        }

        private void OnAddRecipientClicked()
        {
            using (var dlg = new BulkAddRecipientForm())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                var newRecipient = dlg.BuildRecipient();
                var targets = _lvObjects.CheckedItems.Cast<ListViewItem>()
                    .Where(it => it?.Tag is ObjectSelection)
                    .Select(it => (ObjectSelection)it.Tag)
                    .ToList();

                if (targets.Count == 0)
                {
                    MessageBox.Show(this, "Keine sichtbaren Notification Class Objekte.", "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _progressBar.Visible = true;
                _progressBar.Maximum = targets.Count;
                _progressBar.Value = 0;
                _lblStatus.Text = $"Füge Empfänger zu {targets.Count} NC-Objekt(en) hinzu...";
                Application.DoEvents();

                int processed = 0;
                foreach (var t in targets)
                {
                    try
                    {
                        if (!t.Device.ReadPropertyRequest(t.ObjectId, BacnetPropertyIds.PROP_RECIPIENT_LIST, out IList<BacnetValue> existing))
                            existing = null;

                        var writeList = new List<BacnetValue>();
                        if (existing?.Count > 0)
                        {
                            int count = existing.Count / 7;
                            for (int i = 0; i < count; i++)
                            {
                                var dr = new DeviceReportingRecipient(existing[i * 7 + 0], existing[i * 7 + 1], existing[i * 7 + 2],
                                    existing[i * 7 + 3], existing[i * 7 + 4], existing[i * 7 + 5], existing[i * 7 + 6]);
                                writeList.Add(new BacnetValue(dr));
                            }
                        }

                        writeList.Add(new BacnetValue(newRecipient));
                        t.Device.WritePropertyRequest(t.ObjectId, BacnetPropertyIds.PROP_RECIPIENT_LIST, writeList);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Fehler beim Hinzufügen: {ex.Message}");
                    }

                    processed++;
                    _progressBar.Value = processed;
                    _lblStatus.Text = $"Füge Empfänger hinzu... {processed}/{targets.Count}";
                    Application.DoEvents();
                }

                _progressBar.Visible = false;
                _lblStatus.Text = $"Empfänger hinzugefügt für {targets.Count} NC-Objekt(e).";
                MessageBox.Show(this, $"Empfänger wurde {targets.Count} mal hinzugefügt.", "Fertig", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
