using System;
using System.Windows.Forms;

namespace ClearRecipientList
{
    public class HelpForm : Form
    {
        public HelpForm()
        {
            this.Text = "Hilfe zu ClearRecipientList";
            this.Size = new System.Drawing.Size(750, 650);
            var helpBox = new RichTextBox();
            helpBox.ReadOnly = true;
            helpBox.Dock = DockStyle.Fill;
            helpBox.Font = new System.Drawing.Font("Arial", 11);
            helpBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            helpBox.BorderStyle = BorderStyle.None;
            helpBox.Rtf =
                "{\\rtf1\\ansi\\deff0"
                + "{\\fonttbl{\\f0 Arial;}}"
                + "\\fs24 "
                + "{\\b ClearRecipientList Plugin}\\par"
                + "{\\f0 ==========================}\\par\\par"
                + "{\\f0 Dieses Plugin ermöglicht es, die Empfängerliste (Recipient List) eines BACnet-Geräts zu löschen.}\\par\\par"
                + "{\\b Verwendung:}\\par"
                + "{\\f0 -----------}\\par"
                + "{\\f0 1. Wählen Sie ein BACnet-Gerät in der Geräteliste aus.}\\par"
                + "{\\f0 2. Öffnen Sie das Kontextmenü und wählen Sie 'Clear Recipient List'.}\\par"
                + "{\\f0 3. Das Plugin entfernt alle Empfänger aus der Liste des Geräts.}\\par\\par"
                + "{\\b Hinweise:}\\par"
                + "{\\f0 ---------}\\par"
                + "{\\f0 - Die Funktion benötigt Schreibrechte auf das Zielgerät.}\\par"
                + "{\\f0 - Nicht alle Geräte unterstützen das Löschen der Empfängerliste.}\\par"
                + "{\\f0 - Änderungen können Auswirkungen auf Alarm- und Benachrichtigungsfunktionen haben.}\\par\\par"
                + "{\\b Support:}\\par"
                + "{\\f0 --------}\\par"
                + "{\\f0 Bei Fragen oder Problemen wenden Sie sich bitte an den Entwickler oder konsultieren Sie die Projektdokumentation.}\\par\\par"
                + "{\\b Lizenz:}\\par"
                + "{\\f0 Siehe LICENSE im GitHub-Repository}\\par\\par"
                + "{\\b Autor:}\\par"
                + "{\\f0 Flecky13 (Pedro Tepe)}\\par\\par"
                + "{\\b Repository:}\\par"
                + "{\\field{\\*\\fldinst HYPERLINK \\\"https://github.com/Flecky13/Yabe-ClearRecipientList\\\"}{\\fldrslt https://github.com/Flecky13/Yabe-ClearRecipientList}}\\par\\par"
                + "}";
            this.Controls.Add(helpBox);
        }
    }
}
