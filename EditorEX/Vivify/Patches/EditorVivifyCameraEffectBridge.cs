using SiraUtil.Affinity;
using UnityEngine;
using Vivify.HarmonyPatches;
using Vivify.PostProcessing;

namespace EditorEX.Vivify.Patches
{
    // GameCore also binds CameraEffectApplier. Each instance has its own
    // _postProcessingControllers cache, so the gameplay instance can assign its
    // empty CameraDatas onto the editor MainCamera after (or instead of) the
    // editor instance. Preview-state CreateCamera / DeclareTexture then never
    // reach PostProcessingController, no VivifyCamera children spawn, and
    // shaders that sample those RTs draw nothing.
    internal sealed class EditorVivifyCameraEffectBridge : IAffinity
    {
        private readonly CameraEffectApplier _cameraEffectApplier;

        private EditorVivifyCameraEffectBridge(CameraEffectApplier cameraEffectApplier)
        {
            _cameraEffectApplier = cameraEffectApplier;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MainEffectController), "OnPreRender")]
        private void BindEditorCameraDatas(MainEffectController __instance)
        {
            PostProcessingController? controller =
                __instance.GetComponent<PostProcessingController>();
            if (controller == null)
            {
                return;
            }

            Apply(_cameraEffectApplier, controller);
        }

        internal static void Apply(CameraEffectApplier source, PostProcessingController destination)
        {
            destination.CameraDatas = source.CameraDatas;
            destination.DeclaredTextureDatas = source.DeclaredTextureDatas;
            destination.Effects = source.Effects;
        }
    }
}
