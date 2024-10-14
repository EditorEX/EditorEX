using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace EditorEX.Config
{
    internal class MainConfig
    {
        public virtual CameraConfig CameraOptions { get; set; } = new CameraConfig();
        public virtual SourcesConfig SourcesOptions { get; set; } = new SourcesConfig();
    }
}
