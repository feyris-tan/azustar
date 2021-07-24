using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar.TarModel
{ 
    public enum TypeFlag
    {
        VolumeHeader,
        IncrementalDirectoryInfo, /* see https://www.gnu.org/software/tar/manual/html_node/Dumpdir.html#SEC195 */
        LongFileName,
        NormalFile,
        Continuation,
        Directory
    }
}
