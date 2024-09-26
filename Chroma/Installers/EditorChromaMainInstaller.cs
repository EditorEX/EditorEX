using Chroma.EnvironmentEnhancement;
using EditorEX.Chroma.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorChromaMainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<EnvironmentMaterialsManager.EnvironmentMaterialsManagerInitializer>().AsSingle();
            Container.BindInterfacesTo<InitializeLightIDTable>().AsSingle();

            Container.BindInterfacesTo<InjectCustomDataIntoLivePreview>().AsSingle().NonLazy();
        }
    }
}
