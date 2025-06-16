using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace EditorEX.Config
{
    internal class MainConfig
    {
        public virtual CameraConfig CameraOptions { get; set; } = new();
        public virtual SourcesConfig SourcesOptions { get; set; } = new();
    }
}
