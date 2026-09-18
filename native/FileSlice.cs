using System;
using System.IO;
namespace Jano.AppKit {
// Bounded, seekable stream for large local media and HTTP range requests.
sealed class FileSlice:Stream {
 readonly FileStream stream;readonly long offset,length;long position;
 public FileSlice(string path,long start,long count){stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);offset=start;length=count;stream.Position=start;}
 public override bool CanRead{get{return true;}}public override bool CanSeek{get{return true;}}public override bool CanWrite{get{return false;}}public override long Length{get{return length;}}
 public override long Position{get{return position;}set{Seek(value,SeekOrigin.Begin);}}
 public override int Read(byte[] buffer,int start,int count){int n=stream.Read(buffer,start,(int)Math.Min(count,length-position));position+=n;return n;}
 public override long Seek(long value,SeekOrigin origin){long target=origin==SeekOrigin.Begin?value:origin==SeekOrigin.Current?position+value:length+value;if(target<0||target>length)throw new IOException("Invalid range");stream.Position=offset+target;position=target;return position;}
 public override void Flush(){}public override void SetLength(long value){throw new NotSupportedException();}public override void Write(byte[] buffer,int offset,int count){throw new NotSupportedException();}
 protected override void Dispose(bool disposing){if(disposing)stream.Dispose();base.Dispose(disposing);}
}
}
