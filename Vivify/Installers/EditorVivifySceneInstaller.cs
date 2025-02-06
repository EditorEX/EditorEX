using EditorEX.MapData.Contexts;
using EditorEX.Vivify.Events;
using EditorEX.Vivify.Managers;
using Vivify.HarmonyPatches;
using Vivify.Managers;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorVivifySceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (MapContext.Version.Major < 4)
            {
                Container.BindInterfacesAndSelfTo<EditorAssetBundleManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<PrefabManager>().AsSingle();
                
                Container.BindInterfacesAndSelfTo<CameraPropertyManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<CameraEffectApplier>().AsSingle();
                
                Container.BindInterfacesTo<EditorApplyPostProcessing>().AsSingle();
                Container.BindInterfacesTo<EditorDeclareCullingTexture>().AsSingle();
                Container.BindInterfacesTo<EditorDeclareRenderTexture>().AsSingle();
                Container.BindInterfacesTo<EditorDestroyPrefab>().AsSingle();
                Container.BindInterfacesTo<EditorInstantiatePrefab>().AsSingle();
                Container.BindInterfacesTo<EditorSetAnimatorProperty>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorSetCameraProperty>().AsSingle();
                Container.BindInterfacesTo<EditorSetGlobalProperty>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorSetMaterialProperty>().AsSingle();
                Container.BindInterfacesTo<EditorSetRenderingSettings>().AsSingle();
            }
        }
    }
}
