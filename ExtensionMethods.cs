using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar
{
    public static class ExtensionMethods
    {
        public static string ReadNullTerminatedString(this byte[] buffer, int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (buffer[offset + i] == 0)
                {
                    length = i;
                    break;
                }
            }

            string result = Encoding.UTF8.GetString(buffer, offset, length);
            return result;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static void Fill<T>(this T[] originalArray, T with)
        {
            for (int i = 0; i < originalArray.Length; i++)
            {
                originalArray[i] = with;
            }
        }

        public static void AlignPosition(this Stream self, int blockiness)
        {
            if (self.Position % blockiness != 0)
            {
                long nextBlockOffset = self.Position / blockiness;
                nextBlockOffset++;
                nextBlockOffset *= blockiness;
                self.Position = nextBlockOffset;
            }
        }
    }
}
