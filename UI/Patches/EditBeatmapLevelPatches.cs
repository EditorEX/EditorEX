using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatSaberMarkupLanguage;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Base;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using EditorEX.SDK.Factories;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using EditorEX.SDK.ReactiveComponents.SegmentedControl;
using EditorEX.Util;
using HMUI;
using Reactive;
using Reactive.Yoga;
using SiraUtil.Affinity;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

//:dread:
//TODO: Clean this up dramatically... but how?
namespace EditorEX.UI.Patches
{
    internal class EditBeatmapLevelPatches : IAffinity
    {
        private readonly BeatmapLevelDataModel _beatmapLevelDataModel;
        private readonly LevelCustomDataModel _levelCustomDataModel;
        private readonly IInstantiator _instantiator;
        private readonly AddressableSignalBus _addressableSignalBus;
        private readonly LazyInject<BeatmapProjectManager> _beatmapProjectManager;
        private readonly ColorCollector _colorCollector;
        private readonly EnvironmentsListModel _environmentsListModel;
        private readonly CustomPlatformsListModel _customPlatformsListModel;
        private readonly ReactiveContainer _reactiveContainer;

        private GameObject _songInfoRoot;
        private GameObject _beatmapsRoot;

        private StringInputFieldValidator _levelAuthorInputValidator;
        private EditorTextDropdown<string> _environmentDropdown;
        private EditorTextDropdown<string> _allDirectionsEnvironmentDropdown;
        private EditorTextDropdown<string> _customPlatformDropdown;

        private ObservableValue<bool> V4Level = ValueUtils.Remember(false);
        private ObservableValue<int> MainTab = ValueUtils.Remember(0);

