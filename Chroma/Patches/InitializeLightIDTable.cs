using BeatmapEditor3D;
using Chroma.Lighting;
using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.Chroma.Patches
{
    [AffinityPatch]
    public class InitializeLightIDTable : IAffinity
    {
        [AffinityPatch(typeof(BeatmapEditorLevelSceneTransitionSetupDataSO), nameof(BeatmapEditorLevelSceneTransitionSetupDataSO.Init))]
        [AffinityPostfix]
        private void Postfix(EnvironmentInfoSO environmentInfo)
        {
            LightIDTableManager.SetEnvironment(environmentInfo.serializedName);
        }
    }
}
