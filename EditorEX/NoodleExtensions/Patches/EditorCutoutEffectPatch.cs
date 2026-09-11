using SiraUtil.Affinity;
using UnityEngine;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.Patches
{
    internal class EditorCutoutEffectPatch : IAffinity
    {
        // Do not run SetCutout if the new value is the same as old.
        [AffinityPrefix]
        [AffinityPatch(
            typeof(CutoutEffect),
            nameof(CutoutEffect.SetCutout),
            AffinityMethodType.Normal,
            null,
            typeof(float),
            typeof(Vector3)
        )]
        private bool CheckDifference(float cutout, float ____cutout)
        {
            return Mathf.Abs(cutout - ____cutout) > 0.001f;
        }

        // A new notecontroller can have its dissolve updated before this runs, causing this to override the cutouteffect
        // Thus setting the cutout effect while the _prevArrowTransparency has a different value.
        [AffinityPrefix]
        [AffinityPatch(typeof(CutoutAnimateEffect), nameof(CutoutAnimateEffect.Start))]
        private bool SkipStart()
        {
            return false;
        }

        // A new notecontroller can have its dissolve updated before this runs, causing this to override the cutouteffect
        // Thus setting the cutout effect while the _prevArrowTransparency has a different value.
        [AffinityPrefix]
        [AffinityPatch(
            typeof(CutoutEffect),
            nameof(CutoutEffect.useRandomCutoutOffset),
            AffinityMethodType.Getter
        )]
        private bool SkipRandom()
        {
            return false;
        }
    }
}
