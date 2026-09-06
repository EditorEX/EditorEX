using EditorEX.Essentials.Visuals.Note;
using EditorEX.MapData.Contexts;
using EditorEX.Vivify.Events;
using EditorEX.Vivify.Managers;
using EditorEX.Vivify.ObjectPrefab.Managers;
using EditorEX.Vivify.Patches;
using Vivify.HarmonyPatches;
using Vivify.Managers;
using Zenject;

namespace EditorEX.Vivify.Installers
{
    public class EditorVivifySceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (MapContext.Version.Major < 4)
            {
                Container.BindInterfacesAndSelfTo<EditorAssetBundleManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<PrefabManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorBeatmapObjectPrefabManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorVivifyNotePrefabManager>().AsSingle();

                Container.BindInterfacesAndSelfTo<CameraPropertyManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<CameraEffectApplier>().AsSingle();
                Container.BindInterfacesTo<EditorVivifyCameraEffectBridge>().AsSingle();

                Container.BindInterfacesTo<VivifyObjectPreviewSource>().AsSingle();
                Container.BindInterfacesTo<VivifyPostProcessingPreviewSource>().AsSingle();
                Container.BindInterfacesTo<VivifySetMaterialPropertyPreviewSource>().AsSingle();
                Container.BindInterfacesTo<VivifyAssignObjectPrefabPreviewSource>().AsSingle();
                Container.BindInterfacesTo<VivifySetAnimatorPropertyPreviewSource>().AsSingle();
                Container.BindInterfacesTo<VivifySetCameraPropertyPreviewSource>().AsSingle();
                Container.BindInterfacesTo<VivifySetGlobalPropertyPreviewSource>().AsSingle();
                Container.BindInterfacesTo<VivifySetRenderingSettingsPreviewSource>().AsSingle();

                Container.BindInterfacesAndSelfTo<EditorAssignObjectPrefab>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorSetAnimatorProperty>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorSetCameraProperty>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorSetGlobalProperty>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorSetMaterialProperty>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorSetRenderingSettings>().AsSingle();
            }
        }
    }
}
