using Zenject;

namespace EditorEX.Config.Installers
{
    internal class EditorConfigAppInstaller : Installer
    {
        private readonly MainConfig _config;

        public EditorConfigAppInstaller(MainConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_config.CameraOptions).AsSingle();
            Container.BindInstance(_config.SourcesOptions).AsSingle();
        }
    }
}