        private EditBeatmapLevelPatches(
            BeatmapLevelDataModel beatmapLevelDataModel,
            LevelCustomDataModel levelCustomDataModel,
            IInstantiator instantiator,
            AddressableSignalBus addressableSignalBus,
            LazyInject<BeatmapProjectManager> beatmapProjectManager,
            ColorCollector colorCollector,
            EnvironmentsListModel environmentsListModel,
            CustomPlatformsListModel customPlatformsListModel,
            ReactiveContainer reactiveContainer)
        {
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _levelCustomDataModel = levelCustomDataModel;
            _instantiator = instantiator;
            _addressableSignalBus = addressableSignalBus;
            _beatmapProjectManager = beatmapProjectManager;
            _colorCollector = colorCollector;
            _environmentsListModel = environmentsListModel;
            _customPlatformsListModel = customPlatformsListModel;
            _reactiveContainer = reactiveContainer;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.RefreshData))]
        private void RefreshData(EditBeatmapLevelViewController __instance, bool clearModifiedState)
        {
            if (!__instance._beatmapLevelDataModel.isLoaded || !__instance._audioDataModel.isLoaded)
            {
                return;
            }
            V4Level.Value = LevelContext.Version.Major >= 4;

            if (!V4Level)
            {
                _levelAuthorInputValidator.SetValueWithoutNotify(_levelCustomDataModel.LevelAuthorName, clearModifiedState);
                _environmentDropdown.Select(_levelCustomDataModel.EnvironmentName);
                _allDirectionsEnvironmentDropdown.Select(_levelCustomDataModel.AllDirectionsEnvironmentName);
            }

            var customPlatIndex = _customPlatformsListModel.CustomPlatforms.Select(x => x.FilePath).ToList().IndexOf(_levelCustomDataModel.CustomPlatformInfo.FilePath);
            customPlatIndex = customPlatIndex == -1 ? _customPlatformsListModel.CustomPlatforms.Select(x => x.Hash).ToList().IndexOf(_levelCustomDataModel.CustomPlatformInfo.Hash) : customPlatIndex;

            if (customPlatIndex == -1)
            {
                //_customPlatformDropdown._text.text = _levelCustomDataModel.CustomPlatformInfo.FilePath;
            }
            else
            {
                //_customPlatformDropdown.SelectCellWithIdx(customPlatIndex);
            }

            //ReloadContributors();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.HandleBeatmapProjectSaved))]
        private void HandleBeatmapProjectSaved(EditBeatmapLevelViewController __instance)
        {
            _levelAuthorInputValidator.ClearDirtyState();
            //_environmentDropdown.ClearDirtyState();
            //_allDirectionsEnvironmentDropdown.ClearDirtyState();
            //_customPlatformDropdown.ClearDirtyState();

            //ReloadContributors();
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.DidActivate))]
        private void ModifyUI(EditBeatmapLevelViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                var difficultyBeatmapSetContainer = __instance.transform.Find("DifficultyBeatmapSetContainer").gameObject;
                _beatmapsRoot = difficultyBeatmapSetContainer;
                _beatmapsRoot.EnabledWithObservable(MainTab, 1);
                __instance.transform.Find("BeatmapInfoContainer").gameObject.SetActive(false);

                var secondaryTab = ValueUtils.Remember(0);

                new Layout
                {
                    Children = {
                        new EditorSegmentedControl {
                            SelectedIndex = MainTab,
                            Values = [ "Song Info", "Beatmaps" ],
                            TabbingType = TabbingType.Alpha,
                        }.AsFlexItem(size: new YogaVector(300f, 30f)),
                        new Layout {
                            Children = {
                                new Layout {
                                    Children = {
                                        new Layout() {
                                            Children = {
                                                new EditorImage() {
                                                    Source = "https://picsum.photos/200"
                                                }.AsFlexItem(size: new () {x = 160f, y = 160f}),
                                            }
                                        }
                                        .AsFlexGroup(gap: 40f, direction: FlexDirection.Column, alignItems: Align.FlexStart)
                                        .AsFlexItem(),

                                        new Layout()
                                        {
                                            Children =
                                            {
                                                new EditorStringInput()
                                                    .WithInputValidatorCopy<StringInputFieldValidator, string>(__instance._songNameInputValidator, ref __instance._songNameInputValidator)
                                                    .InEditorNamedRail("Song Name", 18f, 55f)
                                                    .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                                    .AsFlexItem(size: new() { x = 100f.pct(), y = "auto"
                                                    }),

                                                new EditorStringInput()
                                                    .WithInputValidatorCopy<StringInputFieldValidator, string>(__instance._songSubNameInputValidator, ref __instance._songSubNameInputValidator)
                                                    .InEditorNamedRail("Song Sub Name", 18f, 55f)
                                                    .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                                    .AsFlexItem(size: new() { x = 100f.pct(), y = "auto"
                                                    }),

                                                new EditorStringInput()
                                                    .WithInputValidatorCopy<StringInputFieldValidator, string>(__instance._songAuthorNameInputValidator, ref __instance._songAuthorNameInputValidator)
                                                    .InEditorNamedRail("Song Author Name", 18f, 55f)
                                                    .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                                    .AsFlexItem(size: new() { x = 100f.pct(), y = "auto"
                                                    }),

                                                new EditorStringInput()
                                                    .WithInputValidator<StringInputFieldValidator, string>(out _levelAuthorInputValidator, x => __instance._signalBus.Fire(new BeatmapDataModelSignals.UpdateBeatmapDataSignal(null, null, null, x)))
                                                    .InEditorNamedRail("Level Author Name", 18f, 55f)
                                                    .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                                    .EnabledWithObservable(V4Level, false)
                                                    .AsFlexItem(size: new() { x = 100f.pct(), y = "auto"
                                                    }),
                                            }
                                        }
                                        .AsFlexGroup(gap: 40f, direction: FlexDirection.Column, alignItems: Align.FlexStart)
                                        .AsFlexItem(),
                                        
                                        new Layout // Audio Info
                                        {
                                            Children =
                                            {
                                                new Layout
                                                {
                                                    Children = {
                                                        new EditorBackgroundButton
                                                        {
                                                            Source = "#Background8px",
                                                            Children = {
                                                                new EditorImage
                                                                {
                                                                    Source = "#IconOpen"
                                                                }.AsFlexItem(size: new YogaVector(30f, 30f))
                                                            }
                                                        }.AsFlexGroup(justifyContent: Justify.Center, alignItems: Align.Center)
                                                        .AsFlexItem(size: new YogaVector(40f, 40f))
                                                        .With(x => {
                                                            __instance._editBpmGridButton = x.Component.Button;
                                                        }),
                                                        new EditorStringInput()
                                                            .WithInputValidatorCopy<FloatInputFieldValidator, float>(__instance._beatsPerMinuteInputValidator, ref __instance._beatsPerMinuteInputValidator)
                                                            .InEditorNamedRail("BPM", 18f, 35f)
                                                            .LinkNamedRailWithValidator<FloatInputFieldValidator, float>()
                                                            .AsFlexItem(size: new() { x = 260f, y = "auto"}),
                                                    }
                                                }.AsFlexGroup(gap: 4f)
                                                .AsFlexItem(size: new() { x = 100f.pct(), y = "auto"}),

                                                new EditorStringInput()
                                                    .WithInputValidatorCopy<FloatInputFieldValidator, float>(__instance._previewStartTimeInputValidator, ref __instance._previewStartTimeInputValidator)
                                                    .InEditorNamedRail("Preview Start Time", 18f, 30f)
                                                    .LinkNamedRailWithValidator<FloatInputFieldValidator, float>()
                                                    .AsFlexItem(size: new() { x = 100f.pct(), y = "auto"}),

                                                new EditorStringInput()
                                                    .WithInputValidatorCopy<FloatInputFieldValidator, float>(__instance._previewDurationInputValidator, ref __instance._previewDurationInputValidator)
                                                    .InEditorNamedRail("Preview Duration", 18f, 30f)
                                                    .LinkNamedRailWithValidator<FloatInputFieldValidator, float>()
                                                    .AsFlexItem(size: new() { x = 100f.pct(), y = "auto"}),
                                            }
                                        }
                                        .AsFlexGroup(gap: 40f, direction: FlexDirection.Column, alignItems: Align.FlexStart)
                                        .AsFlexItem(size: new () {x = 60f.pct(), y = "auto"}),
                                    }
                                }.AsFlexGroup(gap: 60f, direction: FlexDirection.Column, alignItems: Align.FlexStart, padding: new YogaFrame(40f, 0f, 0f, 0f))
                                .AsFlexItem(size: new () {x = 500f, y = "auto"}),
                                new Layout // Right Side
                                {
                                    Children = {
                                        new EditorSegmentedControl {
                                            SelectedIndex = secondaryTab,
                                            Values = [ "Environments", "Contributors" ],
                                            TabbingType = TabbingType.Qwerty,
                                        }.AsFlexItem(size: new YogaVector(250f, 30f)),
                                        new EditorBackground {
                                            ColorSO = _colorCollector.GetColor("Navbar/Background/Normal"),
                                            UseScriptableObjectColors = true,
                                            Source = "#Background8px",
                                            ImageType = Image.Type.Sliced,
                                            Children = {
                                                new Layout { // Environments
                                                    Children = {
                                                        new EditorTextDropdown<string>()
                                                        .With(x => {
                                                            x.Items.AddRange(
                                                                _environmentsListModel.GetAllEnvironmentInfosWithType(EnvironmentType.Normal)
                                                                .Select(x => (x.serializedName, x.environmentName))
                                                                .ToDictionary(x => x.serializedName, x => x.environmentName));
                                                        })
                                                        .EnabledWithObservable(V4Level, false)
                                                        .Bind(ref _environmentDropdown)
                                                        .ExtractObservable(out var normalEnvironmentValue, _environmentDropdown.Items.First().ExtractTupleFromKVP())
                                                        .DropdownWithObservable(normalEnvironmentValue)
                                                        .Animate(normalEnvironmentValue, (x, v) => {
                                                            Debug.Log($"Selected Normal Environment: {v}");
                                                        })
                                                        .AsFlexItem(size: new YogaVector(200f, 40f))
                                                        .InEditorNamedRail("Environment", 18f),

                                                        new EditorTextDropdown<string>()
                                                        .With(x => {
                                                            x.Items.AddRange(
                                                                _environmentsListModel.GetAllEnvironmentInfosWithType(EnvironmentType.Circle)
                                                                .ToDictionary(x => x.serializedName, x => x.environmentName));
                                                        })
                                                        .EnabledWithObservable(V4Level, false)
                                                        .Bind(ref _allDirectionsEnvironmentDropdown)
                                                        .ExtractObservable(out var allDirectionsEnvironmentValue, _allDirectionsEnvironmentDropdown.Items.First().ExtractTupleFromKVP())
                                                        .DropdownWithObservable(allDirectionsEnvironmentValue)
                                                        .Animate(allDirectionsEnvironmentValue, (x, v) => {
                                                            Debug.Log($"Selected All Directions Environment: {v}");
                                                        })
                                                        .AsFlexItem(size: new YogaVector(200f, 40f))
                                                        .InEditorNamedRail("360 Environment", 18f),
                                                    }
                                                }.EnabledWithObservable(secondaryTab, 0)
                                                .AsFlexGroup(FlexDirection.Column, gap: 5f, padding: new YogaFrame(24f, 80f))
                                                .AsFlexItem(size: 100.pct()),

                                                new Layout { // Contributors
                                                    Children = {
                                                    }
                                                }.EnabledWithObservable(secondaryTab, 1)
                                                .AsFlexGroup(gap: 5f, padding: 24f)
                                                .AsFlexItem(),
                                            }
                                        }.AsFlexItem(minSize: new () {x = 800f, y = 800f})
                                    }
                                }.AsFlexGroup(gap: 10f, direction: FlexDirection.Column, alignItems: Align.Center)
                                .AsFlexItem()
                            }
                        }.AsFlexGroup(gap: 250f, direction: FlexDirection.Row, alignItems: Align.FlexStart)
                        .AsFlexItem()
                        .EnabledWithObservable(MainTab, 0)
                    }
                }.AsFlexGroup(gap: 60f, padding: 10f, direction: FlexDirection.Column, alignItems: Align.Center)
                .WithReactiveContainer(_reactiveContainer)
                .Use(__instance.transform);

                __instance.gameObject.SetActive(true);
            }
        }
    }
}
