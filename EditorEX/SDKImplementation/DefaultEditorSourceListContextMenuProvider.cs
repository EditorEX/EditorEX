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
        private BeatmapsListViewControllerPatches _beatmapsListViewControllerPatches = null!;
        private StringInputDialogModal _stringInputDialogModal = null!;

        [Inject]
        private void Construct(
            SourcesConfig sourcesConfig,
            BeatmapsListViewControllerPatches beatmapsListViewControllerPatches,
            StringInputDialogModal stringInputDialogModal
        )
        {
            _sourcesConfig = sourcesConfig;
            _beatmapsListViewControllerPatches = beatmapsListViewControllerPatches;
            _stringInputDialogModal = stringInputDialogModal;
        }

        public override ContextOption<SourceListContextMenuObject>[] GetContextOptions()
        {
            return [new("Rename", Rename), new("Add Path", AddPath), new("Delete", Delete)];
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

            _beatmapsListViewControllerPatches.ReloadCells();
        }

        private void Delete(SourceListContextMenuObject contextObject)
        {
            if (_sourcesConfig.SelectedSource == contextObject.SourceName)
            {
                var keys = _sourcesConfig.Sources.Keys.ToList();
                _sourcesConfig.SelectedSource = keys[keys.IndexOf(contextObject.SourceName) - 1];
            }
            _sourcesConfig.Sources.Remove(contextObject.SourceName);

            _beatmapsListViewControllerPatches.ReloadCells();
        }

        private void AddPath(SourceListContextMenuObject contextObject)
        {
            // var newSource = NativeFileDialogs.OpenDirectoryDialog($"New {contextObject.SourceName} Directory", Environment.CurrentDirectory);

            // if (newSource != null && Directory.Exists(newSource))
            //     _sourcesConfig.Sources[contextObject.SourceName].Add(newSource.Replace("\\", "/"));

            // _beatmapsListViewControllerPatches.ReloadCells();
        }
    }
}
