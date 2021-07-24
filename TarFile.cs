using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzusTar.IO;
using AzusTar.TarModel;

namespace AzusTar
{
    public class TarFile : IDisposable
    {
        public TarFile(FileInfo fi)
        : this(fi.OpenRead())
        {

        }

        public TarFile(Stream s)
        {
            if (!s.CanSeek)
                throw new ArgumentException("Requires seekable tars!", nameof(s));

            records = new List<UstarRecord>();
            wrappedStream = s;

            while (s.Position < s.Length)
            {
                UstarRecord record = UstarRecord.ReadFrom(s);
                if (record != null)
                {
                    s.Position += record.Length;
                    records.Add(record);
                }

                s.AlignPosition(512);
            }
        }

        private List<UstarRecord> records;
        private Stream wrappedStream;
        private GuardedStream guardedStream;

        public IReadOnlyList<UstarRecord> GetRecords()
        {
            return records.AsReadOnly();
        }

        public Stream GetBackingStream()
        {
            if (guardedStream == null)
                guardedStream = new GuardedStream(wrappedStream, false);
            return guardedStream;
        }

        public void Dispose()
        {
            wrappedStream?.Dispose();
            records.Clear();
        }
    }
}
