using BetterEditor.Essentials.Features.HideUI;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsCommandInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<HideUIFeatureToggledSignal>().OptionalSubscriber();
        }
    }
}
