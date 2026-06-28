using BeatmapEditor3D;
using HarmonyLib;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.Essentials.Patches;

public class ObjectMarkerOptimizationPatches : IAffinity
{
    [AffinityPrefix]
    [AffinityPatch(
        typeof(BeatmapLevelEditorInstaller),
        nameof(BeatmapLevelEditorInstaller.InstallBindings)
    )]
    private void UpdatePrefab(BeatmapLevelEditorInstaller __instance)
    {
        Plugin.Logger.Info("Hello");
        Object.Destroy(__instance._textEventMarkerObjectPrefab._sideText);
        __instance._textEventMarkerObjectPrefab._topText.isTextObjectScaleStatic = true;

        Object.Destroy(__instance._lightEventMarkerObjectPrefab._sideText);
        Object.Destroy(__instance._lightEventMarkerObjectPrefab._sideFadeOutSprite);
        Object.Destroy(__instance._lightEventMarkerObjectPrefab._sideFlashOnSprite);
        Object.Destroy(__instance._lightEventMarkerObjectPrefab._sideOnSprite);
        __instance._lightEventMarkerObjectPrefab._topText.isTextObjectScaleStatic = true;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(TextEventMarkerObject), nameof(TextEventMarkerObject.SetText))]
    private bool FixNullAccess(TextEventMarkerObject __instance, string text)
    {
        __instance._topText.text = text;
        if (__instance._sideText == null)
            return false;
        __instance._sideText.text = text;
        return false;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(LightEventMarkerObject), nameof(LightEventMarkerObject.SetEventValue))]
    private bool FixNullAccess2(LightEventMarkerObject __instance, int value, float floatValue)
    {
        var hasSide = __instance._sideText != null;
        var hasTop = __instance._topText != null;

        var text = string.Format("{0:F0}", (double)floatValue * 100.0);

        if (hasSide)
            __instance._sideText.text = text;

        if (hasTop)
            __instance._topText.text = text;

        if (hasSide)
        {
            __instance._sideCurrentSprite?.SetActive(false);
            __instance._sideCurrentSprite = null;
        }

        if (hasTop)
        {
            __instance._topCurrentSprite?.SetActive(false);
            __instance._topCurrentSprite = null;
        }

        switch (value)
        {
            case 1:
            case 5:
            case 9:
                if (hasSide)
                {
                    __instance._sideCurrentSprite = __instance._sideOnSprite;
                }

                if (hasTop)
                {
                    __instance._topCurrentSprite = __instance._topOnSprite;
                }
                break;
            case 2:
            case 4:
            case 6:
            case 8:
            case 10:
            case 12:
                if (hasSide)
                {
                    __instance._sideCurrentSprite = __instance._sideFadeOutSprite;
                }

                if (hasTop)
                {
                    __instance._topCurrentSprite = __instance._topFadeOutSprite;
                }
                break;
            case 3:
            case 7:
            case 11:
                if (hasSide)
                {
                    __instance._sideCurrentSprite = __instance._sideFlashOnSprite;
                }

                if (hasTop)
                {
                    __instance._topCurrentSprite = __instance._topFlashOnSprite;
                }
                break;
        }

        if (__instance._topCurrentSprite != null)
        {
            __instance._topCurrentSprite?.SetActive(true);
        }

        if (__instance._sideCurrentSprite != null)
        {
            __instance._sideCurrentSprite?.SetActive(true);
        }

        return false;
    }
}
