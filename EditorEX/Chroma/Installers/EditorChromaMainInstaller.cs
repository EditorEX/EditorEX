using EditorEX.Chroma.EnvironmentEnhancement;
using EditorEX.Chroma.Patches;
using EditorEX.Chroma.Patches.EnvironmentComponent;
using EditorEX.SDK.AddressableHelpers;
using UnityEngine;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorChromaMainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<EditorEnvironmentMaterialsManager.EditorEnvironmentMaterialsManagerInitializer>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<EditorRingAwakeInstantiator>().AsSingle();

            Container.BindInterfacesTo<InjectCustomDataIntoLivePreview>().AsSingle().NonLazy();
        }
    }
}
