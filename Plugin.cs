/**************************************************************************
*                           MIT License
*
* Custom Plugin for clearing recipient lists from Notification Class Objects
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
*
* For simplicity all this code can be tested directly into Yabe project
* before exporting it
*
*********************************************************************/

using System;
using System.Windows.Forms;
using Yabe;

namespace ClearRecipientList
{
    public class Plugin : IYabePlugin
    {
        private YabeMainDialog _yabeFrm;

        public void Init(YabeMainDialog yabeFrm)
        {
            this._yabeFrm = yabeFrm;

            // Creates the Menu Item
            ToolStripMenuItem menuItem = new ToolStripMenuItem();
            menuItem.Text = "Clear Recipient Lists";
            menuItem.Click += MenuItem_Click;

            // Add it as a sub menu (pluginsToolStripMenuItem is the only public Menu member)
            yabeFrm.pluginsToolStripMenuItem.DropDownItems.Add(menuItem);
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Combined window: Select devices and notification class objects
                SelectItemsForm itemsForm = new SelectItemsForm(_yabeFrm);
                if (itemsForm.ShowDialog() == DialogResult.OK)
                {
                    // Execute the clearing operation
                    ClearRecipientLists(itemsForm.SelectedObjects);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}\n\n{ex.StackTrace}",
                    "Plugin Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ClearRecipientLists(System.Collections.Generic.List<ObjectSelection> selectedObjects)
        {
            if (selectedObjects.Count == 0)
            {
                MessageBox.Show("Keine Objekte ausgewählt.", "Information");
                return;
            }

            var progressForm = new ProgressForm();
            progressForm.Show();

            try
            {
                int processed = 0;
                foreach (var objSel in selectedObjects)
                {
                    progressForm.UpdateProgress(processed, selectedObjects.Count,
                        $"Verarbeite: {objSel.Device.deviceName} - {objSel.ObjectId}");

                    try
                    {
                        // Clear the Recipient List by writing an empty list
                        // PROP_RECIPIENT_LIST = 54
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
                }

                progressForm.Close();
                MessageBox.Show($"Verarbeitung abgeschlossen: {processed} Objekte verarbeitet.",
                    "Erfolgreich",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                progressForm?.Dispose();
            }
        }
    }

    /// <summary>
    /// Helper class to hold device and object selection data
    /// </summary>
    public class ObjectSelection
    {
        public Yabe.BACnetDevice Device { get; set; }
        public System.IO.BACnet.BacnetObjectId ObjectId { get; set; }
        public string ObjectName { get; set; }
    }
}
