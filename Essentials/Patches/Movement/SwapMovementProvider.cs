using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D;
using HarmonyLib;
using SiraUtil.Affinity;

namespace EditorEX.Essentials.Patches.Movement
{
    [AffinityPatch]
    public class SwapMovementProvider : IAffinity
    {
        private static readonly MethodInfo _installOld = AccessTools
            .Method(typeof(Zenject.DiContainer), "BindInterfacesAndSelfTo")
            .MakeGenericMethod(typeof(BeatmapEditorVariableMovementDataProvider));

        private static readonly MethodInfo _installNew = AccessTools
            .Method(typeof(Zenject.DiContainer), "BindInterfacesAndSelfTo")
            .MakeGenericMethod(typeof(VariableMovementDataProvider));

        [AffinityPatch(typeof(BeatmapLevelEditorSceneSetup), nameof(BeatmapLevelEditorSceneSetup.InstallBindings))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerNote(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _installOld))
                .ThrowIfNotMatch("die")
                .SetInstruction(new CodeInstruction(OpCodes.Callvirt, _installNew))
                .InstructionEnumeration();

            return result;
        }
    }
}