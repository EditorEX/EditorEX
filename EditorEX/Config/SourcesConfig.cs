using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace EditorEX.Config
{
    internal class SourcesConfig
    {
        public virtual string SelectedSource { get; set; } = "Custom Levels";

        public virtual string SaveSource { get; set; } = "Custom WIP Levels";

        [UseConverter(typeof(DictionaryConverter<string>))]
        public virtual Dictionary<string, string> Sources { get; set; } = new();
    }
}
