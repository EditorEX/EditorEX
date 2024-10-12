using EditorEX.NoodleExtensions.Events;
using EditorEX.NoodleExtensions.Managers;
using NoodleExtensions.Animation;
using Zenject;

namespace EditorEX.NoodleExtensions.Installers
{
    public class EditorNoodleSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<AnimationHelper>().AsSingle();

            Container.BindInterfacesTo<EditorAssignTrackParent>().AsSingle();
            Container.BindInterfacesTo<EditorAssignPlayerToTrack>().AsSingle();

            Container.BindInterfacesAndSelfTo<EditorSpawnDataManager>().AsSingle();
        }
    }
}
