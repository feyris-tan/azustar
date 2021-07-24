using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar
{
    class VolumeSplice
    {
        internal Stream backing;
        internal long offset;
        internal long size;

        public override string ToString()
        {
            return String.Format("{0},{1},{2}", backing.ToString(), offset, size);
        }
    }
}
