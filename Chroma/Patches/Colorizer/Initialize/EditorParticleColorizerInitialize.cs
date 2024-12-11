using EditorEX.Chroma.Colorizer;
using SiraUtil.Affinity;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Chroma.Patches.Colorizer.Initialize
{
    internal class EditorParticleColorizerInitialize : IAffinity
    {
        private readonly EditorParticleColorizerManager _manager;

        private EditorParticleColorizerInitialize(EditorParticleColorizerManager manager)
        {
            _manager = manager;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ParticleSystemEventEffect), nameof(ParticleSystemEventEffect.Start))]
        private void IntializeParticleColorizer(ParticleSystemEventEffect __instance)
        {
            _manager.Create(__instance);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ParticleSystemEventEffect), nameof(ParticleSystemEventEffect.HandleBeatmapEvent))]
        private bool SkipCallback()
        {
            return false;
        }
    }
}
