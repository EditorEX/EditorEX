using BetterEditor.Chroma.Lighting;
using Chroma.Lighting;
using SiraUtil.Affinity;

namespace Chroma.HarmonyPatches.Colorizer.Initialize
{
    internal class EditorLightColorizerInitialize : IAffinity
    {
        private readonly EditorChromaLightSwitchEventEffect.Factory _factory;

        private EditorLightColorizerInitialize(EditorChromaLightSwitchEventEffect.Factory factory)
        {
            _factory = factory;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LightSwitchEventEffect), nameof(LightSwitchEventEffect.Start))]
        private bool IntializeChromaLightSwitchEventEffect(LightSwitchEventEffect __instance)
        {
            _factory.Create(__instance);
            return false;
        }
    }
}
