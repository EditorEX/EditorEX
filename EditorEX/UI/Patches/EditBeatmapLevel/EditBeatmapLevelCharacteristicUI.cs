using System.IO;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomDataModels;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.UI.Components;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using Zenject;

namespace EditorEX.UI.Patches.EditBeatmapLevel
{
    /// <summary>
    /// Populates the <see cref="CharacteristicSettingsModal"/> shell with one row per
    /// characteristic present in the map, binding edits to
    /// <see cref="ILevelCustomDataModel.CharacteristicDetailsByName"/>.
    /// </summary>
    internal class EditBeatmapLevelCharacteristicUI
    {
        private readonly BeatmapLevelDataModel _beatmapLevelDataModel;
        private readonly ILevelCustomDataModel _levelCustomDataModel;
        private readonly LazyInject<BeatmapProjectManager> _beatmapProjectManager;

        private CharacteristicSettingsModal? _charModal;

        public EditBeatmapLevelCharacteristicUI(
            BeatmapLevelDataModel beatmapLevelDataModel,
            ILevelCustomDataModel levelCustomDataModel,
            LazyInject<BeatmapProjectManager> beatmapProjectManager
        )
        {
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _levelCustomDataModel = levelCustomDataModel;
            _beatmapProjectManager = beatmapProjectManager;
        }

        public void Show()
        {
            _charModal ??= new CharacteristicSettingsModal();
            _charModal.IsPushed = true;

            var content = _charModal.ContentLayout;
            content.Children.Clear();

            var folder = _beatmapProjectManager.Value._workingBeatmapProject ?? string.Empty;

            var characteristics = _beatmapLevelDataModel
                .difficultyBeatmaps.Keys.Select(k => k.Item1)
                .Where(c => c != null)
                .Distinct();

            foreach (var characteristic in characteristics)
            {
                var name = characteristic.serializedName;
                if (
                    !_levelCustomDataModel.CharacteristicDetailsByName.TryGetValue(
                        name,
                        out var details
                    )
                )
                {
                    details = new CharacteristicDetailsData();
                    _levelCustomDataModel.CharacteristicDetailsByName[name] = details;
                }

                // Default icon is the editor's "open" glyph, hinting the image is clickable.
                string IconSource() =>
                    string.IsNullOrWhiteSpace(details.IconFilename)
                        ? "#IconOpen"
                        : Path.Combine(folder, details.IconFilename);

                EditorImageButton? iconButton = null;

                var labelInput = new EditorStringInput { Placeholder = name };
                labelInput.InputField.SetTextWithoutNotify(details.Label ?? string.Empty);
                labelInput.InputField.onEndEdit.AddListener(value =>
                {
                    details.Label = string.IsNullOrWhiteSpace(value) ? null : value;
                });

                content.Children.Add(
                    new LayoutChildren
                    {
                        new EditorImageButton
                        {
                            Source = IconSource(),
                            OnClick = () =>
                            {
                                var picked = NativeFileDialogs.OpenFileDialog(
                                    "Select Characteristic Icon",
                                    "png",
                                    folder
                                );
                                if (string.IsNullOrEmpty(picked))
                                    return;
                                // Store the bare filename when the image lives in the level
                                // folder (SongCore convention), otherwise the full path.
                                var fileName = Path.GetFileName(picked);
                                var inFolder = Path.Combine(folder, fileName);
                                details.IconFilename = File.Exists(inFolder) ? fileName : picked;
                                if (iconButton != null)
                                    iconButton.Source = IconSource();
                            },
                        }
                            .Bind(ref iconButton)
                            .AsFlexItem(size: new YogaVector(44f, 44f)),
                        labelInput.AsFlexItem(flexGrow: 1f, size: new YogaVector("auto", 40f)),
                    }
                        .AsLayout()
                        .AsFlexGroup(FlexDirection.Row, gap: 10f, alignItems: Align.Center)
                        .AsFlexItem(size: new YogaVector(100.pct, 50f))
                );
            }
        }
    }
}
