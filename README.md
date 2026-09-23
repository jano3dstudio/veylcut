# VEYLCUT · Video Studio 0.1.0

Lokaler Windows-Prototyp für Jona. **VEYLCUT ist ein vorläufiger Arbeitstitel.** Markenregister- und Ähnlichkeitsprüfung noch offen; keine öffentliche Veröffentlichung. Eigenes Icon als SVG, PNG und echtes ICO mit 16/24/32/48/64/128/256 px.

## Start

`VEYLCUT.exe` im Auslieferungsordner doppelklicken. Quellcode und Build liegen unter `_Projekt`. Auf diesem Rechner wurden Microsoft Edge WebView2 Runtime und FFmpeg 8.1.1 erfolgreich verwendet. FFmpeg / FFprobe werden im `engine`-Unterordner neben der EXE, über PATH oder in der bestehenden Gyan-WinGet-Installation gesucht. Sie werden nicht neu installiert oder mitverteilt. Ohne FFmpeg zeigt die App einen Hinweis; Export und Medienanalyse benötigen es.

Die Anwendung bleibt lokal. Kein Server, Login, Cloud-Konto oder API-Schlüssel notwendig. Der Autorenlink öffnet auf Wunsch LinkedIn im Browser.

## Ein vollständiger Ablauf

1. **Video laden** oder das ausdrücklich synthetische Testvideo verwenden.
2. Links Intro, rechts Outro wählen. Signal, Frame und Minimal sind eingebaute transparente Grafikvorlagen. Eigene PNGs und Alpha-Videos können importiert werden; MOV-Alpha wird beim Rendern verarbeitet.
3. Text für den Hauptteil eingeben, Schrift und Position auswählen. TTF/OTF/WOFF/WOFF2 lassen sich in die Schriftbibliothek importieren.
4. Unter dem Video den gewünschten Schnitt beschreiben und **Schnitt erstellen** drücken. Action erzeugt kurze, Showcase längere Ausschnitte. „15 Sekunden“ im Prompt setzt beim Erstellen die Ziellänge. „Ungeschnitten“ erhält einen zusammenhängenden Ausschnitt.
5. Format wählen: 1920×1080, 1080×1920, 1080×1080 oder 1080×1350. Bild füllen (mittig beschneiden) oder vollständig einpassen.
6. Schutzbereich wählen und bei Bedarf die vier Ränder in Prozent ändern. Eine zusätzliche PNG ist eine visuelle Referenz. Die erzeugten Texte passen in den verbleibenden Bereich. Hilfsmarkierungen werden niemals ausgegeben.
7. Eigene Musik und optional einen Outro-Sound importieren, Lautstärken einstellen.
8. **Vorschau rendern** erzeugt eine abspielbare MP4 in reduzierter Auflösung mit dem vollständigen Schnitt, allen Ebenen und dem Audio-Mix.
9. **MP4 exportieren** schreibt H.264/AAC in der gewählten Auflösung an den gewählten Ort. Vorhandene Dateien werden nur nach dem Speicherdialog ersetzt. Ein Abbruch verändert keine Quelldatei und hinterlässt keine halbfertige Datei am Ausgabeziel.

Intro und Outro liegen über dem ersten bzw. letzten Teil des geschnittenen Videos, maximal je zwei Sekunden (bei kurzen Clips je 20 %). Der Haupttext liegt im mittleren Abschnitt. Transparenz muss im importierten Asset vorhanden sein; ein undurchsichtiges Video verdeckt das Footage. Importierte Overlays werden passend eingepasst; ihr eigener Ton wird nicht gemischt. Intro-/Outro-Textfelder gelten für die eingebauten Vorlagen.

## Projekte und Bibliotheken

- **Projekt speichern / Öffnen**: `.vey` speichert Einstellungen, Schnittliste und Referenzen auf die im Projekt verwendeten Medien. Es kopiert kein großes Quellvideo. Fehlende Quelldateien werden gemeldet. Für einen Rechnerwechsel die referenzierten Dateien zusätzlich mitnehmen.
- Importierte Library-Assets werden in `%LOCALAPPDATA%\JANO\Veylcut\library` kopiert, damit die Bibliothek nach einem Neustart erhalten bleibt. Das geladene Hauptvideo bleibt an seinem ursprünglichen Ort.
- Vorschau-Dateien und Render-Zwischendaten liegen unter `%LOCALAPPDATA%\JANO\Veylcut\renders`; es gibt noch keine automatische Cache-Bereinigung.
- Look, Sprache und Formulareinstellungen liegen im lokalen WebView2-Profil. Ein Neustart lädt das Quellvideo erst nach explizitem Öffnen/Laden.
- Ein Projekt auf dem Laufwerk ist die dauerhafte Sicherung eines Schnitts. Gespeicherte Bibliotheken und Einstellungen allein ersetzen es nicht.

