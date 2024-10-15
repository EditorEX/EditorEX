using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace EditorEX.Config
{
    internal class SourcesConfig
    {
        public virtual string SelectedSource { get; set; } = "Custom Levels";

        [UseConverter(typeof(DictionaryConverter<List<string>, ListConverter<string>>))]
        public virtual Dictionary<string, List<string>> Sources { get; set; } = new Dictionary<string, List<string>>();
    }
}
