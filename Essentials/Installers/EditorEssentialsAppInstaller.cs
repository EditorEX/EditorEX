﻿using EditorEX.Essentials.Patches;
using EditorEX.Essentials.SpawnProcessing;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PopulateBeatmap>().AsSingle().NonLazy();
        }
    }
}