using EditorEX.Essentials.Features.HideUI;
using EditorEX.Essentials.Features.ViewMode;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsCommandInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<HideUIFeatureToggledSignal>().OptionalSubscriber();
            Container.DeclareSignal<ViewModeSwitchedSignal>().OptionalSubscriber();
        }
    }
}
