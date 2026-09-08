namespace EditorEX.Essentials.Visuals
{
    internal static class DissolveCutout
    {
        internal static bool TryGetChanged(float? dissolve, ref float lastCutout, out float cutout)
        {
            if (!dissolve.HasValue)
            {
                cutout = lastCutout;
                return false;
            }

            cutout = 1f - dissolve.Value;
            if (cutout == lastCutout)
            {
                return false;
            }

            lastCutout = cutout;
            return true;
        }
    }
}
