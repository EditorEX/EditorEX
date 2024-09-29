using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.MapData.Contexts
{
    //TODO: Phase this out to 100% zenject like everything else...
    public static class MapContext
    {
        public static Version Version { get; set; }
    }
}
