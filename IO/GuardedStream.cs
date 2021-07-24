using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar.IO
{
    class GuardedStream : Stream
    {
        public GuardedStream(Stream wrapped, bool mayDispose)
        {
            this.wrapped = wrapped;
            this.mayDispose = mayDispose;
        }

        private Stream wrapped;
        private bool mayDispose;
        private bool disposed;

        public override void Flush()
        {
            if (disposed)
                throw new ObjectDisposedException(this.ToString());
            throw new IOException("Stream is read-only");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (disposed)
                throw new ObjectDisposedException(this.ToString());
            return wrapped.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new IOException("Stream is read-only");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (disposed)
                throw new ObjectDisposedException(this.ToString());
            return wrapped.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new IOException("Stream is read-only");
        }

        public override bool CanRead => wrapped.CanRead;
        public override bool CanSeek => wrapped.CanSeek;
        public override bool CanWrite => false;
        public override long Length => wrapped.Length;

        public override long Position
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(this.ToString());
                return wrapped.Position;
            }
            set
            {
                if (disposed)
                    throw new ObjectDisposedException(this.ToString());
                wrapped.Position = value;
            }
        }

        public override void Close()
        {
            if (!mayDispose)
                return;
            if (disposed)
                throw new ObjectDisposedException(this.ToString());

            wrapped.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (!mayDispose)
                return;

            if (disposed)
                throw new ObjectDisposedException(this.ToString());
            
            base.Dispose(disposing);
            wrapped.Dispose();
            disposed = true;
        }

        public override string ToString()
        {
            return String.Format("{0} (guarded)", wrapped.ToString());
        }
    }
}
