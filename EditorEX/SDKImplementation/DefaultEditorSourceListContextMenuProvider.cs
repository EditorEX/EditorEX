using System.Linq;
using EditorEX.Config;
using EditorEX.SDK.Components;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.ContextMenu.Objects;
using EditorEX.UI.Patches;
using Zenject;

namespace EditorEX.SDKImplementation
{
    public class DefaultEditorSourceListContextMenuProvider
        : ContextMenuProvider<SourceListContextMenuObject>
    {
        private SourcesConfig _sourcesConfig = null!;
        private IBeatmapsListRefresh _beatmapsListRefresh = null!;
        private StringInputDialogModal _stringInputDialogModal = null!;

        [Inject]
        private void Construct(
            SourcesConfig sourcesConfig,
            IBeatmapsListRefresh beatmapsListRefresh,
            StringInputDialogModal stringInputDialogModal
        )
        {
            _sourcesConfig = sourcesConfig;
            _beatmapsListRefresh = beatmapsListRefresh;
            _stringInputDialogModal = stringInputDialogModal;
        }

        public override ContextOption<SourceListContextMenuObject>[] GetContextOptions()
        {
            return [new("Rename", Rename), new("Delete", Delete)];
        }

        private void Rename(SourceListContextMenuObject contextObject)
        {
            _stringInputDialogModal.Prompt(
                "Rename Source",
                "Name",
                contextObject.SourceName,
                x =>
                {
                    _sourcesConfig.Sources[x] = _sourcesConfig.Sources[contextObject.SourceName];
                    _sourcesConfig.Sources.Remove(contextObject.SourceName);
                },
                null
            );

            _beatmapsListRefresh.ReloadCells();
        }

        private void Delete(SourceListContextMenuObject contextObject)
        {
            if (_sourcesConfig.SelectedSource == contextObject.SourceName)
            {
                var keys = _sourcesConfig.Sources.Keys.ToList();
                _sourcesConfig.SelectedSource = keys[keys.IndexOf(contextObject.SourceName) - 1];
            }
            _sourcesConfig.Sources.Remove(contextObject.SourceName);

            _beatmapsListRefresh.ReloadCells();
        }
    }
}
