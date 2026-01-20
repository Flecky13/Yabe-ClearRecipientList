# Clear Recipient List Plugin für Yabe

Ein Yabe-Plugin zur effizienten Verwaltung und Leerung von Notification Class Recipient Lists in BACnet-Netzwerken.

## Funktionalität

Dieses Plugin ermöglicht es, die Recipient Lists von Notification Class Objekten in BACnet-Geräten massenhaft zu leeren. Dies ist besonders nützlich für:

- **Netzwerk-Bereinigung** - Entfernen ungültiger Empfänger nach Änderungen im Netzwerk
- **Wartung** - Zurücksetzen von Benachrichtigungskonfigurationen auf Standard
- **Migration** - Vorbereitung für Umzug zu neuen Systemen

### Hauptfeatures

✅ **Geräteverwaltung**
- Alle entdeckten BACnet-Geräte werden automatisch aufgelistet
- Anzeige von Geräte-ID, Name, Beschreibung und BACnet-Adresse

## Installation

### Voraussetzungen
- Yabe muss installiert sein (oder aus dem Quellcode kompiliert)
- .NET Framework 4.8+

### Installationsschritte

1. **Plugin-Datei herunterladen oder kompilieren**
   - Release von GitHub herunterladen: `ClearRecipientList.dll`
   - ODER selbst kompilieren.

2. **Plugin ins Yabe-Verzeichnis kopieren**
    - Finde dein Yabe-Installationsverzeichnis
    - Erstelle einen `Plugins`-Ordner falls noch nicht vorhanden:
       ```
       C:\Program Files\Yabe\Plugins\
       ```
    - Kopiere `ClearRecipientList.dll` in diesen `Plugins`-Ordner

3. **Config-Datei erstellen oder anpassen**
   - Prüfe ob `Yabe.exe.config` im Yabe-Verzeichnis existiert
   - **Falls nicht:** Erstelle oder Kopieren diese aus dem Yabe Repository
         wenn kopiert, Berechtigungen überprüfen, Rechtklich auf .config Eigenschaften -> Tab Allgemein -> Sicherheit Zulassen
   - erweitere die Liste der PlugIns
   ```xml
       <setting name="Plugins" serializeAs="String">
          <value>..., ...,  ..., ClearRecipientList</value>
        </setting>

   ```

4. **Yabe neu starten**
   - Yabe komplett beenden
   - Yabe neu starten
   - Das Plugin erscheint im Menü: `Plugins` → `Clear Recipient Lists`
   - Falls das Menü nicht erscheint → Berechtigungen überprüfen, Rechtklich auf .dll Eigenschaften -> Tab Allgemein -> Sicherheit Zulassen


Lizenz & Kontakt
Siehe LICENSE im Repository. Für Fragen zum Code bitte Issues/PRs im Repo verwenden.

https://buymeacoffee.com/pedrotepe
