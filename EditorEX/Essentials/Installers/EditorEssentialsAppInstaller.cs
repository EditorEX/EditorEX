using BeatmapEditor3D.InputSystem;
using EditorEX.Essentials.Patches;
using EditorEX.Essentials.Patches.Movement;
using EditorEX.SDK.Input;
using EditorEX.Util;
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

            CustomInputBuilder
                .StartGroup("EditorEX", "Essentials")
                .AddKeybinding("Toggle Editor GUI", [InputKey.l], ref InputRef.ToggleEditorGUI)
                .AddViewModeBindings()
                .Build(ref InputRef.EssentialsGroup, Container);
        }
    }
}
