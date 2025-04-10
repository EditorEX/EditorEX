using BeatmapEditor3D.InputSystem;
using BetterEditor.Essentials;
using BetterEditor.SDK.Input;
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
            Container.BindInterfacesAndSelfTo<SwapMovementProvider>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<CustomInputActionRegistry>().AsSingle().NonLazy();

            CustomInputBuilder.StartGroup("EditorEX", "Essentials")
                .AddKeybinding("Toggle Editor GUI", [InputKey.l], ref InputRef.ToggleEditorGUI)
                .AddKeybinding("Set Basic Viewing Mode", [InputKey.key1], ref InputRef.ToggleBasicViewingMode)
                .AddKeybinding("Set Preview Viewing Mode", [InputKey.key2], ref InputRef.TogglePreviewViewingMode)
                .AddKeybinding("Set Preview Viewing Mode W/ Camera Lock", [InputKey.key3], ref InputRef.TogglePreviewViewingModeWithCameraLock)
                .Build(ref InputRef.EssentialsGroup, Container);
        }
    }
}
