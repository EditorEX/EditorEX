using BeatmapEditor3D.InputSystem;
using EditorEX.Essentials.Patches;
using EditorEX.Essentials.Patches.Movement;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.SDK.Input;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PopulateBeatmap>().AsSingle().NonLazy();

            // SceneSetup, DataModels, and CommandInstaller are sibling contexts.
            // App is the parent they all inherit, so load/journal/command share one index.
            Container.Bind<EditorSpawnTimeIndex>().AsSingle();
            Container.Bind<EditorSpawnProcessor>().AsSingle();
            Container.Bind<EditorSpawnDirtyTracker>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditorSpawnDirtyJournal>().AsSingle().NonLazy();
            Container
                .BindInterfacesAndSelfTo<ObjectMarkerOptimizationPatches>()
                .AsSingle()
                .NonLazy();
            Container
                .BindInterfacesAndSelfTo<BasicEventObjectsViewRefreshPatch>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<SwapMovementProvider>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ProfilerMarking>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<CustomInputActionRegistry>().AsSingle().NonLazy();

            CustomInputBuilder
                .StartGroup("EditorEX", "Essentials")
                .AddKeybinding("Toggle Editor GUI", [InputKey.l], ref InputRef.ToggleEditorGUI)
                .AddKeybinding(
                    "Shift Next Viewing Mode",
                    [InputKey.ctrl, InputKey.rightArrow],
                    ref InputRef.ShiftNextViewingMode,
                    true
                )
                .AddKeybinding(
                    "Shift Previous Viewing Mode",
                    [InputKey.ctrl, InputKey.leftArrow],
                    ref InputRef.ShiftPreviousViewingMode,
                    true
                )
                .Build(ref InputRef.EssentialsGroup, Container);
        }
    }
}
