using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar.IO
{
    class SubStream : Stream
    {
        public SubStream(Stream wrapped, long wrappedOffset, long wrappedLength)
        {
            if (!wrapped.CanRead)
                throw new ArgumentException("non-seekable stream", nameof(wrapped));

            this.wrapped = wrapped;
            this.wrappedOffset = wrappedOffset;
            this.wrappedLength = wrappedLength;
        }

        private Stream wrapped;
        private long wrappedOffset;
        private long wrappedLength;
        private long internalPosition;


        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    internalPosition = offset;
                    return internalPosition;
                case SeekOrigin.Current:
                    internalPosition += offset;
                    return internalPosition;
                case SeekOrigin.End:
                    internalPosition = wrappedLength - offset;
                    return internalPosition;
                default:
                    throw new NotImplementedException(origin.ToString());
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (internalPosition + count > wrappedLength)
                count = (int)(wrappedLength - internalPosition);
            if (count == 0)
                return 0;

            wrapped.Position = wrappedOffset + internalPosition;
            int result = wrapped.Read(buffer, offset, count);
            internalPosition += result;
            return result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => wrapped.CanRead;
        public override bool CanSeek => wrapped.CanSeek;
        public override bool CanWrite => false;
        public override long Length => wrappedLength;

        public override long Position
        {
            get { return internalPosition; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("must be positive");
                internalPosition = value;
            }
        }

        public override string ToString()
        {
            return String.Format("{0},{1},{2}", wrapped.ToString(), wrappedOffset, wrappedLength);
        }
    }
}
