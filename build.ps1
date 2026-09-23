param([string]$OutputPath)
$ErrorActionPreference='Stop'
$appRoot=$PSScriptRoot
if (-not $OutputPath) { $OutputPath=Join-Path $appRoot 'dist\VEYLCUT.exe' }
$OutputPath=[IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Force -Path (Split-Path $OutputPath) | Out-Null
$sdk=Join-Path $appRoot '.deps'
if (-not (Test-Path (Join-Path $sdk 'Microsoft.Web.WebView2.Core.dll'))) { throw 'WebView2 SDK dependency missing; see README.md.' }
$icoPath=Join-Path $appRoot 'assets\icon.ico'
if(-not (Test-Path -LiteralPath $icoPath)){throw 'App icon missing. See ../jano-app-kit/APP_ICONS.md.'}
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$buildArgs=@('/nologo','/target:winexe','/platform:x64','/optimize+',"/out:$OutputPath",'/main:Jano.AppKit.DesktopStart',"/win32icon:$icoPath",'/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll','/reference:System.Core.dll','/reference:System.Web.Extensions.dll',('/reference:'+(Join-Path $sdk 'Microsoft.Web.WebView2.Core.dll')),('/reference:'+(Join-Path $sdk 'Microsoft.Web.WebView2.WinForms.dll')))
foreach($name in @('Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll','WebView2Loader.dll','LICENSE.txt')){$buildArgs+=('/resource:'+(Join-Path $sdk $name)+',payload/'+$name)}
Get-ChildItem -LiteralPath (Join-Path $appRoot 'ui') -File -Recurse | ForEach-Object {$relative=$_.FullName.Substring($appRoot.Length+1).Replace('\','/');$buildArgs+=('/resource:'+$_.FullName+',payload/'+$relative)}
Get-ChildItem -LiteralPath (Join-Path $appRoot 'native') -Filter '*.cs' | ForEach-Object {$buildArgs+=$_.FullName}
& $compiler @buildArgs
if ($LASTEXITCODE -ne 0) { throw 'VEYLCUT build failed.' }
Get-Item -LiteralPath $OutputPath | Select-Object FullName,Length,LastWriteTime
