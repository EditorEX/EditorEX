using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.Essentials
{
    public class VersionContext
    {
        public VersionContext(Version version)
        {
            Version = version;
        }

        public Version Version { get; }
    }
}
