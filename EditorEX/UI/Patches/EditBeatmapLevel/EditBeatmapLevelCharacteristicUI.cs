using System.IO;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BGLib.Polyglot;
using EditorEX.CustomDataModels;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.UI.Components;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
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

        public void Show(Transform child)
        {
            _charModal ??= new CharacteristicSettingsModal();
            _charModal.PresentEditor(child);

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

                var localized = Localization.Get(characteristic.characteristicNameLocalizationKey);
                var displayName = string.IsNullOrWhiteSpace(localized) ? name : localized;

                EditorClickableImage? iconImage = null;

                void ApplyIcon()
                {
                    if (iconImage == null)
                        return;
                    if (!string.IsNullOrWhiteSpace(details.IconFilename))
                        iconImage.Source = Path.Combine(folder, details.IconFilename);
                    else
                        iconImage.Sprite = characteristic.icon;
                }

                var labelInput = new EditorStringInput();
                labelInput.Placeholder = displayName;
                if (!string.IsNullOrWhiteSpace(details.Label))
                {
                    labelInput.InputField.SetTextWithoutNotify(details.Label);
                }
                labelInput.InputField.onEndEdit.AddListener(value =>
                {
                    details.Label = string.IsNullOrWhiteSpace(value) ? null : value;
                });

                content.Children.Add(
                    new LayoutChildren
                    {
                        new EditorClickableImage
                        {
                            PreserveAspect = true,
                            OnClick = () =>
                            {
                                var picked = NativeFileDialogs.OpenFileDialog(
                                    "Select Characteristic Icon",
                                    "png",
                                    folder
                                );
                                if (string.IsNullOrEmpty(picked))
                                    return;

                                var ext = Path.GetExtension(picked);
                                if (string.IsNullOrEmpty(ext))
                                    ext = ".png";

                                var fileName = $"{name}_icon{ext}";
                                var dest = Path.Combine(folder, fileName);
                                File.Copy(picked, dest, overwrite: true);
                                details.IconFilename = fileName;
                                ApplyIcon();
                            },
                        }
                            .Bind(ref iconImage)
                            .With(_ => ApplyIcon())
                            .AsFlexItem(size: new YogaVector(44f, 44f)),
                        labelInput.AsFlexItem(flexGrow: 1f, size: new YogaVector("auto", 30f)),
                    }
                        .AsLayout()
                        .AsFlexGroup(FlexDirection.Row, gap: 10f, alignItems: Align.Center)
                        .AsFlexItem(size: new YogaVector(100.pct, 50f))
                );
            }
        }
    }
}
