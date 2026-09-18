param([string]$ExePath)
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
if(-not $ExePath){$ExePath=Join-Path $project 'dist\VEYLCUT.exe'}
$qaRoot=Join-Path $PSScriptRoot ('output\desktop-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Force -Path $qaRoot | Out-Null
$qaProcess=Start-Process -FilePath $ExePath -ArgumentList @('--self-test',('"'+$qaRoot+'"')) -WindowStyle Hidden -PassThru
@{path=$qaRoot;pid=$qaProcess.Id;exe=$ExePath} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'output\latest.json')
Get-Content -LiteralPath (Join-Path $PSScriptRoot 'output\latest.json')
