param([string]$OutputPath)
$ErrorActionPreference='Stop'
$appRoot=$PSScriptRoot
if (-not $OutputPath) { $OutputPath=Join-Path $appRoot 'dist\VEYLCUT.exe' }
$OutputPath=[IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Force -Path (Split-Path $OutputPath) | Out-Null
$sdk=Join-Path $appRoot '.deps'
if (-not (Test-Path (Join-Path $sdk 'Microsoft.Web.WebView2.Core.dll'))) { throw 'WebView2 SDK dependency missing; see README.md.' }
Add-Type -AssemblyName System.Drawing
$bitmap=New-Object Drawing.Bitmap 256,256
$g=[Drawing.Graphics]::FromImage($bitmap)
$g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([Drawing.Color]::Transparent)
$bg=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#101714'))
$shape=[Drawing.Drawing2D.GraphicsPath]::new()
$shape.AddArc(0,0,112,112,180,90); $shape.AddArc(144,0,112,112,270,90); $shape.AddArc(144,144,112,112,0,90); $shape.AddArc(0,144,112,112,90,90); $shape.CloseFigure()
$g.FillPath($bg,$shape)
$p1=[Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#345c48'),14)
$p2=[Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#7aaf92'),14)
$g.DrawRectangle($p1,50,58,122,138); $g.DrawRectangle($p2,80,42,126,136)
$green=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#3cff91'))
$points=[Drawing.PointF[]]@([Drawing.PointF]::new(50,116),[Drawing.PointF]::new(100,184),[Drawing.PointF]::new(210,74),[Drawing.PointF]::new(176,68),[Drawing.PointF]::new(104,140),[Drawing.PointF]::new(80,104))
$g.FillPolygon($green,$points)
$g.FillPolygon($green,[Drawing.PointF[]]@([Drawing.PointF]::new(162,200),[Drawing.PointF]::new(202,160),[Drawing.PointF]::new(202,200)))
$g.Dispose(); $bg.Dispose(); $green.Dispose(); $p1.Dispose(); $p2.Dispose(); $shape.Dispose()
$bitmap.Save((Join-Path $appRoot 'assets\icon.png'),[Drawing.Imaging.ImageFormat]::Png)
# ICO directory with genuine PNG-encoded 16/24/32/48/64/128/256 pixel entries.
$sizes=@(16,24,32,48,64,128,256); $images=@()
foreach($size in $sizes){$small=[Drawing.Bitmap]::new($bitmap,$size,$size);$ms=[IO.MemoryStream]::new();$small.Save($ms,[Drawing.Imaging.ImageFormat]::Png);$images+=,$ms.ToArray();$ms.Dispose();$small.Dispose()}
$icoPath=Join-Path $appRoot 'assets\icon.ico';$fs=[IO.File]::Create($icoPath);$writer=[IO.BinaryWriter]::new($fs)
$writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$sizes.Count);$offset=6+16*$sizes.Count
for($i=0;$i -lt $sizes.Count;$i++){$byteSize=if($sizes[$i] -eq 256){0}else{$sizes[$i]};$writer.Write([byte]$byteSize);$writer.Write([byte]$byteSize);$writer.Write([byte]0);$writer.Write([byte]0);$writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$images[$i].Length);$writer.Write([uint32]$offset);$offset+=$images[$i].Length}
foreach($bytes in $images){$writer.Write([byte[]]$bytes)};$writer.Dispose();$bitmap.Dispose()
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$buildArgs=@('/nologo','/target:winexe','/platform:x64','/optimize+',"/out:$OutputPath",'/main:Jano.AppKit.DesktopStart',"/win32icon:$icoPath",'/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll','/reference:System.Core.dll','/reference:System.Web.Extensions.dll',('/reference:'+(Join-Path $sdk 'Microsoft.Web.WebView2.Core.dll')),('/reference:'+(Join-Path $sdk 'Microsoft.Web.WebView2.WinForms.dll')))
foreach($name in @('Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll','WebView2Loader.dll','LICENSE.txt')){$buildArgs+=('/resource:'+(Join-Path $sdk $name)+',payload/'+$name)}
Get-ChildItem -LiteralPath (Join-Path $appRoot 'ui') -File -Recurse | ForEach-Object {$relative=$_.FullName.Substring($appRoot.Length+1).Replace('\','/');$buildArgs+=('/resource:'+$_.FullName+',payload/'+$relative)}
Get-ChildItem -LiteralPath (Join-Path $appRoot 'native') -Filter '*.cs' | ForEach-Object {$buildArgs+=$_.FullName}
& $compiler @buildArgs
if ($LASTEXITCODE -ne 0) { throw 'VEYLCUT build failed.' }
Get-Item -LiteralPath $OutputPath | Select-Object FullName,Length,LastWriteTime
