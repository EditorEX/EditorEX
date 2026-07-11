using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using EditorEX.SDK.ReactiveComponents.SegmentedControl;
using EditorEX.UI.Components;
using EditorEX.Util;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using SiraUtil.Affinity;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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

        private GameObject? _songInfoRoot;
        private GameObject? _beatmapsRoot;

        private StringInputFieldValidator? _levelAuthorInputValidator;
        private EditorTextDropdown<string>? _environmentDropdown;
        private EditorTextDropdown<string>? _allDirectionsEnvironmentDropdown;
        private EditorTextDropdown<string>? _customPlatformDropdown;

        private State<bool> V4Level = StateUtils.Remember(false);
        private State<int> MainTab = StateUtils.Remember(0);

        private SharedModal<CharacteristicSettingsModal>? _charModal;
        private EditorLabelButton? _charSettingsButton;

        private EditBeatmapLevelPatches(
            BeatmapLevelDataModel beatmapLevelDataModel,
            LevelCustomDataModel levelCustomDataModel,
            IInstantiator instantiator,
            AddressableSignalBus addressableSignalBus,
            LazyInject<BeatmapProjectManager> beatmapProjectManager,
            ColorCollector colorCollector,
            EnvironmentsListModel environmentsListModel,
            CustomPlatformsListModel customPlatformsListModel,
            ReactiveContainer reactiveContainer
        )
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
        [AffinityPatch(
            typeof(EditBeatmapLevelViewController),
            nameof(EditBeatmapLevelViewController.RefreshData)
        )]
        private void RefreshData(EditBeatmapLevelViewController __instance, bool clearModifiedState)
        {
            if (!__instance._beatmapLevelDataModel.isLoaded || !__instance._audioDataModel.isLoaded)
            {
                return;
            }

            V4Level.Value = LevelContext.Version.Major >= 4;

            if (!V4Level)
            {
                _levelAuthorInputValidator!.SetValueWithoutNotify(
                    _levelCustomDataModel.LevelAuthorName,
                    clearModifiedState
                );
                if (_levelCustomDataModel.EnvironmentName != null)
                    _environmentDropdown!.Select(_levelCustomDataModel.EnvironmentName);
                if (_levelCustomDataModel.AllDirectionsEnvironmentName != null)
                    _allDirectionsEnvironmentDropdown!.Select(
                        _levelCustomDataModel.AllDirectionsEnvironmentName
                    );
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

            //ReloadContributors();
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(EditBeatmapLevelViewController),
            nameof(EditBeatmapLevelViewController.HandleBeatmapProjectSaved)
        )]
        private void HandleBeatmapProjectSaved(EditBeatmapLevelViewController __instance)
        {
            _levelAuthorInputValidator!.ClearDirtyState();
            //_environmentDropdown.ClearDirtyState();
            //_allDirectionsEnvironmentDropdown.ClearDirtyState();
            //_customPlatformDropdown.ClearDirtyState();

            //ReloadContributors();
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(EditBeatmapLevelViewController),
            nameof(EditBeatmapLevelViewController.DidActivate)
        )]
        private void ModifyUI(EditBeatmapLevelViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                AddCustomCharacteristicTab(__instance, "Lawless");
                AddCustomCharacteristicTab(__instance, "Lightshow");

                var difficultyBeatmapSetContainer = __instance
                    .transform.Find("DifficultyBeatmapSetContainer")
                    .gameObject;
                _beatmapsRoot = difficultyBeatmapSetContainer;
                _beatmapsRoot.EnabledWithState(MainTab, 1);
                var infoContainer = __instance.transform.Find("BeatmapInfoContainer");
                infoContainer.gameObject.SetActive(false);

                var secondaryTab = StateUtils.Remember(0);

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
                        OnClick = () => ShowCharacteristicModal(),
                    }
                        .Bind(ref _charSettingsButton)
                        .EnabledWithState(MainTab, 1)
                        .AsFlexItem(size: new YogaVector(240f, 30f)),
                    new LayoutChildren
                    {
                        new LayoutChildren
                        {
                            new LayoutChildren
                            {
                                new EditorImage
                                {
                                    Source = __instance._beatmapLevelDataModel.coverImageFilePath,
                                }.AsFlexItem(size: 200f),
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
                                        __instance._songNameInputValidator,
                                        ref __instance._songNameInputValidator
                                    )
                                    .InEditorNamedRail("Song Name", 18f, 55f)
                                    .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                    .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                                new EditorStringInput()
                                    .WithInputValidatorCopy<StringInputFieldValidator, string>(
                                        __instance._songSubNameInputValidator,
                                        ref __instance._songSubNameInputValidator
                                    )
                                    .InEditorNamedRail("Song Sub Name", 18f, 55f)
                                    .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                    .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                                new EditorStringInput()
                                    .WithInputValidatorCopy<StringInputFieldValidator, string>(
                                        __instance._songAuthorNameInputValidator,
                                        ref __instance._songAuthorNameInputValidator
                                    )
                                    .InEditorNamedRail("Song Author Name", 18f, 55f)
                                    .LinkNamedRailWithValidator<StringInputFieldValidator, string>()
                                    .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                                new EditorStringInput()
                                    .WithInputValidator<StringInputFieldValidator, string>(
                                        out _levelAuthorInputValidator,
                                        x =>
                                            __instance._signalBus.Fire(
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
                                            __instance._editBpmGridButton = x.Component.Button;
                                        }),
                                    new EditorStringInput()
                                        .WithInputValidatorCopy<FloatInputFieldValidator, float>(
                                            __instance._beatsPerMinuteInputValidator,
                                            ref __instance._beatsPerMinuteInputValidator
                                        )
                                        .InEditorNamedRail("BPM", 18f, 35f)
                                        .LinkNamedRailWithValidator<
                                            FloatInputFieldValidator,
                                            float
                                        >()
                                        .AsFlexItem(size: new() { x = 260f, y = "auto" }),
                                }
                                    .AsLayout()
                                    .AsFlexGroup(gap: 4f)
                                    .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                                new EditorStringInput()
                                    .WithInputValidatorCopy<FloatInputFieldValidator, float>(
                                        __instance._previewStartTimeInputValidator,
                                        ref __instance._previewStartTimeInputValidator
                                    )
                                    .InEditorNamedRail("Preview Start Time", 18f, 30f)
                                    .LinkNamedRailWithValidator<FloatInputFieldValidator, float>()
                                    .AsFlexItem(size: new() { x = 100f.pct, y = "auto" }),
                                new EditorStringInput()
                                    .WithInputValidatorCopy<FloatInputFieldValidator, float>(
                                        __instance._previewDurationInputValidator,
                                        ref __instance._previewDurationInputValidator
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
                                    new EditorTextDropdown<string>()
                                        .With(x =>
                                        {
                                            x.Items.AddRange(
                                                _environmentsListModel
                                                    .GetAllEnvironmentInfosWithType(
                                                        EnvironmentType.Normal
                                                    )
                                                    .Select(x =>
                                                        (x.serializedName, x.environmentName)
                                                    )
                                                    .ToDictionary(
                                                        x => x.serializedName,
                                                        x => x.environmentName
                                                    )
                                            );
                                        })
                                        .EnabledWithState(V4Level, false)
                                        .Bind(ref _environmentDropdown)
                                        .ExtractState(
                                            out var normalEnvironmentValue,
                                            _environmentDropdown!
                                                .Items.First()
                                                .ExtractTupleFromKVP()
                                        )
                                        .DropdownWithState(normalEnvironmentValue)
                                        .On(
                                            normalEnvironmentValue,
                                            (x, v) =>
                                            {
                                                Debug.Log($"Selected Normal Environment: {v}");
                                            }
                                        )
                                        .AsFlexItem(size: new YogaVector(200f, 40f))
                                        .InEditorNamedRail("Environment", 18f),
                                    new EditorTextDropdown<string>()
                                        .With(x =>
                                        {
                                            x.Items.AddRange(
                                                _environmentsListModel
                                                    .GetAllEnvironmentInfosWithType(
                                                        EnvironmentType.Circle
                                                    )
                                                    .ToDictionary(
                                                        x => x.serializedName,
                                                        x => x.environmentName
                                                    )
                                            );
                                        })
                                        .EnabledWithState(V4Level, false)
                                        .Bind(ref _allDirectionsEnvironmentDropdown)
                                        .ExtractState(
                                            out var allDirectionsEnvironmentValue,
                                            _allDirectionsEnvironmentDropdown!
                                                .Items.First()
                                                .ExtractTupleFromKVP()
                                        )
                                        .DropdownWithState(allDirectionsEnvironmentValue)
                                        .On(
                                            allDirectionsEnvironmentValue,
                                            (x, v) =>
                                            {
                                                Debug.Log(
                                                    $"Selected All Directions Environment: {v}"
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
                                    x.ColorSO = _colorCollector.GetColor(
                                        "Navbar/Background/Normal"
                                    );
                                    x.UseScriptableObjectColors = true;
                                    x.Source = "#Background8px";
                                    x.ImageType = Image.Type.Sliced;
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
                    .Use(__instance.transform);

                __instance.gameObject.SetActive(true);
            }
        }

        private void ShowCharacteristicModal()
        {
            if (_charSettingsButton == null)
                return;
            if (_charModal?.ModalOpened ?? false)
            {
                _charModal?.Modal?.Destroy();
            }

            _charModal ??= new SharedModal<CharacteristicSettingsModal>();
            _charModal.PresentEditor(_charSettingsButton.ContentTransform);

            var content = _charModal.Modal.ContentLayout;
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
                        : System.IO.Path.Combine(folder, details.IconFilename);

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
                                var picked = BeatmapEditor3D.NativeFileDialogs.OpenFileDialog(
                                    "Select Characteristic Icon",
                                    "png",
                                    folder
                                );
                                if (string.IsNullOrEmpty(picked))
                                    return;
                                // Store the bare filename when the image lives in the level
                                // folder (SongCore convention), otherwise the full path.
                                var fileName = System.IO.Path.GetFileName(picked);
                                var inFolder = System.IO.Path.Combine(folder, fileName);
                                details.IconFilename = System.IO.File.Exists(inFolder)
                                    ? fileName
                                    : picked;
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

        private static BeatmapCharacteristicSO? GetCustomCharacteristic(string serializedName)
        {
            try
            {
                foreach (var so in SongCore.Collections.customCharacteristics)
                {
                    if (so != null && so.serializedName == serializedName)
                        return so;
                }
            }
            catch
            {
                // SongCore not present / not initialised — feature degrades gracefully.
            }
            return null;
        }

        private void AddCustomCharacteristicTab(
            EditBeatmapLevelViewController controller,
            string serializedName
        )
        {
            var so = GetCustomCharacteristic(serializedName);
            if (so == null)
                return;

            var existing = controller._difficultyBeatmapSetTabButtons;
            if (existing == null || existing.Length == 0)
                return;
            if (existing.Any(b => b != null && b.beatmapCharacteristic == so))
                return;

            var template = existing[0];
            // Instantiate through Zenject (NOT Object.Instantiate) so the button's
            // [Inject] dependencies are populated on the clone. The tab's
            // SelectableStateController._tweeningManager is injected; without injection it
            // is null and the hover state transition (ColorGraphicStateTransition.StartTween)
            // throws an NRE.
            var clone = _instantiator.InstantiatePrefab(
                template.gameObject,
                template.transform.parent
            );
            clone.name = "DifficultyBeatmapSetTabButton_" + serializedName;
            var cloneButton = clone.GetComponent<DifficultyBeatmapSetTabButton>();
            if (cloneButton == null)
            {
                UnityEngine.Object.Destroy(clone);
                return;
            }
            cloneButton._beatmapCharacteristic = so;

            var label = clone.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (label != null)
                label.text = serializedName;

            var list = existing.ToList();
            list.Add(cloneButton);
            controller._difficultyBeatmapSetTabButtons = list.ToArray();
        }
    }
}
