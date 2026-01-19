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
using System.Reflection;
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
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "1.0.0.0";
            menuItem.Text = $"Clear Recipient Lists (v{version})";
            menuItem.Click += MenuItem_Click;

            // Add it as a sub menu (pluginsToolStripMenuItem is the only public Menu member)
            yabeFrm.pluginsToolStripMenuItem.DropDownItems.Add(menuItem);
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_yabeFrm == null)
                {
                    MessageBox.Show("Yabe-Fenster nicht verfügbar.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var devices = _yabeFrm.YabeDiscoveredDevices;
                if (devices == null || devices.Length == 0)
                {
                    MessageBox.Show("Keine BACnet-Geräte gefunden. Bitte starten Sie zunächst eine Geräteerkennung.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SelectItemsForm itemsForm = new SelectItemsForm(_yabeFrm);
                itemsForm.ShowDialog();
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show($"Nullfehler: {ex.Message}\n\n{ex.StackTrace}\n\nBitte stellen Sie sicher, dass Geräte entdeckt wurden.",
                    "Nullfehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}\n\n{ex.StackTrace}",
                    "Plugin Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
