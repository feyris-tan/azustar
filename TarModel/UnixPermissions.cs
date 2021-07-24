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
            int u = Int32.Parse(octal.Substring(0, 1));
            int g = Int32.Parse(octal.Substring(1, 1));
            int w = Int32.Parse(octal.Substring(2, 1));
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
            char[] result = new char[9];
            result.Fill('-');
            if (UserRead) result[0] = 'r';
            if (UserWrite) result[1] = 'w';
            if (UserExecute) result[2] = 'x';
            if (GroupRead) result[3] = 'r';
            if (GroupWrite) result[4] = 'w';
            if (GroupExecute) result[5] = 'x';
            if (WorldRead) result[6] = 'r';
            if (WorldWrite) result[7] = 'w';
            if (WorldExecute) result[8] = 'x';
            return new string(result);
        }
    }
}
