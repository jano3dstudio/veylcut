# VEYLCUT · Build und Lieferung

Stand: 24.09.2026. [Website](https://tools.jano3dstudio.de/veylcut/) · [GitHub-Releases](https://github.com/jano3dstudio/veylcut/releases).

## Vorbereiteter Build

`VEYLCUT-Windows-20260924.zip` (303,074 Bytes), SHA-256 `01938f939827f5d4bff06b52a15eb5c367f1f07ef70a9cf7f41703bacb0a3be1`.

Quelle im lokalen App-Ordner: `VEYLCUT.exe`.
Startweg des bestehenden Lieferstands: `VEYLCUT.exe`.
Der Build ist ein vorhandener Lieferstand, kein frisch kompilierter oder erneut funktional abgenommener Build.
Paketintegritaet und Pruefsumme wurden geprueft. Die Release-Ablage wird separat bestaetigt; diese Datei behauptet keinen bereits erfolgten Upload.

## Selbst bauen

[Entwicklungsanleitung](DEVELOPMENT.md) · [Build-Einstieg](<build.ps1>).
Voraussetzungen, gepinnte SDKs und produktspezifische Tests stehen in der Entwicklungsanleitung.
Fuer gemeinsame Module benoetigt man gegebenenfalls das [MODULO-Repository](https://github.com/jano3dstudio/jano-app-kit); benachbarte Checkout-Ordner muessen den dort dokumentierten Namen behalten.
Der vorhandene lokale Build beweist keinen erfolgreichen Build aus einem frischen Checkout.

## Ordner und Daten

Quell-, Start- und Profilpfade bleiben stabil. Laufzeitdaten, Passwoerter, Testprofile und Sicherungen gehoeren nicht in Release-Pakete.
Alte Buildstaende bleiben lokal; fuer diese Lieferung wurde nur der oben genannte Kandidat ausgewaehlt.
EXE/ZIP-Dateien werden nicht in die Quellcode-Historie gezwungen. Getrennte Release-Assets enthalten SHA256SUMS.txt.

Vor oeffentlicher Weitergabe [PUBLICATION_REVIEW.md](PUBLICATION_REVIEW.md) beachten.

Startdatei im ZIP: `VEYLCUT.exe`. ZIP zuerst vollständig entpacken.
