using EditorEX.Analyzer.Swings;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using Zenject;

namespace EditorEX.Analyzer.Installers
{
    public class EditorAnalyzerSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<LevelUtils>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AnalyzerSaberManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
