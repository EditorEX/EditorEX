using Chroma.EnvironmentEnhancement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BetterEditor.Chroma.Installers
{
    public class EditorChromaMainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<EnvironmentMaterialsManager.EnvironmentMaterialsManagerInitializer>().AsSingle();
        }
    }
}
