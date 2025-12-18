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
        private readonly YabeMainDialog _yabeFrm;
        private readonly List<BACnetDevice> _allDevices;
        private readonly List<ObjectSelection> _allObjects = new List<ObjectSelection>();
        private readonly List<ListViewItem> _allObjectItems = new List<ListViewItem>();

        private ListView _lvDevices;
        private ListView _lvObjects;
        private TextBox _txtFilterObjects;
        private CheckBox _chkSelectAllDevices;
        private CheckBox _chkSelectAllObjects;
        private ProgressBar _progressBar;
        private Label _lblStatus;

        private bool _isUpdatingCheckboxes;
        private bool _isLoadingDevices;

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
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            var leftPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0, 0, 5, 0)
            };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

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
            _lvDevices.Columns.Add("Device ID", 80);
            _lvDevices.Columns.Add("Name", 160);
            _lvDevices.Columns.Add("Description", 200);
            _lvDevices.Columns.Add("Adresse", 140);
            _lvDevices.ItemChecked += (s, e) => LvDevices_ItemChecked();
            leftPanel.Controls.Add(_lvDevices, 0, 2);

            var rightPanel = new TableLayoutPanel
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

            var lblObjects = new Label
            {
                Text = "Notification Class Objekte:",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5),
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblObjects, 0, 0);

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
            _lvObjects.Columns.Add("Gerät", 160);
            _lvObjects.Columns.Add("Objekt ID", 120);
            _lvObjects.Columns.Add("Name", 260);
            _lvObjects.ItemChecked += (s, e) => LvObjects_ItemChecked();
            rightPanel.Controls.Add(_lvObjects, 0, 3);

            var btnPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            btnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            btnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var leftBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0)
            };

            var rightBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0)
            };

            var btnDelete = new Button
            {
                Text = "Löschen",
                Width = 120,
                Margin = new Padding(0)
            };
            btnDelete.Click += (s, e) => ConfirmSelectionAndClose();

            var btnAddRecipient = new Button
            {
                Text = "Hinzufügen...",
                Width = 140,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnAddRecipient.Click += (s, e) => OnAddRecipientClicked();

            var btnClose = new Button
            {
                Text = "Schließen",
                Width = 110
            };
            btnClose.Click += (s, e) => Close();

            var btnCancel = new Button
            {
                Text = "Abbrechen",
                DialogResult = DialogResult.Cancel,
                Width = 110,
                Margin = new Padding(10, 0, 0, 0)
            };

            leftBtns.Controls.Add(btnDelete);
            leftBtns.Controls.Add(btnAddRecipient);
            rightBtns.Controls.Add(btnClose);
            rightBtns.Controls.Add(btnCancel);

            btnPanel.Controls.Add(leftBtns, 0, 0);
            btnPanel.Controls.Add(rightBtns, 1, 0);
            rightPanel.Controls.Add(btnPanel, 0, 4);

            var progressPanel = new TableLayoutPanel
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

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);
            mainLayout.Controls.Add(progressPanel, 0, 1);
            mainLayout.SetColumnSpan(progressPanel, 2);

            Controls.Add(mainLayout);
            AcceptButton = btnDelete;
            CancelButton = btnCancel;

            Shown += async (s, e) => await Task.Run(() => LoadDevicesIncrementallyAsync());
        }

        private void ConfirmSelectionAndClose()
        {
            SelectedDevices = _lvDevices.CheckedItems.Cast<ListViewItem>()
                .Select(it => (BACnetDevice)it.Tag)
                .ToList();
            SelectedObjects = _lvObjects.CheckedItems.Cast<ListViewItem>()
                .Select(it => (ObjectSelection)it.Tag)
                .ToList();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void LoadDevicesIncrementallyAsync()
        {
            _isLoadingDevices = true;

            if (_allDevices == null || _allDevices.Count == 0)
            {
                Invoke((Action)(() =>
                {
                    _lblStatus.Text = "Keine Geräte gefunden.";
                    _progressBar.Visible = false;
                    _isLoadingDevices = false;
                }));
                return;
            }

            Invoke((Action)(() =>
            {
                _progressBar.Visible = true;
                _progressBar.Maximum = Math.Max(1, _allDevices.Count);
                _progressBar.Value = 0;
                _lblStatus.Text = $"Lade Geräte... 0/{_allDevices.Count}";
            }));

            for (int i = 0; i < _allDevices.Count; i++)
            {
                var device = _allDevices[i];
                int idx = i;

                try
                {
                    var deviceObjectId = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device.deviceId);
                    string deviceName = device.ReadObjectName(deviceObjectId) ?? "(unbekannt)";

                    string description = string.Empty;
                    if (device.ReadPropertyRequest(deviceObjectId, BacnetPropertyIds.PROP_DESCRIPTION, out IList<BacnetValue> descValue))
                    {
                        if (descValue != null && descValue.Count > 0)
                            description = descValue[0].Value?.ToString() ?? string.Empty;
                    }

                    Invoke((Action)(() =>
                    {
                        var item = new ListViewItem(new[]
                        {
                            device.deviceId.ToString(),
                            deviceName,
                            description,
                            device.BacAdr.ToString()
                        })
                        {
                            Tag = device,
                            Checked = true
                        };
                        _lvDevices.Items.Add(item);

                        _progressBar.Value = Math.Min(_progressBar.Maximum, idx + 1);
                        _lblStatus.Text = $"Lade Geräte... {idx + 1}/{_allDevices.Count}";
                    }));

                    LoadNotificationClassObjects(device);
                }
                catch (Exception ex)
                {
                    Invoke((Action)(() =>
                    {
                        var item = new ListViewItem(new[]
                        {
                            device.deviceId.ToString(),
                            "(Fehler)",
                            ex.Message,
                            device.BacAdr.ToString()
                        })
                        {
                            Tag = device,
                            Checked = true
                        };
                        _lvDevices.Items.Add(item);

                        _progressBar.Value = Math.Min(_progressBar.Maximum, idx + 1);
                        _lblStatus.Text = $"Lade Geräte... {idx + 1}/{_allDevices.Count}";
                    }));
                    System.Diagnostics.Trace.WriteLine($"Fehler beim Laden von Device {device.deviceId}: {ex.Message}");
                }
            }

            Invoke((Action)(() =>
            {
                _isLoadingDevices = false;
                _progressBar.Visible = false;
                _lblStatus.Text = $"Fertig: {_allDevices.Count} Geräte geladen";
                ApplyFilterToObjects();
            }));
        }

        private void LoadNotificationClassObjects(BACnetDevice device)
        {
            try
            {
                if (!device.ReadObjectList(out List<BacnetObjectId> objectList, out uint _))
                    return;
                if (objectList == null)
                    return;

                var notificationObjects = objectList
                    .Where(o => o.type == BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS)
                    .ToList();

                foreach (var objId in notificationObjects)
                {
                    try
                    {
                        string objName = device.GetObjectName(objId) ?? objId.ToString();

                        var objSelection = new ObjectSelection
                        {
                            Device = device,
                            ObjectId = objId,
                            ObjectName = objName
                        };

                        var item = new ListViewItem(new[]
                        {
                            device.deviceName,
                            objId.ToString(),
                            objName
                        })
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
            try
            {
                if (_lvObjects == null || _lvDevices == null)
                    return;

                if (_lvObjects.InvokeRequired)
                {
                    _lvObjects.Invoke((Action)UpdateObjectCheckboxes);
                    return;
                }

                _lvObjects.BeginUpdate();
                _isUpdatingCheckboxes = true;
                try
                {
                    var checkedDeviceIds = _lvDevices.CheckedItems.Cast<ListViewItem>()
                        .Where(li => li?.Tag is BACnetDevice)
                        .Select(li => ((BACnetDevice)li.Tag).deviceId)
                        .ToHashSet();

                    foreach (ListViewItem item in _lvObjects.Items)
                    {
                        try
                        {
                            if (item?.Tag == null)
                                continue;

                            var objSelection = item.Tag as ObjectSelection;
                            if (objSelection?.Device == null)
                                continue;

                            item.Checked = checkedDeviceIds.Contains(objSelection.Device.deviceId);
                        }
                        catch
                        {
                            // Ignoriere fehlerhafte Items
                        }
                    }
                }
                finally
                {
                    _isUpdatingCheckboxes = false;
                    _lvObjects.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Fehler in UpdateObjectCheckboxes: {ex.Message}");
            }
        }

        private void ChkSelectAllDevices_CheckedChanged()
        {
            if (_isUpdatingCheckboxes) return;

            _isUpdatingCheckboxes = true;
            foreach (ListViewItem item in _lvDevices.Items)
                item.Checked = _chkSelectAllDevices.Checked;
            _isUpdatingCheckboxes = false;

            UpdateObjectCheckboxes();
        }

        private void LvDevices_ItemChecked()
        {
            try
            {
                if (_isUpdatingCheckboxes || _isLoadingDevices)
                    return;

                // Guard: Stelle sicher, dass die Controls initialisiert sind
                if (_lvDevices == null || _chkSelectAllDevices == null)
                    return;

                if (_lvDevices.Items.Count > 0)
                {
                    try
                    {
                        bool allChecked = _lvDevices.Items.Cast<ListViewItem>().All(li => li?.Checked ?? false);
                        if (_chkSelectAllDevices.Checked != allChecked)
                        {
                            _isUpdatingCheckboxes = true;
                            _chkSelectAllDevices.Checked = allChecked;
                            _isUpdatingCheckboxes = false;
                        }
                    }
                    catch
                    {
                        // Ignoriere Fehler bei der All-Abfrage
                    }
                }

                UpdateObjectCheckboxes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Fehler in LvDevices_ItemChecked: {ex.Message}");
            }
        }

        private void ChkSelectAllObjects_CheckedChanged()
        {
            if (_isUpdatingCheckboxes) return;

            _isUpdatingCheckboxes = true;
            foreach (ListViewItem item in _lvObjects.Items)
                item.Checked = _chkSelectAllObjects.Checked;
            _isUpdatingCheckboxes = false;

            LvObjects_ItemChecked();
        }

        private void LvObjects_ItemChecked()
        {
            try
            {
                if (_isUpdatingCheckboxes)
                    return;

                // Guard: Stelle sicher, dass die Controls initialisiert sind
                if (_lvObjects == null || _lvDevices == null || _chkSelectAllObjects == null)
                    return;

                if (_lvObjects.Items.Count > 0)
                {
                    try
                    {
                        bool allChecked = _lvObjects.Items.Cast<ListViewItem>().All(li => li?.Checked ?? false);
                        if (_chkSelectAllObjects.Checked != allChecked)
                        {
                            _isUpdatingCheckboxes = true;
                            _chkSelectAllObjects.Checked = allChecked;
                            _isUpdatingCheckboxes = false;
                        }
                    }
                    catch
                    {
                        // Ignoriere Fehler bei der All-Abfrage
                    }
                }

                var deviceToObjects = new Dictionary<uint, List<ListViewItem>>();
                foreach (ListViewItem item in _lvObjects.Items)
                {
                    try
                    {
                        if (item?.Tag == null)
                            continue;

                        var objSelection = item.Tag as ObjectSelection;
                        if (objSelection?.Device == null)
                            continue;

                        if (!deviceToObjects.TryGetValue(objSelection.Device.deviceId, out var list))
                        {
                            list = new List<ListViewItem>();
                            deviceToObjects[objSelection.Device.deviceId] = list;
                        }
                        list.Add(item);
                    }
                    catch
                    {
                        // Ignoriere fehlerhafte Items
                    }
                }

                _isUpdatingCheckboxes = true;
                foreach (ListViewItem deviceItem in _lvDevices.Items)
                {
                    try
                    {
                        if (deviceItem?.Tag == null)
                            continue;

                        var device = deviceItem.Tag as BACnetDevice;
                        if (device == null)
                            continue;

                        if (deviceToObjects.TryGetValue(device.deviceId, out var objItems))
                        {
                            bool anyChecked = objItems.Any(o => o?.Checked ?? false);
                            deviceItem.Checked = anyChecked;
                        }
                    }
                    catch
                    {
                        // Ignoriere fehlerhafte DeviceItems
                    }
                }
                _isUpdatingCheckboxes = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Fehler in LvObjects_ItemChecked: {ex.Message}");
            }
        }

        private void ApplyFilterToObjects()
        {
            if (_lvObjects.InvokeRequired)
            {
                _lvObjects.Invoke((Action)ApplyFilterToObjects);
                return;
            }

            _lvObjects.BeginUpdate();
            try
            {
                string filterText = _txtFilterObjects.Text.ToLowerInvariant().Trim();

                var visibleItems = new List<ListViewItem>();
                lock (_allObjects)
                {
                    foreach (var item in _allObjectItems)
                    {
                        var objSelection = (ObjectSelection)item.Tag;
                        bool visible = string.IsNullOrEmpty(filterText) ||
                                       objSelection.ObjectName.ToLowerInvariant().Contains(filterText);
                        if (visible)
                            visibleItems.Add(item);
                    }
                }

                _lvObjects.Items.Clear();
                _lvObjects.Items.AddRange(visibleItems.ToArray());
            }
            finally
            {
                _lvObjects.EndUpdate();
            }
        }

        private void OnAddRecipientClicked()
        {
            using (var dlg = new BulkAddRecipientForm())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                var newRecipient = dlg.BuildRecipient();
                var targets = _lvObjects.Items.Cast<ListViewItem>()
                    .Select(it => (ObjectSelection)it.Tag)
                    .ToList();

                if (targets.Count == 0)
                {
                    MessageBox.Show(this, "Keine sichtbaren Notification Class Objekte.", "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _lblStatus.Text = $"Füge Empfänger zu {targets.Count} NC-Objekt(en) hinzu...";

                foreach (var t in targets)
                {
                    try
                    {
                        if (!t.Device.ReadPropertyRequest(t.ObjectId, BacnetPropertyIds.PROP_RECIPIENT_LIST, out IList<BacnetValue> existing))
                            existing = null;

                        var writeList = new List<BacnetValue>();

                        if (existing != null && existing.Count > 0)
                        {
                            int count = existing.Count / 7;
                            for (int i = 0; i < count; i++)
                            {
                                var dr = new DeviceReportingRecipient(
                                    existing[i * 7 + 0],
                                    existing[i * 7 + 1],
                                    existing[i * 7 + 2],
                                    existing[i * 7 + 3],
                                    existing[i * 7 + 4],
                                    existing[i * 7 + 5],
                                    existing[i * 7 + 6]
                                );
                                writeList.Add(new BacnetValue(dr));
                            }
                        }

                        writeList.Add(new BacnetValue(newRecipient));
                        t.Device.WritePropertyRequest(t.ObjectId, BacnetPropertyIds.PROP_RECIPIENT_LIST, writeList);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Fehler beim Hinzufügen des Empfängers zu {t.Device.deviceName} {t.ObjectId}: {ex.Message}");
                    }
                }

                _lblStatus.Text = $"Empfänger hinzugefügt für {targets.Count} NC-Objekt(e).";
                MessageBox.Show(this, $"Empfänger wurde {targets.Count} mal hinzugefügt.", "Fertig", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
