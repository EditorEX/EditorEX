using System.Collections.Generic;
using SiraUtil.Affinity;
using UnityEngine;
using Vivify.Controllers;
using Vivify.Managers;
using Vivify.PostProcessing;
using Vivify.TrackGameObject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Patches
{
    // Vivify's CullingCameraController.OnPreCull walks every child renderer on every
    // culling track, HashSet.Add-ing (GameObject, layer) and calling set_layer each
    // PreCull. The editor makes that list huge: a 16s spawn window, Basic+Game visual
    // trees under one tracked root, and GetComponentsInChildren(includeInactive: true).
    // Replace the rebuild and the layer dance with unique active GameObjects and a List.
    internal sealed class EditorCullingCameraPatch : IAffinity
    {
        private static readonly Dictionary<int, CameraScratch> _scratches = new();

        [AffinityPrefix]
        [AffinityPatch(
            typeof(CullingTextureTracker),
            nameof(CullingTextureTracker.GameObjects),
            AffinityMethodType.Getter
        )]
        private bool UniqueActiveGameObjects(
            ref GameObject[] __result,
            ref GameObject[] ____gameObjects,
            ref bool ____gameObjectsDirty,
            HashSet<RendererController> ____maskRenderers
        )
        {
            if (!____gameObjectsDirty)
            {
                __result = ____gameObjects;
                return false;
            }

            ____gameObjects = CullingLayerScratch.UniqueActive(____maskRenderers);
            ____gameObjectsDirty = false;
            __result = ____gameObjects;
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(CullingCameraController), "OnPreCull")]
        private bool FasterPreCull(CullingCameraController __instance)
        {
            CullingTextureTracker? data = __instance.CullingTextureData;
            if (data != null && data.Whitelist)
            {
                __instance._cachedMask = __instance.Camera.cullingMask;
                __instance.Camera.cullingMask = 1 << CULLING_LAYER;
            }

            if (data == null)
            {
                return false;
            }

            CameraScratch scratch = ScratchFor(__instance);
            CullingLayerScratch.Capture(
                data.GameObjects,
                scratch.Seen,
                scratch.Cache,
                CULLING_LAYER
            );
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(CullingCameraController), "OnPostRender")]
        private bool FasterPostRender(CullingCameraController __instance)
        {
            if (__instance._cachedMask.HasValue)
            {
                __instance.Camera.cullingMask = __instance._cachedMask.Value;
                __instance._cachedMask = null;
            }

            if (_scratches.TryGetValue(__instance.GetInstanceID(), out CameraScratch? scratch))
            {
                CullingLayerScratch.Restore(scratch.Cache);
                scratch.Seen.Clear();
            }

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(CameraPropertyManager.CameraProperties), "set_CullingTextureData")]
        private void DisposeReplacedTracker(
            CullingTextureTracker? ____cullingTextureData,
            CullingTextureTracker? value
        )
        {
            if (____cullingTextureData != null && !ReferenceEquals(____cullingTextureData, value))
            {
                ____cullingTextureData.Dispose();
            }
        }

        private static CameraScratch ScratchFor(CullingCameraController controller)
        {
            int id = controller.GetInstanceID();
            if (!_scratches.TryGetValue(id, out CameraScratch? scratch))
            {
                scratch = new CameraScratch();
                _scratches[id] = scratch;
            }

            return scratch;
        }

        private sealed class CameraScratch
        {
            internal HashSet<int> Seen { get; } = new();

            internal List<(GameObject obj, int layer)> Cache { get; } = new();
        }
    }
}
