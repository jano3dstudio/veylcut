# VEYLCUT

Local Windows video editor, working name, version 0.1.0. No remote publication authorized.
Pinned JANO App-Kit 0.2.0: ui/kit and native window sources copied from ../jano-app-kit; read its DESIGN_SYSTEM.md before UI edits.
The WebView2 UI is the shipped UI. FFmpeg performs all final renders locally. Preserve input files; exports use a temporary file and a user-selected destination. Never claim the rule-based prompt parser understands footage semantically.
Keep a real render test (duration, dimensions, audio, overlays, Unicode filenames), and capture the actual EXE UI before delivery. Safe-zone presets are adjustable editorial guides, not platform compliance guarantees.

## Gemeinsame Modulbauweise vorbereiten · 21.09.2026

Vor Arbeiten an gemeinsamen UI-Bausteinen `app-kit.plan.json` lesen und
`./Check-AppKit.ps1` ausfuehren. Der Plan verweist auf die zentrale Kit-Quelle;
Ablauf dort in `ADOPTION.md`. Second Brain bleibt die Referenz zur Abstimmung.
Diese Vorbereitung aktiviert keine neuen Module. Bestehende aktive `kit.ref.json`
und ansonsten bisherige Pins gelten bis zur gezielten Migration weiter.
Fenster, Buttons, Schriftgroessen, Rundungen, Looks, Dialoggriffe und Arbeitsanzeige
zentral weiterentwickeln; Produktlogik, Nutzerdaten und Host-Regeln erhalten.
Ein erfolgreicher Vorbereitungscheck ist keine Build-, UI- oder Designfreigabe.

## Ablage-Wegweiser · 21.09.2026

Siehe [PROJECT_MAP.json](PROJECT_MAP.json). Quell-, Build-, Start- und Datenpfade sind erhalten.
Der README-Einstieg im App-Hauptordner fuehrt zu allen aktuellen Arbeitsstellen.

## Entwickler-Einstieg · 21.09.2026

[DEVELOPMENT.md](DEVELOPMENT.md) beschreibt Voraussetzungen, konkrete Build-/Testbefehle,
Datenablage, Modulgrenzen und offene Punkte. Vor Weiterarbeit zuerst dort lesen;
vorhandene Produktregeln und fachliche Nachweise bleiben massgeblich.
