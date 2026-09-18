using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Jano.AppKit {
public class MediaAsset {
 public string id, name, path, kind, url;
 public double duration; public int width,height; public bool audio;
}
public class Segment { public double start,duration; }
public class RenderRequest {
 public string video, music, sound, introAsset, outroAsset, fit;
 public int width,height; public double originalVolume,musicVolume,soundVolume;
 public Segment[] segments;
 public string introPng,textPng,outroPng;
}
public class EditorEngine {
 public static readonly JavaScriptSerializer Json=new JavaScriptSerializer{MaxJsonLength=64*1024*1024};
 public readonly string Root,FFmpeg,FFprobe;
 public readonly Dictionary<string,MediaAsset> Assets=new Dictionary<string,MediaAsset>();
 public CancellationTokenSource Cancellation;
 public Action<double> Progress;
 public EditorEngine(string root){Root=root;Directory.CreateDirectory(root);FFmpeg=Find("ffmpeg.exe");FFprobe=Find("ffprobe.exe");}
 static string Find(string name){
  string bundled=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"engine",name);if(File.Exists(bundled))return bundled;
  foreach(var dir in (Environment.GetEnvironmentVariable("PATH")??"").Split(';')){try{string p=Path.Combine(dir.Trim('"'),name);if(File.Exists(p))return p;}catch{}}
  string winget=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Microsoft","WinGet","Packages");
  if(Directory.Exists(winget))foreach(var dir in Directory.GetDirectories(winget,"Gyan.FFmpeg*")){var files=Directory.GetFiles(dir,name,SearchOption.AllDirectories);if(files.Length>0)return files[0];}
  return null;
 }
 public static string Q(string s){if(s.IndexOf('\0')>=0 || s.IndexOf('"')>=0)throw new ArgumentException("Invalid path");return "\""+s+new string('\\',s.Reverse().TakeWhile(c=>c=='\\').Count())+"\"";}
 public static string N(double n){return n.ToString("0.######",CultureInfo.InvariantCulture);}
 public static async Task<string> ProcessRun(string exe,string args,CancellationToken token,Action<string> line=null){
  if(exe==null)throw new Exception("FFmpeg / FFprobe fehlt. Bitte FFmpeg installieren und die App neu starten.");
  using(var p=new Process{StartInfo=new ProcessStartInfo(exe,args){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8}}){
   var log=new StringBuilder();p.ErrorDataReceived+=(s,e)=>{if(e.Data!=null)lock(log){if(log.Length>16000)log.Remove(0,8000);log.AppendLine(e.Data);}};
   p.Start();p.BeginErrorReadLine();
   using(token.Register(()=>{try{if(!p.HasExited)p.Kill();}catch{}})){
    string output="";if(line==null)output=await p.StandardOutput.ReadToEndAsync();else {string row;while((row=await p.StandardOutput.ReadLineAsync())!=null)line(row);}
    await Task.Run(()=>p.WaitForExit());token.ThrowIfCancellationRequested();if(p.ExitCode!=0)throw new Exception("Medienverarbeitung fehlgeschlagen:\n"+log.ToString());return output;
   }
  }
 }
 public async Task<MediaAsset> Register(string path,string kind,bool copy){
  path=Path.GetFullPath(path);if(!File.Exists(path))throw new FileNotFoundException("Datei nicht gefunden",path);
  string id=Guid.NewGuid().ToString("N");string originalName=Path.GetFileName(path);
  if(copy){string folder=Path.Combine(Root,"library",id);Directory.CreateDirectory(folder);string dest=Path.Combine(folder,originalName);File.Copy(path,dest);path=dest;}
  var a=new MediaAsset{id=id,name=originalName,path=path,kind=kind};
  if(kind!="font"){
   string raw=await ProcessRun(FFprobe,"-v error -print_format json -show_format -show_streams "+Q(path),CancellationToken.None);
   var d=Json.Deserialize<Dictionary<string,object>>(raw);var streams=(System.Collections.ArrayList)d["streams"];
   foreach(Dictionary<string,object> stream in streams){string type=Convert.ToString(stream["codec_type"]);if(type=="video"){a.width=Convert.ToInt32(stream["width"]);a.height=Convert.ToInt32(stream["height"]);}if(type=="audio")a.audio=true;}
   if(d.ContainsKey("format")){var f=(Dictionary<string,object>)d["format"];if(f.ContainsKey("duration"))double.TryParse(Convert.ToString(f["duration"]),NumberStyles.Any,CultureInfo.InvariantCulture,out a.duration);}
   if(kind=="video"&&(a.width==0||a.duration<=0))throw new Exception("Diese Datei enthält kein lesbares Video mit Laufzeit.");
   if((kind=="music"||kind=="sound")&&!a.audio)throw new Exception("Die Datei enthält keine Audiospur.");
   if((kind=="intro"||kind=="outro"||kind=="mask")&&a.width==0)throw new Exception("Die Datei enthält kein Bild oder Video.");
  }
  Assets[id]=a;return a;
 }
 public MediaAsset Asset(string id){MediaAsset a;if(id==null||!Assets.TryGetValue(id,out a)||!File.Exists(a.path))throw new Exception("Die Mediendatei ist nicht mehr verfügbar. Bitte erneut laden.");return a;}
 public async Task<MediaAsset> Demo(){
  string p=Path.Combine(Root,"demo.mp4");if(!File.Exists(p))await ProcessRun(FFmpeg,"-hide_banner -loglevel error -y -f lavfi -i testsrc2=size=960x540:rate=30 -f lavfi -i sine=frequency=220:sample_rate=48000 -t 12 -c:v libx264 -preset ultrafast -crf 22 -pix_fmt yuv420p -c:a aac -shortest "+Q(p),CancellationToken.None);
  return await Register(p,"video",false);
 }
 static string Png(string data,string folder,string name){
  if(data==null||!data.StartsWith("data:image/png;base64,"))throw new Exception("Vorschauebene fehlt.");
  var bytes=Convert.FromBase64String(data.Substring(22));if(bytes.Length>16*1024*1024)throw new Exception("Bildebene ist zu groß.");
  string p=Path.Combine(folder,name+".png");File.WriteAllBytes(p,bytes);return p;
 }
 public async Task<string> Render(RenderRequest r,string target,bool preview){
  var video=Asset(r.video);if(r.segments==null||r.segments.Length<1||r.segments.Length>120)throw new Exception("Bitte zuerst einen Schnitt erstellen.");
  if(!new[]{"1920x1080","1080x1920","1080x1080","1080x1350"}.Contains(r.width+"x"+r.height))throw new Exception("Ungültiges Ausgabeformat.");
  double duration=0;foreach(var s in r.segments){if(double.IsNaN(s.start)||double.IsNaN(s.duration)||s.start<0||s.duration<.15||s.start+s.duration>video.duration+.08)throw new Exception("Schnitt liegt außerhalb des Videos.");duration+=s.duration;}
  if(duration>300)throw new Exception("Der erste Prototyp unterstützt maximal 5 Minuten je Ausgabe.");
  foreach(double v in new[]{r.originalVolume,r.musicVolume,r.soundVolume})if(double.IsNaN(v)||v<0||v>1)throw new Exception("Ungültige Lautstärke.");
  string folder=Path.Combine(Root,"renders",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
  string temp=Path.Combine(folder,"render.mp4");int w=preview?r.width/3:r.width,h=preview?r.height/3:r.height;w=w/2*2;h=h/2*2;
  double edge=Math.Min(2,duration*.2),outStart=duration-edge;
  var args=new StringBuilder("-hide_banner -loglevel error -y -i "+Q(video.path));
  var graph=new StringBuilder();int next=1;
  for(int i=0;i<r.segments.Length;i++){
   var s=r.segments[i];string resize=r.fit=="contain"?"scale="+w+":"+h+":force_original_aspect_ratio=decrease,pad="+w+":"+h+":(ow-iw)/2:(oh-ih)/2:color=0x090d10":"scale="+w+":"+h+":force_original_aspect_ratio=increase,crop="+w+":"+h;
   graph.Append("[0:v]trim=start="+N(s.start)+":duration="+N(s.duration)+",setpts=PTS-STARTPTS,"+resize+",setsar=1,fps=30,format=yuv420p[v"+i+"];");
   if(video.audio)graph.Append("[0:a]atrim=start="+N(s.start)+":duration="+N(s.duration)+",asetpts=PTS-STARTPTS,aresample=48000,aformat=sample_fmts=fltp:channel_layouts=stereo,apad,atrim=duration="+N(s.duration)+"[a"+i+"];");
   else graph.Append("anullsrc=r=48000:cl=stereo,atrim=duration="+N(s.duration)+"[a"+i+"];");
  }
  for(int i=0;i<r.segments.Length;i++)graph.Append("[v"+i+"][a"+i+"]");graph.Append("concat=n="+r.segments.Length+":v=1:a=1[base][original];[original]volume="+N(r.originalVolume)+"[dry];");
  string current="base";
  string[] pngs={r.introPng,r.textPng,r.outroPng};
  for(int layer=0;layer<3;layer++){
   string external=layer==0?r.introAsset:layer==2?r.outroAsset:null;
   int input=next++;string path=external==null?Png(pngs[layer],folder,"layer"+layer):Asset(external).path;
   bool still=external==null||Asset(external).duration<=0;
   args.Append(still?" -loop 1 -framerate 30":" -stream_loop -1");args.Append(" -i "+Q(path));
   string prefix="ov"+layer;double offset=layer==2?outStart:0;
   graph.Append("["+input+":v]format=rgba,scale="+w+":"+h+":force_original_aspect_ratio=decrease,pad="+w+":"+h+":(ow-iw)/2:(oh-ih)/2:color=black@0,setsar=1,setpts=PTS-STARTPTS+"+N(offset)+"/TB["+prefix+"];");
   string enable=layer==0?"between(t,0,"+N(edge)+")":layer==2?"gte(t,"+N(outStart)+")":"between(t,"+N(edge)+","+N(outStart)+")";
   string output="layered"+layer;graph.Append("["+current+"]["+prefix+"]overlay=0:0:enable='"+enable+"':eof_action=pass:repeatlast=0["+output+"];");current=output;
  }
  var audio=new List<string>{"dry"};
  if(r.music!=null){var a=Asset(r.music);if(!a.audio)throw new Exception("Musik enthält kein Audio.");args.Append(" -stream_loop -1 -i "+Q(a.path));graph.Append("["+(next++)+":a]aresample=48000,atrim=duration="+N(duration)+",asetpts=PTS-STARTPTS,volume="+N(r.musicVolume)+",afade=t=out:st="+N(Math.Max(0,duration-1))+":d=1[bgm];");audio.Add("bgm");}
  if(r.sound!=null){var a=Asset(r.sound);if(!a.audio)throw new Exception("Sound enthält kein Audio.");args.Append(" -i "+Q(a.path));graph.Append("["+(next++)+":a]aresample=48000,atrim=duration="+N(edge)+",asetpts=PTS-STARTPTS,volume="+N(r.soundVolume)+",adelay="+((int)(outStart*1000))+":all=1[sfx];");audio.Add("sfx");}
  foreach(var a in audio)graph.Append("["+a+"]");graph.Append("amix=inputs="+audio.Count+":duration=first:normalize=0,alimiter=limit=0.95:latency=1[aout]");
  string script=Path.Combine(folder,"filters.txt");File.WriteAllText(script,graph.ToString(),new UTF8Encoding(false));
  args.Append(" -filter_complex_threads 2 -filter_complex_script "+Q(script)+" -map ["+current+"] -map [aout] -t "+N(duration)+" -c:v libx264 -preset "+(preview?"ultrafast":"fast")+" -crf "+(preview?"25":"19")+" -pix_fmt yuv420p -c:a aac -b:a 192k -movflags +faststart -progress pipe:1 -nostats "+Q(temp));
  Cancellation=new CancellationTokenSource();
  try{await ProcessRun(FFmpeg,args.ToString(),Cancellation.Token,row=>{if(row.StartsWith("out_time_us=")){double t;if(double.TryParse(row.Substring(12),out t)&&Progress!=null)Progress(Math.Min(.99,t/1000000/duration));}});
   if(preview){if(Progress!=null)Progress(1);return temp;}
   string staged=target+"."+Guid.NewGuid().ToString("N")+".partial";try{File.Copy(temp,staged);if(File.Exists(target))File.Replace(staged,target,null);else File.Move(staged,target);}finally{if(File.Exists(staged))File.Delete(staged);}
   if(Progress!=null)Progress(1);return target;
  }finally{Cancellation.Dispose();Cancellation=null;}
 }
}
}
