$ErrorActionPreference='Stop'
$sourceRoot=[IO.Path]::GetFullPath($PSScriptRoot)
$parent=Split-Path (Split-Path $sourceRoot -Parent) -Parent
$destination=[IO.Path]::GetFullPath((Join-Path $parent 'JS_GitHub\veylcut'))
$expectedParent=[IO.Path]::GetFullPath((Join-Path $parent 'JS_GitHub'))+[IO.Path]::DirectorySeparatorChar
if(-not $destination.StartsWith($expectedParent,[StringComparison]::OrdinalIgnoreCase)){throw 'Unexpected delivery destination'}
if(Test-Path -LiteralPath $destination){throw 'Destination already exists. Inspect before updating; no automatic overwrite.'}
$candidate=Join-Path $sourceRoot 'dist\VEYLCUT.exe'
$expected='858799C5D9C148FB640D4B24A96B5A7EAFBB15982B5BD02CBCC4342390EAD6D7'
if((Get-FileHash -LiteralPath $candidate -Algorithm SHA256).Hash -ne $expected){throw 'Candidate differs from verified EXE'}
$qa=Get-Content -LiteralPath (Join-Path $sourceRoot 'tests\output\latest.json') | ConvertFrom-Json
if(-not (Test-Path -LiteralPath (Join-Path $qa.path 'result.json'))){throw 'EXE test result missing'}
if(Test-Path -LiteralPath (Join-Path $qa.path 'error.txt')){throw 'EXE test failed'}
New-Item -ItemType Directory -Force -Path (Join-Path $destination '_Projekt'),(Join-Path $destination '_Nachweise') | Out-Null
$manifest=[Collections.Generic.List[object]]::new()
foreach($file in Get-ChildItem -LiteralPath $sourceRoot -File -Force -Recurse){
 $relative=$file.FullName.Substring($sourceRoot.Length+1)
 if($relative.StartsWith('dist\') -or $relative.StartsWith('tests\output\')){continue}
 $target=Join-Path (Join-Path $destination '_Projekt') $relative
 New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
 Copy-Item -LiteralPath $file.FullName -Destination $target
 $before=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
 $after=(Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
 if($before -ne $after){throw "Checksum mismatch: $relative"}
 $manifest.Add(@{file=('_Projekt/'+$relative.Replace('\','/'));sha256=$after})
}
Copy-Item -LiteralPath $candidate -Destination (Join-Path $destination 'VEYLCUT.exe')
if((Get-FileHash -LiteralPath (Join-Path $destination 'VEYLCUT.exe') -Algorithm SHA256).Hash -ne $expected){throw 'Delivered EXE checksum mismatch'}
foreach($name in @('editor.png','empty.png','look.png','compact.png','output-settings.png','result.json','verified-export.mp4')){
 $source=Join-Path $qa.path $name;$target=Join-Path (Join-Path $destination '_Nachweise') $name
 Copy-Item -LiteralPath $source -Destination $target
 if((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $target).Hash){throw 'Evidence checksum mismatch'}
}
$intro=@'
# VEYLCUT 0.1.0

**Start: VEYLCUT.exe doppelklicken.**

Video laden → Intro und Outro wählen → Text und Schnitt-Prompt eingeben → Format/Schutzbereiche einstellen → Vorschau rendern → MP4 exportieren.

„Mit Testvideo ausprobieren“ funktioniert ohne eigene Aufnahme. Das Testmaterial ist synthetisch.

- Quellcode, Build und ausführliche Anleitung: [_Projekt/README.md](_Projekt/README.md)
- Prüfbericht: [_Projekt/VERIFICATION.md](_Projekt/VERIFICATION.md)
- Screenshots und tatsächlich exportiertes Testvideo: `_Nachweise`

Lokaler Prototyp; benötigt die hier bereits vorhandenen WebView2- und FFmpeg-Installationen. Kein Upload, kein Abo. Schnitt-Prompt derzeit regelbasiert; VEYLCUT ist ein noch nicht abschließend markengeprüfter Arbeitstitel.
'@
[IO.File]::WriteAllText((Join-Path $destination 'README.md'),$intro,[Text.UTF8Encoding]::new($false))
$manifest.Add(@{file='VEYLCUT.exe';sha256=$expected})
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $destination '_Nachweise\delivery-sha256.json')
@{destination=$destination;exeHash=$expected;verifiedFiles=$manifest.Count} | ConvertTo-Json
