using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Signals;
using EditorEX.SDKImplementation.Patches;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Installers
{
    public class EditorSDKModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<IAddressableCollectorItemLoadedSignal>().OptionalSubscriber();

            Container.BindInterfacesAndSelfTo<AllowSignalInterfacesPatches>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<FixUnityExplorerInputError>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<AddressableCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressableSignalBus>().AsSingle();
        }
    }
}
