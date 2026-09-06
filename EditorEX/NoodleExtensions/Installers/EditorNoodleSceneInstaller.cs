using EditorEX.NoodleExtensions.Events;
using EditorEX.NoodleExtensions.Managers;
using EditorEX.NoodleExtensions.Patches;
using NoodleExtensions.Animation;
using Zenject;

namespace EditorEX.NoodleExtensions.Installers
{
    public class EditorNoodleSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<AnimationHelper>().AsSingle();

            Container.BindInterfacesTo<AssignTrackParentPreviewSource>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditorAssignPlayerToTrack>().AsSingle();
            Container.BindInterfacesTo<AssignPlayerToTrackPreviewSource>().AsSingle();
            Container.BindInterfacesTo<EditorFakeNoteTickPatch>().AsSingle();
            Container.BindInterfacesTo<EditorCutoutEffectPatch>().AsSingle();

            Container.BindInterfacesAndSelfTo<EditorSpawnDataManager>().AsSingle();
        }
    }
}
