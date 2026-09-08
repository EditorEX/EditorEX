using System;

namespace EditorEX.UI.Patches
{
    internal static class VariableNjsSupport
    {
        public static bool IsAvailable(Version? beatmapVersion)
        {
            return beatmapVersion != null && beatmapVersion.Major >= 4;
        }
    }
}
