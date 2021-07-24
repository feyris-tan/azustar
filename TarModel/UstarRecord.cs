using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar.TarModel
{
    public class UstarRecord
    {
        private UstarRecord() { }
        public static UstarRecord ReadFrom(Stream s)
        {
            UstarRecord child = new UstarRecord();
            child.HeaderOffset = s.Position;

            byte[] buffer = new byte[512];
            if (s.Read(buffer, 0, 512) != 512)
                throw new EndOfStreamException();

            bool empty = true;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != 0)
                {
                    empty = false;
                    break;
                }
            }
            if (empty)
                return null;
            
            child.FileName = buffer.ReadNullTerminatedString(0, 100);

            string filemodeOctal = buffer.ReadNullTerminatedString(100, 8);
            if (!string.IsNullOrEmpty(filemodeOctal))
            {
                child.FileMode = new UnixPermissions(filemodeOctal);
            }

            string ownerOctal = buffer.ReadNullTerminatedString(108, 8);
            if (!string.IsNullOrEmpty(ownerOctal))
            {
                child.OwnerUserId = Convert.ToInt32(ownerOctal, 8);
            }

            string groupOctal = buffer.ReadNullTerminatedString(116, 8);
            if (!string.IsNullOrEmpty(groupOctal))
            {
                child.OwnerGroupId = Convert.ToInt32(groupOctal, 8);
            }

            string fileSizeOctal = buffer.ReadNullTerminatedString(124, 12);
            if (!string.IsNullOrEmpty(fileSizeOctal))
            {
                child.Length = Convert.ToInt64(fileSizeOctal, 8);
            }

            string lastModOctal = buffer.ReadNullTerminatedString(136, 12);
            if (!string.IsNullOrEmpty(lastModOctal))
            {
                long lastModDecimal = Convert.ToInt64(lastModOctal, 8);
                child.LastModificationTime = ExtensionMethods.UnixTimeStampToDateTime(lastModDecimal);
            }

            string checksum = buffer.ReadNullTerminatedString(148, 8);
            if (!string.IsNullOrEmpty(checksum))
            {
                child.Checksum = Convert.ToInt32(checksum, 8);
            }

            string linkIndicator = buffer.ReadNullTerminatedString(156, 1);
            switch (linkIndicator[0])
            {
                /* Info taken from https://serverfault.com/a/897948 */
                case 'V': child.TypeFlag = TypeFlag.VolumeHeader; break;
                case 'D': child.TypeFlag = TypeFlag.IncrementalDirectoryInfo; break;
                case 'L': child.TypeFlag = TypeFlag.LongFileName; break;
                case '0': child.TypeFlag = TypeFlag.NormalFile; break;
                case 'M': child.TypeFlag = TypeFlag.Continuation; break;
                case '5': child.TypeFlag = TypeFlag.Directory; break;
                default:
                    throw new NotImplementedException(linkIndicator);
            }

            string nameOfLinkedFile = buffer.ReadNullTerminatedString(157, 100);
            if (!string.IsNullOrEmpty(nameOfLinkedFile))
                throw new NotImplementedException(nameof(nameOfLinkedFile));

            string ustarMagic = buffer.ReadNullTerminatedString(257, 8);
            if (!string.IsNullOrEmpty(ustarMagic))
            {
                ustarMagic = ustarMagic.Trim();
                switch (ustarMagic)
                {
                    case "ustar":
                        break;
                    default:
                        throw new NotImplementedException(ustarMagic);
                }
            }
                                                                                   
            string ownerUsername = buffer.ReadNullTerminatedString(265, 32);
            if (!string.IsNullOrEmpty(ownerUsername))
            {
                child.OwnerUserName = ownerUsername;
            }

            string ownerGroupName = buffer.ReadNullTerminatedString(297, 32);
            if (!string.IsNullOrEmpty(ownerGroupName))
            {
                child.OwnerGroupName = ownerGroupName;
            }

            string deviceMajorNumber = buffer.ReadNullTerminatedString(329, 8);
            if (!string.IsNullOrEmpty(deviceMajorNumber))
                throw new NotImplementedException(nameof(deviceMajorNumber));

            string deviceMinorNumber = buffer.ReadNullTerminatedString(337, 8);
            if (!string.IsNullOrEmpty(deviceMinorNumber))
                throw new NotImplementedException(nameof(deviceMinorNumber));

            string filenamePrefix = buffer.ReadNullTerminatedString(345, 155);
            if (!string.IsNullOrEmpty(filenamePrefix))
                if (child.TypeFlag != TypeFlag.IncrementalDirectoryInfo && child.TypeFlag != TypeFlag.NormalFile)
                    throw new NotImplementedException(nameof(filenamePrefix));

            long actualChecksum = 0;
            for (int i = 0; i < 512; i++)
            {
                bool inChksum = i >= 148 && i < (148 + 8);
                actualChecksum += inChksum ? (byte)' ' : buffer[i];
            }

            child.ChecksumValid = actualChecksum == child.Checksum;
            child.PayloadOffset = s.Position;
            return child;
        }

        public long HeaderOffset { get; private set; }
        public string FileName { get; private set; }
        public DateTime LastModificationTime { get; set; }
        private int Checksum;
        public TypeFlag TypeFlag { get; private set; }

        public override string ToString()
        {
            return String.Format("{0}: {1}", TypeFlag.ToString(), FileName);
        }

        public UnixPermissions FileMode { get; private set; }
        public int OwnerUserId { get; private set; }
        public int OwnerGroupId { get; private set; }
        public long Length { get; private set; }
        public string OwnerUserName { get; private set; }
        public string OwnerGroupName { get; private set; }
        public bool ChecksumValid { get; private set; }
        public long PayloadOffset { get; private set; }
    }
}
