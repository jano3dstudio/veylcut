# Prüfung · VEYLCUT 0.1.0

2026-09-18, lokal auf Jonas Windows-Rechner. Visuelle Abnahme durch Jona noch offen.

## Geprüfter Build

`dist/VEYLCUT.exe`, SHA-256 `858799C5D9C148FB640D4B24A96B5A7EAFBB15982B5BD02CBCC4342390EAD6D7`.

Native EXE gebaut ohne Compilerfehler/-warnungen. WebView2 UI aus exakt den eingebetteten Dateien gestartet, isoliertes Testprofil. Screenshots der realen EXE-Oberfläche geprüft: leerer Einstieg, Editor mit dekodiertem Video/Text, Look-Dialog, kompakte Ansicht und ausgeklappte Ausgabe-/Schutzbereichseinstellungen. Keine horizontale Überbreite; drei Maximieren/Wiederherstellen-Zyklen ohne Größenänderung. Kein separater Windows-DPI-Test bei 150 % und keine vollständige Desktop-Compositor-Aufnahme durchgeführt.

## Funktionstest

Nachweisordner im Entwicklungsprojekt: `tests/output/desktop-20260918-105409`.

- FFmpeg/FFprobe erkannt; synthetisches Video geladen und im WebView2 dekodiert.
- Mehrere regelbasierte Ausschnitte, Gesamtlänge sechs Sekunden, Quellgrenzen eingehalten.
- Schnittposition angesprungen und tatsächliches Videobild nach dem Seek geprüft.
- Byte-Range-Request mit 16 Bytes liefert 206 und exakt 16 Bytes.
- Text-Schutzbereich editierbar; Sprachwechsel erhält Texte; Look-Abbrechen stellt Akzent wieder her.
- Projekt geschrieben/zurückgelesen; nur verwendete Medienreferenzen gespeichert.
- Gerenderte Vorschau 360×640, circa sechs Sekunden, mit eingebrannten Ebenen und Audio; im App-Player dekodiert.
- Echter Export 1920×1080, H.264, AAC 48 kHz, exakt sechs Sekunden laut FFprobe.

## Medien- und Abbruchtest

`tests/output/engine-20260918-105023/PASS.txt`:

- Unicode-/Leerzeichen-/&-Dateipfade.
- Quellvideo ohne Ton, Musik und Outro-Sound gleichzeitig.
- Tatsächlicher MOV-Alpha-Clip am Anfang, PNG-Overlay am Ende.
- Pixelvergleich: Overlay an den richtigen Zeiten sichtbar; transparente Bereiche erhalten das Quellbild; Hauptteil frei von Intro-/Outro-Grafik.
- 1:1-Ausgabe mit Einpassen; Vorschauauflösung und Laufzeit.
- Render-Abbruch lässt keinen halbfertigen finalen Export zurück.

`node tests/core.test.cjs`: kurze/lange Quellen, vier Tempostile, Dauererhalt und Clipgrenzen; alle bestanden.

## Korrigierte Befunde

Die erste virtuelle Medienzuordnung lieferte keine abspielbare Quelle. Die App verwendet jetzt eine explizite same-origin Ressourcenroute mit MIME-Typen und begrenzten Dateistreams für Range-Requests. Ein erfolgreicher MP4-Export allein hatte diesen UI-Fehler nicht erkannt; Video-Decoding, gerenderter Player und Seek sind deshalb Teil der EXE-Prüfung.

Header-Zeilenumbrüche korrigiert; Exportleiste bleibt beim Scrollen sichtbar. Dynamische Video-/Schnittbeschriftungen bleiben beim Sprachwechsel erhalten.

## Grenzen

Noch keine Nutzer-Gameplay-Aufnahme und keine großen realen Produktionsdateien getestet. Datei-Auswahldialoge, sämtliche möglichen Quellcodecs und importierte Schriftdateien wurden nicht umfassend durchgetestet. Der Prompt ist regelbasiert. Safe-Zone-Werte sind Richtwerte. Namen/Marke nicht abschließend geklärt. Details in README und docs/BRAND_CHECK.md.
