using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Jano.AppKit {
static class DesktopStart {
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern bool SetDllDirectory(string path);
 [DllImport("user32.dll")] static extern bool SetProcessDpiAwarenessContext(IntPtr value);
 public static string AssetRoot,DataRoot,CheckRoot;
 [STAThread] static int Main(string[] args){try{
  CheckRoot=args.Length==2&&args[0]=="--self-test"?Path.GetFullPath(args[1]):null;
  string hash;using(var sha=SHA256.Create())using(var f=File.OpenRead(Assembly.GetExecutingAssembly().Location))hash=BitConverter.ToString(sha.ComputeHash(f)).Replace("-","").Substring(0,16);
  DataRoot=CheckRoot??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"JANO","Veylcut");
  AssetRoot=Path.Combine(DataRoot,"runtime",hash);Directory.CreateDirectory(AssetRoot);
  foreach(string name in Assembly.GetExecutingAssembly().GetManifestResourceNames())if(name.StartsWith("payload/")){
   string dest=Path.Combine(AssetRoot,name.Substring(8).Replace('/',Path.DirectorySeparatorChar));Directory.CreateDirectory(Path.GetDirectoryName(dest));
   using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream(name))using(var buffer=new MemoryStream()){stream.CopyTo(buffer);var bytes=buffer.ToArray();if(!File.Exists(dest)||!bytes.SequenceEqual(File.ReadAllBytes(dest)))File.WriteAllBytes(dest,bytes);}
  }
  SetDllDirectory(AssetRoot);AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string n=new AssemblyName(e.Name).Name;return n.StartsWith("Microsoft.Web.WebView2.")?Assembly.LoadFrom(Path.Combine(AssetRoot,n+".dll")):null;};
  try{SetProcessDpiAwarenessContext(new IntPtr(-4));}catch{}
  return Run();
 }catch(Exception e){if(CheckRoot!=null)File.WriteAllText(Path.Combine(CheckRoot,"error.txt"),e.ToString());else MessageBox.Show(e.Message,"VEYLCUT");return 1;}}
 [MethodImpl(MethodImplOptions.NoInlining)] static int Run(){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);using(var app=new EditorWindow()){Application.Run(app);return app.ExitCode;}}
}
public class EditorWindow:JanoWindow {
 readonly WebView2 web=new WebView2();readonly EditorEngine engine;readonly Label loading;

