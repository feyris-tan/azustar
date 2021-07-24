using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar.TarModel
{
    public struct UnixPermissions
    {
        public UnixPermissions(string octal)
        {
            int unk1 = Int32.Parse(octal.Substring(0, 1));
            int unk2 = Int32.Parse(octal.Substring(1, 1));
            int unk3 = Int32.Parse(octal.Substring(2, 1));
            int unk4 = Int32.Parse(octal.Substring(3, 1));
            int u = Int32.Parse(octal.Substring(4, 1));
            int g = Int32.Parse(octal.Substring(5, 1));
            int w = Int32.Parse(octal.Substring(6, 1));
            Directory = (unk3 & 4) != 0;
            UserRead = (u & 4) != 0;
            UserWrite = (u & 2) != 0;
            UserExecute = (u & 1) != 0;
            GroupRead = (g & 4) != 0;
            GroupWrite = (g & 2) != 0;
            GroupExecute = (g & 1) != 0;
            WorldRead = (w & 4) != 0;
            WorldWrite = (w & 2) != 0;
            WorldExecute = (w & 1) != 0;
        }

        public bool Directory { get; private set; }
        public bool UserRead { get; private set; }
        public bool UserWrite { get; private set; }
        public bool UserExecute { get; private set; }
        public bool GroupRead { get; private set; }
        public bool GroupWrite { get; private set; }
        public bool GroupExecute { get; private set; }
        public bool WorldRead { get; private set; }
        public bool WorldWrite { get; private set; }
        public bool WorldExecute { get; private set; }

        public override string ToString()
        {
            char[] result = new char[10];
            result.Fill('-');
            if (Directory) result[0] = 'd';
            if (UserRead) result[1] = 'r';
            if (UserWrite) result[2] = 'w';
            if (UserExecute) result[3] = 'x';
            if (GroupRead) result[4] = 'r';
            if (GroupWrite) result[5] = 'w';
            if (GroupExecute) result[6] = 'x';
            if (WorldRead) result[7] = 'r';
            if (WorldWrite) result[8] = 'w';
            if (WorldExecute) result[9] = 'x';
            return new string(result);
        }
    }
}
