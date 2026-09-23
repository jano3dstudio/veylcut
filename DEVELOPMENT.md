# VEYLCUT · Weiterentwicklung

Stand: 21.09.2026. Diese Anleitung wurde gegen lokale Quellen und Build-Scripts
abgeglichen. Sie ist kein neuer Nachweis einer Installation auf einem fremden PC.
Produktpruefungen und visuelle Abnahme stehen in den unten verlinkten Belegen.

## 1. Einstieg

App-Hauptordner relativ zu dieser Datei: `..`. Git-Repository: dieser
Ordner. Quellen relativ zu dieser Datei: `.`. Zuerst
[_Projekt/AGENTS.md](<AGENTS.md>) und [PROJECT_MAP.json](PROJECT_MAP.json) lesen. Bestehende
Pfade, Daten und uncommitted Aenderungen erhalten. **Alle folgenden Befehle
aus dem Quellordner `.` ausfuehren**, nicht aus einem beliebigen cwd.

## 2. Voraussetzungen

Windows x64, .NET Framework und WebView2 Runtime. SDK-Dateien unter .deps. FFmpeg und FFprobe fuer reale Medienverarbeitung; bisher lokal FFmpeg 8.1.1 verwendet. Suche erfolgt ueber engine neben der EXE, PATH oder lokale WinGet-Installation. Node fuer Core-Tests. Der Build regeneriert assets/icon.png und assets/icon.ico: fuer Experimente eine isolierte Arbeitskopie verwenden.

SDK-Ziel relativ zum App-Hauptordner: `_Projekt/.deps`. Benoetigt werden
Core.dll, WinForms.dll (jeweils mit Praefix Microsoft.Web.WebView2),
WebView2Loader.dll und LICENSE.txt. Die genaue gemeinsame Wiederherstellung
steht in [DEPENDENCIES.md](<../../jano-app-kit/DEPENDENCIES.md>).
SDK zum Kompilieren und installierte WebView2 Runtime zum Starten sind getrennt.

## 3. Bauen und starten

Fuer Kandidaten einen neuen Ausgabeordner verwenden; nicht ueber die laufende
oder ausgelieferte EXE bauen. Kein Build-Befehl hier startet einen Upload.

```powershell
$devOut = Join-Path ([IO.Path]::GetTempPath()) ("jano-veylcut-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $devOut | Out-Null
$candidate = Join-Path $devOut 'candidate.exe'
& .\build.ps1 -OutputPath $candidate
```

Startweg des bestehenden Lieferstands: [VEYLCUT.exe](<../VEYLCUT.exe>).
Den Kandidaten erst nach den passenden Tests uebernehmen. Paketorte und
Liefer-Scripts sind in PROJECT_MAP.json und der bisherigen README benannt.

## 4. Aufbau und gemeinsame Module

ui/: Schnittmodell und Bedienung; native/Engine.cs: FFmpeg/FFprobe-Verarbeitung; Desktop.cs: Host/Bridge. Gemeinsame Kit-Werte bleiben aktuell gepinnte Kopien.

Modulstand und Migrationsgrenzen: [_Projekt/app-kit.plan.json](<app-kit.plan.json>).
Second Brain bleibt die Referenz fuer die Abstimmung gemeinsamer Bausteine.
Eine Modulvorbereitung bedeutet keine aktive Uebernahme oder Designfreigabe.

## 5. Pruefen

```powershell
node tests/core.test.cjs
# Separat mit FFmpeg/FFprobe, erzeugt synthetische Testmedien:
& .\tests\engine-smoke.ps1
```

tests/start-qa.ps1 prueft die native App; tests/engine-smoke.ps1 reale synthetische Renderausgaben. Originalmedien niemals als Testmaterial verwenden.

Erwartung: Die genannten Tests laufen ohne Fehler/mit Exitcode 0 durch.
Fehlende Laufzeiten, Browser oder Fixtures als fehlende Voraussetzung melden,
nicht als bestandenen Test. GUI-/Host-/Netztests bleiben gesonderte Schritte.
Testausgaben duerfen nur synthetische Inhalte enthalten. Nach einer UI-Aenderung
die tatsaechliche Kandidaten-App inklusive Fensterbedienung pruefen.

## 6. Daten und Konfiguration

%LOCALAPPDATA%/JANO/Veylcut/library enthaelt importierte Assets, renders die Zwischendaten. .vey-Projekte speichern Medienverweise und Einstellungen, nicht automatisch die Originalvideos. Fuer Rechnerwechsel referenzierte Medien separat mitnehmen; keine Quelldateien ueberschreiben.

Echte Zugangsdaten nicht in .env-Beispiele, Logs, Screenshots oder Git aufnehmen.
Es gibt durch diese Dokumentationspflege keine neue globale .env-Konfiguration.
Bestehende Profile vor einer beauftragten Migration sichern; keine Migration
allein zum Einrichten des Entwicklungsplatzes ausfuehren.

## 7. Stand, offene Punkte und Zusammenarbeit

Lokaler Schnittprototyp. Prompt-Auswertung ist regelbasiert; kein semantisches Verstehen des Filminhalts. FFmpeg-Ausgaben muessen real auf Dauer, Bildgroesse, Audio und Unicode-Pfade geprueft werden; kein Platzhalter-Rendernachweis.

Massgebliche bestehende Quellen (keine zweite Statuschronik):

- [_Projekt/README.md](<README.md>)
- [_Projekt/VERIFICATION.md](<VERIFICATION.md>)
- [_Projekt/THIRD_PARTY.md](<THIRD_PARTY.md>)

Keine Projektlizenz am Repository-Einstieg gefunden. Diese Anleitung vergibt keine Nutzungsrechte; Lizenzentscheidung vor externer Weitergabe mit Jona klaeren. Bestehende Drittanbieterhinweise gelten weiterhin.

Arbeitsablauf fuer kleine Aenderungen, Nachweise und Uebergaben:
[CONTRIBUTING_APPS.md](<../../jano-app-kit/CONTRIBUTING_APPS.md>).
Fuer neue Entwickler zuerst die risikoarmen Checks ausfuehren, dann eine kleine
Aenderung im eigenen Arbeitsstand. Abschluss mit geaenderten Dateien,
ausgefuehrten Tests, offen gebliebenen Pruefungen und genauem Kandidatenpfad.