## Bewusste Grenzen dieser ersten Version

- Der Prompt steuert **lokale Regeln für Tempo und Länge**, keine semantische KI. Die App erkennt noch keine Kills, Siege, Höhepunkte oder Handlungsbögen und synchronisiert Schnitte noch nicht mit Musikbeats.
- Die Arbeitsvorschau zeigt Video, Grafik/Text und Schutzbereiche. Der vollständige Tonmix und exotische Alpha-Codecs werden in der gerenderten Vorschau geprüft. Wenn der Browser den Quellcodec nicht abspielen kann, bleibt FFmpeg-Vorschau/Export nutzbar.
- Eine PNG-Schutzmaske wird angezeigt, aber nicht automatisch in Ausschlussgeometrie umgerechnet. Die vier Randwerte sind die wirksame Textbegrenzung. Text in fertigen Overlay-Videos wird nicht umplatziert.
- Plattform-Presets sind **editierbare Richtwerte**, keine garantierten TikTok-/Meta-/YouTube-Spezifikationen. Sie müssen zum konkreten Placement passen.
- Ein Hauptvideo, eine Musikspur und ein Outro-Sound pro Projekt. Maximal fünf Minuten Ausgabe. Keine Mehrclip-Quellbibliothek, keine freie Timeline, keine Übergänge oder Geschwindigkeitseffekte in 0.1.
- Eigene Bild-/Ton-/Schriftdateien importieren; es werden keine fremden Musik- oder Fontpakete mitgeliefert.

## Quellcode und Build

`ui/` ist exakt die Oberfläche der EXE. `native/` enthält Fenster, lokale Medienbereitstellung mit Range-Support und FFmpeg-Renderpfad. JANO App-Kit **0.2.0** ist fest kopiert; keine automatische Aktualisierung anderer Apps.

Windows-Build mit vorhandenem .NET-Framework-C#-Compiler:

```powershell
& .\build.ps1
node .\tests\core.test.cjs
& .\tests\engine-smoke.ps1
& .\tests\start-qa.ps1
```

WebView2 SDK 1.0.2903.40 liegt in der lokalen Lieferkopie unter `.deps`. Für einen frischen Quellcode-Checkout: das gleichnamige Microsoft NuGet-Paket herunterladen und net462-Core/WinForms sowie win-x64 WebView2Loader.dll und LICENSE.txt nach `.deps` übernehmen. Lokale SDK-Herkunft: `jano-app-kit/.deps/webview2-1.0.2903.40`.

Siehe [VERIFICATION.md](VERIFICATION.md), [Markenprüfung](docs/BRAND_CHECK.md), [Schutzbereiche](docs/SAFE_ZONES.md) und [Fremdkomponenten](THIRD_PARTY.md).

## Entwickler-Einstieg · 21.09.2026

[DEVELOPMENT.md](DEVELOPMENT.md) beschreibt Voraussetzungen, konkrete Build-/Testbefehle,
Datenablage, Modulgrenzen und offene Punkte. Vor Weiterarbeit zuerst dort lesen;
vorhandene Produktregeln und fachliche Nachweise bleiben massgeblich.


## GitHub-Ablage

VEYLCUT – Lokaler Videoeditor mit Schnittvorlagen, Overlays und FFmpeg-Export.

Repository: `jano3dstudio/veylcut` (privat). Quellen, Build-Anleitung und Projektregeln werden versioniert. Persönliche Laufzeitdaten, Zugangsdaten und lokale Sicherungen gehören nicht in Git. Bestehende lokale Start- und Quellpfade bleiben erhalten. Der Upload ist eine Quellcodesicherung; technische Prüfstände und persönliche Freigabe stehen separat in der Projektdokumentation.
