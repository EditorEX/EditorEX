using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using EditorEX.SDK.ReactiveComponents.SegmentedControl;
using EditorEX.UI.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.UI.Patches.EditBeatmapLevel
{
    /// <summary>
    /// Builds the "Song Info" tab layout for <see cref="EditBeatmapLevelViewController"/>
    /// (song/beatmap tab switch, cover, song/author fields, audio info, environment
    /// dropdowns) and applies <c>RefreshData</c>/save-state updates to that layout.
    /// </summary>
    internal class EditBeatmapLevelSongInfoUI
    {
        private readonly ILevelCustomDataModel _levelCustomDataModel;
        private readonly EnvironmentsListModel _environmentsListModel;
        private readonly CustomPlatformsListModel _customPlatformsListModel;
        private readonly IColorCollector _colorCollector;
        private readonly IReactiveContainer _reactiveContainer;
        private readonly EditBeatmapLevelCoverController _coverController;
        private readonly EditBeatmapLevelCharacteristicUI _characteristicUI;

        private readonly State<bool> V4Level = StateUtils.Remember(false);
        private readonly State<int> MainTab = StateUtils.Remember(0);

        private StringInputFieldValidator? _levelAuthorInputValidator;
        private EditorDropdown<string>? _environmentDropdown;
        private EditorDropdown<string>? _allDirectionsEnvironmentDropdown;
        private EditorDropdown<string>? _customPlatformDropdown;

        public EditBeatmapLevelSongInfoUI(
            ILevelCustomDataModel levelCustomDataModel,
            EnvironmentsListModel environmentsListModel,
            CustomPlatformsListModel customPlatformsListModel,
            IColorCollector colorCollector,
            IReactiveContainer reactiveContainer,
            EditBeatmapLevelCoverController coverController,
            EditBeatmapLevelCharacteristicUI characteristicUI
        )
        {
            _levelCustomDataModel = levelCustomDataModel;
            _environmentsListModel = environmentsListModel;
            _customPlatformsListModel = customPlatformsListModel;
            _colorCollector = colorCollector;
            _reactiveContainer = reactiveContainer;
            _coverController = coverController;
            _characteristicUI = characteristicUI;
        }

        public void RefreshData(bool clearModifiedState)
        {
            V4Level.Value = LevelContext.Version.Major >= 4;

            if (!V4Level)
            {
                _levelAuthorInputValidator!.SetValueWithoutNotify(
                    _levelCustomDataModel.LevelAuthorName,
                    clearModifiedState
                );
                if (_levelCustomDataModel.EnvironmentName != null)
                {
                    _environmentDropdown!.Key = _levelCustomDataModel.EnvironmentName;
                }
                if (_levelCustomDataModel.AllDirectionsEnvironmentName != null)
                {
                    _allDirectionsEnvironmentDropdown!.Key =
                        _levelCustomDataModel.AllDirectionsEnvironmentName;
                }
            }

            if (_levelCustomDataModel.CustomPlatformInfo != null)
            {
                var customPlatIndex = _customPlatformsListModel
                    .CustomPlatforms.Select(x => x.FilePath)
                    .ToList()
                    .IndexOf(_levelCustomDataModel.CustomPlatformInfo.FilePath);
                customPlatIndex =
                    customPlatIndex == -1
                        ? _customPlatformsListModel
                            .CustomPlatforms.Select(x => x.Hash)
                            .ToList()
                            .IndexOf(_levelCustomDataModel.CustomPlatformInfo.Hash)
                        : customPlatIndex;

                if (customPlatIndex == -1)
                {
                    //_customPlatformDropdown._text.text = _levelCustomDataModel.CustomPlatformInfo.FilePath;
                }
                else
                {
                    //_customPlatformDropdown.Select();
                }
            }
        }

        public void HandleBeatmapProjectSaved()
        {
            _levelAuthorInputValidator!.ClearDirtyState();
            //_environmentDropdown.ClearDirtyState();
            //_allDirectionsEnvironmentDropdown.ClearDirtyState();
            //_customPlatformDropdown.ClearDirtyState();
        }

        public void BuildLayout(EditBeatmapLevelViewController controller)
        {
            var difficultyBeatmapSetContainer = controller
                .transform.Find("DifficultyBeatmapSetContainer")
                .gameObject;
            difficultyBeatmapSetContainer.EnabledWithState(MainTab, 1);
            var infoContainer = controller.transform.Find("BeatmapInfoContainer");
            infoContainer.gameObject.SetActive(false);

            var secondaryTab = StateUtils.Remember(0);
            var normalEnvironmentItems = _environmentsListModel
                .GetAllEnvironmentInfosWithType(EnvironmentType.Normal)
                .ToDictionary(
                    info => info.serializedName,
                    info => new BsDropdownItem(info.environmentName, null)
                );
            var allDirectionsEnvironmentItems = _environmentsListModel
                .GetAllEnvironmentInfosWithType(EnvironmentType.Circle)
                .ToDictionary(
                    info => info.serializedName,
                    info => new BsDropdownItem(info.environmentName, null)
                );

            new LayoutChildren
            {
                new EditorSegmentedControl
                {
                    SelectedIndex = MainTab,
                    Values = ["Song Info", "Beatmaps"],
                    TabbingType = TabbingType.Alpha,
                }.AsFlexItem(size: new YogaVector(300f, 30f)),
                new EditorLabelButton
                {
                    Text = "Custom Characteristics",
                    OnClick = () => _characteristicUI.Show(),
                }
                    .EnabledWithState(MainTab, 1)
                    .AsFlexItem(size: new YogaVector(240f, 30f)),
                new LayoutChildren
                {
                    new LayoutChildren
                    {
                        new LayoutChildren
                        {
                            _coverController
                                .CreateCoverImageComponent(controller)
                                .AsFlexItem(size: 200f),
                            new LayoutChildren()
                                .AsLayout()
                                .AsFlexItem()
                                .AsFlexGroup(FlexDirection.Column),
                        }
                            .AsLayout()
                            .AsFlexItem()
                            .AsFlexGroup(FlexDirection.Row),
                        new LayoutChildren
                        {
                            new EditorStringInput()
                                .WithInputValidatorCopy<StringInputFieldValidator, string>(
                                    controller._songNameInputValidator,
                                    ref controller._songNameInputValidator
                                )
                                .InEditorNamedRail("Song Name", 18f, 55f)
                                .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                            new EditorStringInput()
                                .WithInputValidatorCopy<StringInputFieldValidator, string>(
                                    controller._songSubNameInputValidator,
                                    ref controller._songSubNameInputValidator
                                )
                                .InEditorNamedRail("Song Sub Name", 18f, 55f)
                                .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                            new EditorStringInput()
                                .WithInputValidatorCopy<StringInputFieldValidator, string>(
                                    controller._songAuthorNameInputValidator,
                                    ref controller._songAuthorNameInputValidator
                                )
                                .InEditorNamedRail("Song Author Name", 18f, 55f)
                                .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                            new EditorStringInput()
                                .WithInputValidator<StringInputFieldValidator, string>(
                                    out _levelAuthorInputValidator,
                                    x =>
                                        controller._signalBus.Fire(
                                            new BeatmapDataModelSignals.UpdateBeatmapDataSignal(
                                                null,
                                                null,
                                                null,
                                                x
                                            )
                                        )
                                )
                                .InEditorNamedRail("Level Author Name", 18f, 55f)
                                .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                .EnabledWithState(V4Level, false)
                                .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                        }
                            .AsLayout()
                            .AsFlexGroup(
                                gap: 40f,
                                direction: FlexDirection.Column,
                                alignItems: Align.FlexStart
                            )
                            .AsFlexItem(),
                        new LayoutChildren // Audio Info
                        {
                            new LayoutChildren
                            {
                                new LayoutChildren
                                {
                                    new EditorImage { Source = "#IconOpen" }.AsFlexItem(
                                        size: new YogaVector(30f, 30f)
                                    ),
                                }
                                    .As<EditorBackgroundButton>(x =>
                                    {
                                        x.Source = "#Background8px";
                                    })
                                    .AsFlexGroup(
                                        justifyContent: Justify.Center,
                                        alignItems: Align.Center
                                    )
                                    .AsFlexItem(size: new YogaVector(40f, 40f))
                                    .With(x =>
                                    {
                                        controller._editBpmGridButton = x.Component.Button;
                                    }),
                                new EditorStringInput()
                                    .WithInputValidatorCopy<FloatInputFieldValidator, float>(
                                        controller._beatsPerMinuteInputValidator,
                                        ref controller._beatsPerMinuteInputValidator
                                    )
                                    .InEditorNamedRail("BPM", 18f, 35f)
                                    .LinkNamedRailWithValidator<FloatInputFieldValidator, float>()
                                    .AsFlexItem(size: new() { x = 260f, y = "auto" }),
                            }
                                .AsLayout()
                                .AsFlexGroup(gap: 4f)
                                .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                            new EditorStringInput()
                                .WithInputValidatorCopy<FloatInputFieldValidator, float>(
                                    controller._previewStartTimeInputValidator,
                                    ref controller._previewStartTimeInputValidator
                                )
                                .InEditorNamedRail("Preview Start Time", 18f, 30f)
                                .LinkNamedRailWithValidator<FloatInputFieldValidator, float>()
                                .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                            new EditorStringInput()
                                .WithInputValidatorCopy<FloatInputFieldValidator, float>(
                                    controller._previewDurationInputValidator,
                                    ref controller._previewDurationInputValidator
                                )
                                .InEditorNamedRail("Preview Duration", 18f, 30f)
                                .LinkNamedRailWithValidator<FloatInputFieldValidator, float>()
                                .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                        }
                            .AsLayout()
                            .AsFlexGroup(
                                gap: 40f,
                                direction: FlexDirection.Column,
                                alignItems: Align.FlexStart
                            )
                            .AsFlexItem(size: new() { x = 60f.pct, y = "auto" }),
                    }
                        .AsLayout()
                        .AsFlexGroup(
                            gap: 60f,
                            direction: FlexDirection.Column,
                            alignItems: Align.FlexStart,
                            padding: new YogaFrame(40f, 0f, 0f, 0f)
                        )
                        .AsFlexItem(size: new() { x = 500f, y = "auto" }),
                    new LayoutChildren // Right Side
                    {
                        new EditorSegmentedControl
                        {
                            SelectedIndex = secondaryTab,
                            Values = ["Environments", "Contributors"],
                            TabbingType = TabbingType.Qwerty,
                        }.AsFlexItem(size: new YogaVector(250f, 30f)),
                        new LayoutChildren
                        {
                            new LayoutChildren // Environments
                            {
                                new EditorDropdown<string>
                                {
                                    Items = normalEnvironmentItems,
                                    Key = normalEnvironmentItems.Keys.First(),
                                }
                                    .EnabledWithState(V4Level, false)
                                    .Bind(ref _environmentDropdown)
                                    .ExtractStateFromDropdown(out var normalEnvironmentValue)
                                    .DropdownWithState(normalEnvironmentValue)
                                    .On(
                                        normalEnvironmentValue,
                                        (x, v) =>
                                        {
                                            Debug.Log(
                                                $"Selected Normal Environment: {v.Item2.Text}"
                                            );
                                        }
                                    )
                                    .AsFlexItem(size: new YogaVector(200f, 40f))
                                    .InEditorNamedRail("Environment", 18f),
                                new EditorDropdown<string>
                                {
                                    Items = allDirectionsEnvironmentItems,
                                    Key = allDirectionsEnvironmentItems.Keys.First(),
                                }
                                    .EnabledWithState(V4Level, false)
                                    .Bind(ref _allDirectionsEnvironmentDropdown)
                                    .ExtractStateFromDropdown(out var allDirectionsEnvironmentValue)
                                    .DropdownWithState(allDirectionsEnvironmentValue)
                                    .On(
                                        allDirectionsEnvironmentValue,
                                        (x, v) =>
                                        {
                                            Debug.Log(
                                                $"Selected All Directions Environment: {v.Item2.Text}"
                                            );
                                        }
                                    )
                                    .AsFlexItem(size: new YogaVector(200f, 40f))
                                    .InEditorNamedRail("360 Environment", 18f),
                            }
                                .AsLayout()
                                .EnabledWithState(secondaryTab, 0)
                                .AsFlexGroup(
                                    FlexDirection.Column,
                                    gap: 5f,
                                    padding: new YogaFrame(24f, 80f)
                                )
                                .AsFlexItem(size: 100.pct),
                            new LayoutChildren // Contributors
                            { }
                                .AsLayout()
                                .EnabledWithState(secondaryTab, 1)
                                .AsFlexGroup(gap: 5f, padding: 24f)
                                .AsFlexItem(),
                        }
                            .As<EditorBackground>(x =>
                            {
                                x.ColorSO = _colorCollector.GetColor("Navbar/Background/Normal");
                                x.UseScriptableObjectColors = true;
                                x.Source = "#Background8px";
                                x.ImageType = UnityEngine.UI.Image.Type.Sliced;
                            })
                            .AsFlexItem(minSize: new() { x = 800f, y = 800f }),
                    }
                        .AsLayout()
                        .AsFlexGroup(
                            gap: 10f,
                            direction: FlexDirection.Column,
                            alignItems: Align.Center
                        )
                        .AsFlexItem(),
                }
                    .AsLayout()
                    .AsFlexGroup(
                        gap: 250f,
                        direction: FlexDirection.Row,
                        alignItems: Align.FlexStart
                    )
                    .AsFlexItem()
                    .EnabledWithState(MainTab, 0),
            }
                .AsLayout()
                .AsFlexGroup(
                    gap: 60f,
                    padding: 10f,
                    direction: FlexDirection.Column,
                    alignItems: Align.Center
                )
                .WithReactiveContainer(_reactiveContainer)
                .Use(controller.transform);
        }
    }
}
