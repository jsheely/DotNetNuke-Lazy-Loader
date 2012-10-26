using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
namespace TwentyTech.LazyLoader.Components.Utils
{
    class MyRewriterStream : Stream
    {
        private Stream OriginalStream;
        public static string fullContent = "";
        public MyRewriterStream(Stream orig)
        {
            fullContent = "";
            OriginalStream = orig;
            BytesSent = 0;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            string bufferContent = UTF8Encoding.UTF8.GetString(buffer);

            fullContent += bufferContent;

            BytesSent += count;

            OriginalStream.Write(UTF8Encoding.UTF8.GetBytes(bufferContent), offset, UTF8Encoding.UTF8.GetByteCount(bufferContent));
        }
        public long BytesSent { get; set; }
        public override bool CanRead
        {
            get { return OriginalStream.CanRead; }
        }
        public override bool CanSeek
        {
            get { return OriginalStream.CanSeek; }
        }
        public override bool CanWrite
        {
            get { return OriginalStream.CanWrite; }
        }
        public override void Flush()
        {
            OriginalStream.Flush();
        }
        public override long Length
        {
            get { return OriginalStream.Length; }
        }
        public override long Position
        {
            get
            {
                return OriginalStream.Position;
            }
            set
            {
                OriginalStream.Position = value;
            }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return OriginalStream.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return OriginalStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            OriginalStream.SetLength(value);
        }
    }
}