 bool busy; public int ExitCode; string lastOutput;
 const string Origin="https://veylcut.local/";
 public EditorWindow(){
  Text="VEYLCUT · Video Studio";BackColor=Tokens.Background;AutoScaleMode=AutoScaleMode.None;
  var area=Screen.PrimaryScreen.WorkingArea;ClientSize=new Size(Math.Min(1480,area.Width-60),Math.Min(940,area.Height-80));MinimumSize=new Size(900,620);StartPosition=FormStartPosition.CenterScreen;
  engine=new EditorEngine(DesktopStart.DataRoot);web.Dock=DockStyle.Fill;web.DefaultBackgroundColor=Tokens.Background;Controls.Add(web);
  loading=new Label{Text="VEYLCUT wird geöffnet …",Dock=DockStyle.Fill,ForeColor=Tokens.Text,TextAlign=ContentAlignment.MiddleCenter};Controls.Add(loading);loading.BringToFront();
  engine.Progress=p=>{if(!IsDisposed)BeginInvoke((Action)(()=>Send(new{eventName="progress",value=p})));};
  Shown+=async(s,e)=>await Initialize();
  FormClosing+=(s,e)=>{if(busy){e.Cancel=true;MessageBox.Show(this,"Bitte zuerst den laufenden Vorgang abbrechen.","VEYLCUT");}};
 }
 async Task Initialize(){try{
  var env=await CoreWebView2Environment.CreateAsync(null,Path.Combine(DesktopStart.DataRoot,"profile"));await web.EnsureCoreWebView2Async(env);var core=web.CoreWebView2;
  core.Settings.AreDevToolsEnabled=false;core.Settings.AreDefaultContextMenusEnabled=false;core.Settings.AreHostObjectsAllowed=false;core.Settings.IsStatusBarEnabled=false;
  core.NavigationStarting+=(s,e)=>{if(!e.Uri.StartsWith(Origin))e.Cancel=true;};
  core.PermissionRequested+=(s,e)=>e.State=CoreWebView2PermissionState.Deny;core.DownloadStarting+=(s,e)=>e.Cancel=true;
  core.NewWindowRequested+=(s,e)=>{e.Handled=true;if(e.IsUserInitiated&&e.Uri=="https://www.linkedin.com/in/jonaschlegelmilch/")Process.Start(new ProcessStartInfo(e.Uri){UseShellExecute=true});};
  core.AddWebResourceRequestedFilter("*",CoreWebView2WebResourceContext.All);core.WebResourceRequested+=(s,e)=>{
   if(DesktopStart.CheckRoot!=null)File.AppendAllText(Path.Combine(DesktopStart.CheckRoot,"requests.txt"),e.Request.Uri+"\n");
   Uri u;if(!Uri.TryCreate(e.Request.Uri,UriKind.Absolute,out u))return;
   if(u.Host=="veylcut.local"&&u.AbsolutePath.StartsWith("/media/")){ServeMedia(env,e,u);return;}
   if(u.Scheme=="https"&&u.Host=="veylcut.local"){
    try{string root=Path.Combine(DesktopStart.AssetRoot,"ui")+Path.DirectorySeparatorChar;string path=Path.GetFullPath(Path.Combine(root,Uri.UnescapeDataString(u.AbsolutePath.TrimStart('/')).Replace('/',Path.DirectorySeparatorChar)));
     if(!path.StartsWith(root,StringComparison.OrdinalIgnoreCase)||!File.Exists(path))throw new FileNotFoundException();
     string ext=Path.GetExtension(path),type=ext==".html"?"text/html; charset=utf-8":ext==".css"?"text/css; charset=utf-8":ext==".js"?"text/javascript; charset=utf-8":ext==".svg"?"image/svg+xml":"application/octet-stream";
     e.Response=env.CreateWebResourceResponse(File.OpenRead(path),200,"OK","Content-Type: "+type);return;
    }catch{e.Response=env.CreateWebResourceResponse(new MemoryStream(),404,"Not Found","");return;}
   }
   bool permitted=u.Scheme=="data"||u.Scheme=="blob"||u.Scheme=="https"&&u.Host=="veylcut.local";

   if(!permitted)e.Response=env.CreateWebResourceResponse(new MemoryStream(),403,"Blocked","");
  };
  core.WebMessageReceived+=async(s,e)=>{if(!e.Source.StartsWith(Origin))return;Dictionary<string,object> d=null;try{d=EditorEngine.Json.Deserialize<Dictionary<string,object>>(e.WebMessageAsJson);await HandleMessage(d);}catch(Exception error){if(d!=null)Send(new{id=d["id"],ok=false,error=error.Message});}};
  core.NavigationCompleted+=async(s,e)=>{if(e.IsSuccess){loading.Visible=false;web.BringToFront();RefreshChrome();if(DesktopStart.CheckRoot!=null){try{await SelfTest();}catch(Exception error){ExitCode=1;File.WriteAllText(Path.Combine(DesktopStart.CheckRoot,"error.txt"),error.ToString());}busy=false;Close();}}else loading.Text="Die Oberfläche konnte nicht geöffnet werden: "+e.WebErrorStatus;};
  core.Navigate(Origin+"index.html");
 }catch(Exception e){ExitCode=1;loading.Text=e.Message;if(DesktopStart.CheckRoot!=null){File.WriteAllText(Path.Combine(DesktopStart.CheckRoot,"error.txt"),e.ToString());Close();}}}
 void Send(object obj){if(web.CoreWebView2!=null)web.CoreWebView2.PostWebMessageAsJson(EditorEngine.Json.Serialize(obj));}
 MediaAsset Map(MediaAsset a){a.url=Origin+"media/"+a.id+"/"+Uri.EscapeDataString(Path.GetFileName(a.path));return a;}
 void ServeMedia(CoreWebView2Environment env,CoreWebView2WebResourceRequestedEventArgs e,Uri u){
  try{string[] parts=u.AbsolutePath.Split('/');MediaAsset a;if(parts.Length!=4||!engine.Assets.TryGetValue(parts[2],out a)||Uri.UnescapeDataString(parts[3])!=Path.GetFileName(a.path)||!File.Exists(a.path))throw new FileNotFoundException();
   long length=new FileInfo(a.path).Length,start=0,end=length-1;bool partial=false;
   if(e.Request.Headers.Contains("Range")){var match=System.Text.RegularExpressions.Regex.Match(e.Request.Headers.GetHeader("Range"),@"^bytes=(\d*)-(\d*)$");if(match.Success){partial=true;if(match.Groups[1].Value==""){long suffix=long.Parse(match.Groups[2].Value);start=Math.Max(0,length-suffix);}else{start=long.Parse(match.Groups[1].Value);if(match.Groups[2].Value!="")end=Math.Min(end,long.Parse(match.Groups[2].Value));}}}
   if(start>end||start>=length){e.Response=env.CreateWebResourceResponse(new MemoryStream(),416,"Range Not Satisfiable","Content-Range: bytes */"+length);return;}
   string ext=Path.GetExtension(a.path).ToLowerInvariant(),type="application/octet-stream";
   switch(ext){case ".mp4":case ".m4v":type="video/mp4";break;case ".webm":type="video/webm";break;case ".mov":type="video/quicktime";break;case ".png":type="image/png";break;case ".jpg":case ".jpeg":type="image/jpeg";break;case ".mp3":type="audio/mpeg";break;case ".wav":type="audio/wav";break;case ".m4a":type="audio/mp4";break;case ".ttf":type="font/ttf";break;case ".otf":type="font/otf";break;case ".woff":type="font/woff";break;case ".woff2":type="font/woff2";break;}
   string headers="Content-Type: "+type+"\r\nAccept-Ranges: bytes\r\nContent-Length: "+(end-start+1)+"\r\nCache-Control: no-cache";
   if(partial)headers+="\r\nContent-Range: bytes "+start+"-"+end+"/"+length;
   e.Response=env.CreateWebResourceResponse(new FileSlice(a.path,start,end-start+1),partial?206:200,partial?"Partial Content":"OK",headers);
  }catch(Exception error){if(DesktopStart.CheckRoot!=null)File.AppendAllText(Path.Combine(DesktopStart.CheckRoot,"media-errors.txt"),error.ToString()+"\n");e.Response=env.CreateWebResourceResponse(new MemoryStream(),404,"Not Found","");}
 }
 static string Str(Dictionary<string,object> d,string key){return d.ContainsKey(key)?Convert.ToString(d[key]):null;}
 async Task<List<MediaAsset>> Restore(MediaAsset[] saved){var result=new List<MediaAsset>();if(saved==null)return result;foreach(var old in saved){try{var a=await engine.Register(old.path,old.kind,false);engine.Assets.Remove(a.id);a.id=old.id;engine.Assets[a.id]=a;result.Add(Map(a));}catch{}}return result;}
 void SaveLibrary(){var items=engine.Assets.Values.Where(a=>a.kind!="video"&&a.kind!="preview").ToArray();AtomicWrite(Path.Combine(DesktopStart.DataRoot,"library.json"),EditorEngine.Json.Serialize(items));}
 static void AtomicWrite(string path,string text){string temp=path+".tmp";File.WriteAllText(temp,text,new System.Text.UTF8Encoding(false));if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}
 async Task HandleMessage(Dictionary<string,object> d){
  string op=Str(d,"op");object result=null;
  if(op=="cancel"){if(engine.Cancellation!=null)engine.Cancellation.Cancel();Send(new{id=d["id"],ok=true,result=true});return;}
  if(busy)throw new Exception("Bitte den laufenden Vorgang abwarten oder abbrechen.");
  busy=true;try{
   switch(op){
    case "ready":{
     var library=new List<MediaAsset>();string p=Path.Combine(DesktopStart.DataRoot,"library.json");if(File.Exists(p)){try{library=await Restore(EditorEngine.Json.Deserialize<MediaAsset[]>(File.ReadAllText(p)));}catch{}}
     result=new{engine=engine.FFmpeg!=null&&engine.FFprobe!=null,library=library.ToArray(),dataRoot=DesktopStart.DataRoot};break;
    }
    case "import":{
     string kind=Str(d,"kind");if(!new[]{"video","intro","outro","music","sound","font","mask"}.Contains(kind))throw new Exception("Unbekannte Bibliothek.");
     using(var dlg=new OpenFileDialog{Multiselect=false,Title="Datei laden · "+kind,Filter=kind=="font"?"Schriftdateien|*.ttf;*.otf;*.woff;*.woff2":kind=="music"||kind=="sound"?"Audio|*.wav;*.mp3;*.m4a;*.flac;*.ogg":kind=="mask"?"PNG-Maske|*.png":"Medien|*.mp4;*.mov;*.mkv;*.webm;*.avi;*.png;*.jpg;*.jpeg"}){
      if(dlg.ShowDialog(this)==DialogResult.OK){var a=await engine.Register(dlg.FileName,kind,kind!="video");Map(a);SaveLibrary();result=a;}
     }break;
    }
    case "demo":result=Map(await engine.Demo());break;
    case "render":{
     var request=EditorEngine.Json.Deserialize<RenderRequest>(EditorEngine.Json.Serialize(d["request"]));bool preview=Convert.ToBoolean(d["preview"]);string target=null;
     if(!preview){if(DesktopStart.CheckRoot!=null)target=Path.Combine(DesktopStart.CheckRoot,"verified-export.mp4");else using(var dlg=new SaveFileDialog{Filter="MP4 Video|*.mp4",DefaultExt="mp4",AddExtension=true,FileName="VEYLCUT-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".mp4",OverwritePrompt=true}){if(dlg.ShowDialog(this)==DialogResult.OK)target=dlg.FileName;else break;}
      if(engine.Assets.Values.Any(a=>string.Equals(a.path,target,StringComparison.OrdinalIgnoreCase)))throw new Exception("Eine Quelldatei darf nicht überschrieben werden.");
     }
     string output=await engine.Render(request,target,preview);if(preview){var a=await engine.Register(output,"preview",false);result=Map(a);}else{lastOutput=output;result=new{path=output};}break;
    }
    case "saveProject":{
     var settings=(Dictionary<string,object>)d["settings"];var fields=(Dictionary<string,object>)settings["fields"];var selected=new HashSet<string>();
     foreach(string key in new[]{"video","intro","outro","mask"}){string id=Str(settings,key);if(id!=null)selected.Add(id);}
     foreach(string key in new[]{"music","sound","font"}){string id=Str(fields,key);if(id!=null)selected.Add(id.StartsWith("asset:")?id.Substring(6):id);}
     var data=new{version=1,settings=d["settings"],assets=engine.Assets.Values.Where(a=>selected.Contains(a.id)).ToArray()};
     if(DesktopStart.CheckRoot!=null){AtomicWrite(Path.Combine(DesktopStart.CheckRoot,"project.vey"),EditorEngine.Json.Serialize(data));result=true;break;}
     using(var dlg=new SaveFileDialog{Filter="VEYLCUT Projekt|*.vey",DefaultExt="vey",AddExtension=true,FileName="Mein Schnitt.vey"})if(dlg.ShowDialog(this)==DialogResult.OK){AtomicWrite(dlg.FileName,EditorEngine.Json.Serialize(data));result=true;}break;
    }
    case "loadProject":{
     string path=null;if(DesktopStart.CheckRoot!=null)path=Path.Combine(DesktopStart.CheckRoot,"project.vey");else using(var dlg=new OpenFileDialog{Filter="VEYLCUT Projekt|*.vey"})if(dlg.ShowDialog(this)==DialogResult.OK)path=dlg.FileName;
     if(path!=null){var file=new FileInfo(path);if(file.Length>10*1024*1024)throw new Exception("Projektdatei ist zu groß.");var project=EditorEngine.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(path));if(Convert.ToInt32(project["version"])!=1)throw new Exception("Unbekannte Projektversion.");var saved=EditorEngine.Json.Deserialize<MediaAsset[]>(EditorEngine.Json.Serialize(project["assets"]));var restored=await Restore(saved);result=new{settings=project["settings"],assets=restored.ToArray(),missing=saved.Length-restored.Count};}break;
    }
    case "showOutput":if(lastOutput!=null&&File.Exists(lastOutput))Process.Start("explorer.exe","/select,"+EditorEngine.Q(lastOutput));result=true;break;
    default:throw new Exception("Unbekannter Befehl.");
   }
   Send(new{id=d["id"],ok=true,result=result});
  }catch(OperationCanceledException){Send(new{id=d["id"],ok=false,error="Vorgang abgebrochen. Die Quelldateien bleiben erhalten."});}
  finally{busy=false;}
 }
 async Task CaptureImage(string name){using(var s=File.Create(Path.Combine(DesktopStart.CheckRoot,name)))await web.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,s);}
 async Task SelfTest(){
  for(int i=0;i<100;i++){if(await web.CoreWebView2.ExecuteScriptAsync("window.appReady===true")=="true")break;await Task.Delay(100);}
  await CaptureImage("empty.png");
  await web.CoreWebView2.ExecuteScriptAsync("window.runSelfTest().then(r=>{window.testResult=r;window.testDone=true}).catch(e=>{window.testError=e.stack;window.testDone=true})");
  bool done=false;for(int i=0;i<1800;i++){if(await web.CoreWebView2.ExecuteScriptAsync("window.testDone===true")=="true"){done=true;break;}await Task.Delay(100);}
  if(!done)throw new Exception("Self-test timeout");string err=await web.CoreWebView2.ExecuteScriptAsync("window.testError||null");if(err!="null")throw new Exception(err);
  await CaptureImage("editor.png");
  await web.CoreWebView2.ExecuteScriptAsync("document.querySelector('.safe-details').open=true;document.querySelector('.output-panel').scrollIntoView()");await Task.Delay(120);await CaptureImage("output-settings.png");await web.CoreWebView2.ExecuteScriptAsync("document.querySelector('.safe-details').open=false;window.scrollTo(0,0)");
  await web.CoreWebView2.ExecuteScriptAsync("document.querySelector('#look-open').click()");await Task.Delay(120);await CaptureImage("look.png");await web.CoreWebView2.ExecuteScriptAsync("document.querySelector('#look-cancel').click()");
  ClientSize=new Size(980,740);await Task.Delay(200);await CaptureImage("compact.png");
  var before=Size;for(int i=0;i<3;i++){WindowState=FormWindowState.Maximized;await Task.Delay(80);WindowState=FormWindowState.Normal;await Task.Delay(80);}if(Size!=before)throw new Exception("Window maximize/restore changed size");
  string checks=await web.CoreWebView2.ExecuteScriptAsync("JSON.stringify(window.testResult)");File.WriteAllText(Path.Combine(DesktopStart.CheckRoot,"result.json"),checks);
 }
}
}
