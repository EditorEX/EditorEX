using BetterEditor.SDKImplementation.Patches;
using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Signals;
using EditorEX.SDKImplementation.Patches;
using System;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Installers
{
    public class EditorSDKAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InjectCustomKeybindings>().AsSingle().NonLazy();
        }
    }
}
