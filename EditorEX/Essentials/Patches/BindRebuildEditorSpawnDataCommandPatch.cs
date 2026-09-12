using BeatmapEditor3D;
using BeatmapEditor3D.LevelEditor;
using EditorEX.Essentials.SpawnProcessing;
using HarmonyLib;
using Zenject;

namespace EditorEX.Essentials.Patches
{
    [HarmonyPatch(typeof(CommandInstaller), nameof(CommandInstaller.Install))]
    internal static class BindRebuildEditorSpawnDataCommandPatch
    {
        private static void Postfix(DiContainer container)
        {
            DiContainer commands = container.ResolveId<DiContainer>("SignalsContainer");
            if (commands.HasBinding<PlaceholderFactory<RebuildEditorSpawnDataCommand>>())
            {
                return;
            }

            commands.BindFactory<
                RebuildEditorSpawnDataCommand,
                PlaceholderFactory<RebuildEditorSpawnDataCommand>
            >();
            container
                .BindSignal<BeatmapLevelUpdatedSignal>()
                .ToMethod(
                    (BeatmapEditorCommandRunnerSignalBinder binder) =>
                        binder.BindSignal<BeatmapLevelUpdatedSignal, RebuildEditorSpawnDataCommand>
                )
                .FromResolve();
        }
    }
}
