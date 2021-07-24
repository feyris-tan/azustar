using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzusTar.TarModel;

namespace AzusTar
{
    public class AzusTarFilesystem : IDisposable
    {
        public AzusTarFilesystem(FileInfo[] fileInfos)
        {
            TarFile[] tars = new TarFile[fileInfos.Length];
            for (int i = 0; i < fileInfos.Length; i++)
            {
                tars[i] = new TarFile(fileInfos[i].OpenRead());
            }
            Assemble(tars);
        }

        public AzusTarFilesystem(Stream[] streams)
        {
            TarFile[] tars = new TarFile[streams.Length];
            for (int i = 0; i < streams.Length; i++)
            {
                tars[i] = new TarFile(streams[i]);
            }
            Assemble(tars);
        }

        public AzusTarFilesystem(TarFile[] tars)
        {
            Assemble(tars);
        }

        private TarFile[] tars;
        private void Assemble(TarFile[] tars)
        {
            AzustarFileInfo lastFileInfo = null;
            UstarRecord lastFileRecord = null;
            this.tars = tars;
            for (int i = 0; i < tars.Length; i++)
            {
                IReadOnlyList<UstarRecord> records = tars[i].GetRecords();
                for (int j = 0; j < records.Count; j++)
                {
                    UstarRecord record = records[j];
                    if (record.TypeFlag == TypeFlag.VolumeHeader)
                        continue;
                    if (record.TypeFlag == TypeFlag.IncrementalDirectoryInfo)
                        continue;
                    if (record.TypeFlag == TypeFlag.LongFileName)
                        continue;
                    if (record.TypeFlag == TypeFlag.NormalFile)
                    {
                        string fullname;
                        if (j > 0 && records[j - 1].TypeFlag == TypeFlag.LongFileName)
                        {
                            UstarRecord lfnRecord = records[j - 1];
                            Stream lfnStream = tars[i].GetBackingStream();
                            lfnStream.Position = lfnRecord.PayloadOffset;
                            byte[] lfnBuffer = new byte[lfnRecord.Length];
                            if (lfnStream.Read(lfnBuffer, 0, (int) lfnRecord.Length) != lfnRecord.Length)
                                throw new NotImplementedException("splice in mid-filename");
                            fullname = lfnBuffer.ReadNullTerminatedString(0, lfnBuffer.Length);
                        }
                        else
                            fullname = record.FileName;

                        string[] fullnameVines = fullname.Split('/');
                        AzusTarDirectoryInfo dropHere = EnsureFullPathExists(fullnameVines);
                        string actualName = fullnameVines[fullnameVines.Length - 1];
                        AzustarFileInfo fileInfo =
                            new AzustarFileInfo(record, actualName, dropHere, tars[i].GetBackingStream());
                        lastFileInfo = fileInfo;
                        lastFileRecord = record;
                        continue;
                    }

                    if (record.TypeFlag == TypeFlag.Continuation)
                    {
                        //Continuation does not use LFN records.
                        if (lastFileRecord == null || !record.FileName.Equals(lastFileRecord.FileName))
                        {
                            lastFileInfo.Incomplete = true;
                            continue; //Discontinuity. The file can't be reconstructed.
                        }

                        lastFileInfo.AddSplice(record, this.tars[i].GetBackingStream());
                        continue;
                    }
                }
            }
        }

        private AzusTarDirectoryInfo EnsureFullPathExists(string[] vines)
        {
            if (rootDirectoryInfo == null)
                rootDirectoryInfo = new AzusTarDirectoryInfo("", null);

            AzusTarDirectoryInfo branch = rootDirectoryInfo;
            for (int i = 0; i < vines.Length - 1; i++)
            {
                branch = branch.GetOrAddChild(vines[i]);
            }

            return branch;
        }

        public void Dispose()
        {
            for (int i = 0; i < tars.Length; i++)
            {
                tars[i].Dispose();
            }
        }

        private AzusTarDirectoryInfo rootDirectoryInfo;

        public AzusTarDirectoryInfo RootDirectory => rootDirectoryInfo;

        public IEnumerable<AzustarFileInfo> GetAllFiles()
        {
            Queue<AzusTarDirectoryInfo> searchQueue = new Queue<AzusTarDirectoryInfo>();
            searchQueue.Enqueue(rootDirectoryInfo);

            while (searchQueue.Count > 0)
            {
                AzusTarDirectoryInfo directory = searchQueue.Dequeue();
                foreach (AzusTarDirectoryInfo subdirectory in directory.Subdirectories)
                    searchQueue.Enqueue(subdirectory);
                foreach (AzustarFileInfo fileInfo in directory.Files)
                    yield return fileInfo;
            }
        }
        
    }
}
