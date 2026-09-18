using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using Jano.AppKit;
class EngineSmoke {
 static int Main(string[] args){try{Run(args[0]).GetAwaiter().GetResult();return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
 static async Task Run(string folder){
  Directory.CreateDirectory(folder);var engine=new EditorEngine(folder);
  string footage=Path.Combine(folder,"Gameplay weiß & blau.mp4"),png=Path.Combine(folder,"Alpha grün.png"),mov=Path.Combine(folder,"Alpha grün.mov"),music=Path.Combine(folder,"Musik.wav"),sound=Path.Combine(folder,"Sound.wav");
  await EditorEngine.ProcessRun(engine.FFmpeg,"-v error -y -f lavfi -i color=c=0x203060:s=640x360:r=30:d=8 -c:v libx264 -pix_fmt yuv420p "+EditorEngine.Q(footage),CancellationToken.None);
  using(var b=new Bitmap(320,180)){using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);g.FillRectangle(Brushes.Lime,60,15,80,40);}b.Save(png,ImageFormat.Png);}
  await EditorEngine.ProcessRun(engine.FFmpeg,"-v error -y -loop 1 -i "+EditorEngine.Q(png)+" -t 1 -c:v qtrle -pix_fmt argb "+EditorEngine.Q(mov),CancellationToken.None);
  await EditorEngine.ProcessRun(engine.FFmpeg,"-v error -y -f lavfi -i sine=frequency=440:sample_rate=48000:duration=5 "+EditorEngine.Q(music),CancellationToken.None);
  await EditorEngine.ProcessRun(engine.FFmpeg,"-v error -y -f lavfi -i sine=frequency=880:sample_rate=48000:duration=0.5 "+EditorEngine.Q(sound),CancellationToken.None);
  var v=await engine.Register(footage,"video",false);var i=await engine.Register(mov,"intro",false);var o=await engine.Register(png,"outro",false);var m=await engine.Register(music,"music",false);var s=await engine.Register(sound,"sound",false);
  string empty;using(var b=new Bitmap(1080,1080))using(var ms=new MemoryStream()){b.Save(ms,ImageFormat.Png);empty="data:image/png;base64,"+Convert.ToBase64String(ms.ToArray());}
  var request=new RenderRequest{video=v.id,introAsset=i.id,outroAsset=o.id,music=m.id,sound=s.id,width=1080,height=1080,fit="contain",originalVolume=.5,musicVolume=.3,soundVolume=.5,segments=new[]{new Segment{start=.4,duration=2},new Segment{start=5,duration=2}},introPng=empty,textPng=empty,outroPng=empty};
  string output=await engine.Render(request,null,true);var info=await engine.Register(output,"preview",false);if(info.width!=360||info.height!=360||Math.Abs(info.duration-4)>.06||!info.audio)throw new Exception("Render metadata invalid");
  int n=0;foreach(double t in new[]{.3,2.0,3.6}){string frame=Path.Combine(folder,"frame"+n+".png");await EditorEngine.ProcessRun(engine.FFmpeg,"-v error -y -ss "+EditorEngine.N(t)+" -i "+EditorEngine.Q(output)+" -frames:v 1 "+EditorEngine.Q(frame),CancellationToken.None);using(var b=new Bitmap(frame)){var p=b.GetPixel(100,110);if(n!=1&&!(p.G>170&&p.R<65&&p.B<65))throw new Exception("Alpha overlay missing at "+t+": "+p);if(n==1&&!(p.B>p.G&&p.G>p.R))throw new Exception("Middle should preserve footage: "+p);var outside=b.GetPixel(230,150);if(!(outside.B>outside.G&&outside.G>outside.R))throw new Exception("Overlay background is not transparent");}n++;}
  // Output cancellation must never produce the user's selected final destination.
  string cancelled=Path.Combine(folder,"cancelled.mp4");engine.Progress=p=>{if(engine.Cancellation!=null)engine.Cancellation.Cancel();};bool didCancel=false;try{await engine.Render(request,cancelled,false);}catch(OperationCanceledException){didCancel=true;}if(!didCancel||File.Exists(cancelled))throw new Exception("Cancellation did not protect final output");
  File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: Unicode paths; source without audio; real MOV alpha intro and PNG outro; transparent pixels; correctly timed overlays; music plus sound; 1:1 contain; preview duration; cancellation protects final output.");Console.WriteLine(File.ReadAllText(Path.Combine(folder,"PASS.txt")));
 }
}
