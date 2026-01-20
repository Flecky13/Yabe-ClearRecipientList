## Erklärung der Checkboxen

- **Alle auswählen (Geräte):** Mit dieser Checkbox können alle angezeigten BACnet-Geräte auf einmal ausgewählt oder abgewählt werden. Ist sie aktiviert, werden alle Geräte in der Liste markiert. Wird sie deaktiviert, werden alle Markierungen entfernt.
- **Alle auswählen (Notification Class Objekte):** Diese Checkbox steuert die Auswahl aller angezeigten Notification Class Objekte. Sie funktioniert analog zur Geräte-Checkbox und ermöglicht das schnelle Markieren oder Abwählen aller Objekte.
- **Einzelne Checkboxen in den Listen:** Sowohl bei den Geräten als auch bei den Notification Class Objekten kann jedes Element einzeln über eine Checkbox ausgewählt oder abgewählt werden. Die Auswahl bestimmt, auf welche Geräte/Objekte die jeweilige Aktion (z.B. Recipient List leeren) angewendet wird.
- **Synchronisation:** Wird ein einzelnes Element abgewählt, wird die „Alle auswählen“-Checkbox automatisch deaktiviert. Sind alle Einträge manuell ausgewählt, wird die „Alle auswählen“-Checkbox wieder aktiviert.
# Beschreibung: ClearRecipientList Plugin für Yabe

## Zweck und Funktionsumfang

Das Plugin "ClearRecipientList" für Yabe dient der komfortablen Verwaltung und dem gezielten Leeren von Recipient Lists in BACnet Notification Class Objekten. Es richtet sich an Anwender, die viele BACnet-Geräte im Netzwerk verwalten und regelmäßig Empfängerkonfigurationen zurücksetzen oder bereinigen müssen.

### Hauptfunktionen
- **Automatische Gerätesuche:** Alle im Netzwerk gefundenen BACnet-Geräte werden übersichtlich angezeigt.
- **Anzeige relevanter Gerätedaten:** Geräte-ID, Name, Beschreibung und BACnet-Adresse werden dargestellt.
- **Notification Class Übersicht:** Für jedes Gerät werden die vorhandenen Notification Class Objekte und deren aktuelle Recipient Lists angezeigt.
- **Massenbearbeitung:** Mehrere Geräte und/oder Notification Classes können gleichzeitig ausgewählt und deren Recipient Lists in einem Schritt geleert werden.
- **Filterfunktion:** Die Listenansicht kann über ein Textfeld gefiltert werden. Die Filterung erfolgt live nach eingegebenem Text und durchsucht die Namen der Objekte. So lassen sich gezielt bestimmte Geräte oder Notification Classes finden.
- **Protokollierung:** Der Fortschritt und das Ergebnis der Löschvorgänge werden angezeigt.

### Typische Anwendungsfälle
- Netzwerkbereinigung nach Umbauten oder Migrationen
- Rücksetzen von Test- oder Demo-Konfigurationen
- Vorbereitung für Inbetriebnahmen

## Funktionsweise der Filterfunktion
Das Filterfeld oberhalb der Objektliste ermöglicht das schnelle Auffinden bestimmter Geräte oder Notification Classes. Die Filterung ist nicht case-sensitiv und sucht im Namen des jeweiligen Objekts. Sobald Text eingegeben wird, werden nur noch die passenden Einträge angezeigt. Ein leerer Filter zeigt wieder alle Objekte an.
