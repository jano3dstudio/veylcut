$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder=Join-Path $PSScriptRoot ('output\engine-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Force -Path $folder | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$exe=Join-Path $folder 'EngineSmoke.exe'
& $compiler /nologo /target:exe /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Web.Extensions.dll "/out:$exe" (Join-Path $root 'native\Engine.cs') (Join-Path $PSScriptRoot 'EngineSmoke.cs')
if($LASTEXITCODE -ne 0){throw 'Engine test build failed'}
& $exe $folder
if($LASTEXITCODE -ne 0){throw 'Engine smoke test failed'}
