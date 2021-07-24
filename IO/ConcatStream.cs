using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar.IO
{
    class ConcatStream : Stream
    {
        Stream[] _parts;
        int _current_part_no;
        Stream _current_part;
        bool disposed;

        public ConcatStream(Stream[] _s)
        {
            _parts = _s;
            _current_part = _parts[0];
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                long result = 0;
                foreach (Stream part in _parts)
                {
                    result += part.Length;
                }

                return result;
            }
        }

        public override long Position
        {
            get
            {
                if (_current_part_no == 0)
                {
                    return _current_part.Position;
                }
                else
                {
                    long result = 0;
                    for (int i = 0; i < _current_part_no; i++)
                    {
                        result += _parts[i].Length;
                    }

                    result += _current_part.Position;
                    return result;
                }
            }
            set
            {
                long target = value;
                int tmppart = -1;
                for (int i = 0; i < _parts.Length; i++)
                {
                    _parts[i].Position = 0;
                }

                while (target > 0)
                {
                    tmppart++;
                    if (_parts[tmppart].Length > target)
                    {
                        _current_part_no = tmppart;
                        _current_part = _parts[tmppart];
                        _current_part.Position = target;
                        return;
                    }

                    target -= _parts[tmppart].Length;
                }
            }
        }

        public bool Disposed => disposed;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = 0;
            while (count > 0)
            {
                int readAmount = (int) Math.Min(count, Math.Min(Int32.MaxValue, _current_part.Length - _current_part.Position));
                int partialResult = _current_part.Read(buffer, offset, readAmount);
                result += partialResult;
                count -= partialResult;
                offset += partialResult;
                if (_current_part.Position == _current_part.Length)
                {
                    _current_part_no++;
                    if (_current_part_no == _parts.Length)
                    {
                        _current_part_no--;
                        return result;
                    }

                    _current_part = _parts[_current_part_no];
                    _current_part.Position = 0;
                }
            }

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (disposed)
                throw new ObjectDisposedException(ToString());

            long target = 0;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    target = offset;
                    break;
                case SeekOrigin.Current:
                    target = Position + offset;
                    break;
                case SeekOrigin.End:
                    target = Length + offset;
                    break;
            }

            Position = target;
            return Position;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            for (int i = 0; i < _parts.Length; i++)
            {
                _parts[i].Dispose();
            }

            disposed = true;
        }

        public override string ToString()
        {
            return String.Format("ConcatStream ({0} parts)", _parts.Length);
        }
    }
}
