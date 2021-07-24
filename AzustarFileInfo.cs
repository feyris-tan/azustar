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
    public class AzustarFileInfo
    {
        public AzustarFileInfo(UstarRecord record, string actualName, AzusTarDirectoryInfo directory, Stream backing)
        {
            this.Name = actualName;
            this.Parent = directory;
            this.Parent.AddFile(this);
            this.volumeSplices = new List<VolumeSplice>();
            this.AddSplice(record, backing);
            this.Length = record.Length;
        }

        internal void AddSplice(UstarRecord record, Stream backing)
        {
            if (record.TypeFlag == TypeFlag.NormalFile && Splices == 0)
            {
                VolumeSplice first = new VolumeSplice();
                first.backing = backing;
                first.offset = record.PayloadOffset;
                first.size = record.Length;
                volumeSplices.Add(first);
            }
            else if (record.TypeFlag == TypeFlag.Continuation && Splices > 0)
            {
                VolumeSplice lastSplice = volumeSplices.Last();
                lastSplice.size -= record.Length;

                VolumeSplice next = new VolumeSplice();
                next.backing = backing;
                next.offset = record.PayloadOffset;
                next.size = record.Length;
                volumeSplices.Add(next);
            }
            else
                throw new NotImplementedException(String.Format("{0}, {1}", record.TypeFlag, Splices));
        }

        public string Name { get; }
        public AzusTarDirectoryInfo Parent { get; }
        private List<VolumeSplice> volumeSplices;
        public int Splices => volumeSplices.Count;
        public long Length { get; }
        public bool Incomplete { get; internal set; }

        public Stream CreateStream()
        {
            if (Splices == 1)
            {
                VolumeSplice splice = volumeSplices[0];
                return new SubStream(splice.backing, splice.offset, splice.size);
            }
            else
            {
                Stream[] parts = new Stream[Splices];
                for (int i = 0; i < parts.Length; i++)
                {
                    VolumeSplice splice = volumeSplices[i];
                    parts[i] = new SubStream(splice.backing, splice.offset, splice.size);
                }
                return new ConcatStream(parts);
            }
        }
    }
}
