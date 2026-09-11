using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Integration.Patches;
using EditorEX.SDK.Signals;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Installers
{
    public class EditorSDKCommandInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AllowSignalInterfacesPatches>().AsSingle().NonLazy();
            Container.DeclareSignal<IAddressableCollectorItemLoadedSignal>().OptionalSubscriber();
            Container.BindInterfacesAndSelfTo<AddressableCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressableSignalBus>().AsSingle();

            Container
                .Bind<IAddressableCollectorItem>()
                .To<DefaultAddressableCollectorItem<Shader>>()
                .FromInstance(new("waterlit", "Assets/Visuals/Shaders/WaterLit.shader"));

            Container
                .Bind<IAddressableCollectorItem>()
                .To<DefaultAddressableCollectorItem<Shader>>()
                .FromInstance(new("glowing", "Assets/Visuals/Shaders/Glowing.shader"));
        }
    }
}
