using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace EditorEX.Config
{
    internal class SourcesConfig
    {
        public virtual string SelectedSource { get; set; } = "Custom Levels";

        [UseConverter(typeof(DictionaryConverter<string>))]
        public virtual Dictionary<string, string> Sources { get; set; } = new();
    }
}
