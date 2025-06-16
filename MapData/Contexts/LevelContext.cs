using System;

namespace EditorEX.MapData.Contexts
{
    //TODO: Phase this out to 100% zenject like everything else...
    public static class LevelContext
    {
        public static Version Version { get; set; }

        public static void Reset() { }
    }

    public class EditorEXtraSongData { }
}
