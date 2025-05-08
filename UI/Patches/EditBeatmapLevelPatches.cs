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
using EditorEX.SDK.Reactive;
using EditorEX.SDK.Reactive.Components;
using EditorEX.Util;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.Components;
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
        private readonly TextSegmentedControlFactory _textSegmentedControlFactory;
        private readonly DropdownFactory _dropdownFactory;
        private readonly ButtonFactory _buttonFactory;
        private readonly ImageFactory _imageFactory;
        private readonly ClickableImageFactory _clickableImageFactory;
        private readonly StringInputFactory _stringInputFactory;
        private readonly ScrollViewFactory _scrollViewFactory;
        private readonly LevelCustomDataModel _levelCustomDataModel;
        private readonly IInstantiator _instantiator;
        private readonly AddressableSignalBus _addressableSignalBus;
        private readonly LazyInject<BeatmapProjectManager> _beatmapProjectManager;
        private readonly ColorCollector _colorCollector;
        private readonly EnvironmentsListModel _environmentsListModel;
        private readonly CustomPlatformsListModel _customPlatformsListModel;
        private readonly ReactiveContainer _reactiveContainer;

        private TextSegmentedControl _rootSegmentedControl;
        private TabbingSegmentedControlController _rootTabbingSegmentedControlController;

        private GameObject _songInfoRoot;
        private GameObject _beatmapsRoot;

        private StringInputFieldValidator _levelAuthorInputValidator;
        private SimpleTextEditorDropdownView _environmentDropdown;
        private SimpleTextEditorDropdownView _allDirectionsEnvironmentDropdown;
        private SimpleTextEditorDropdownView _customPlatformDropdown;

        private TextSegmentedControl _extraSongInfoSegmentedControl;
        private TabbingSegmentedControlController _extraSongInfoTabbingSegmentedControlController;

        private GameObject _environmentRoot;

        private GameObject _contributorsRoot;
        private ScrollView _scrollView;

        private EditBeatmapLevelPatches(
            BeatmapLevelDataModel beatmapLevelDataModel,
            TextSegmentedControlFactory textSegmentedControlFactory,
            DropdownFactory dropdownFactory,
            ButtonFactory buttonFactory,
            ImageFactory imageFactory,
            ClickableImageFactory clickableImageFactory,
            StringInputFactory stringInputFactory,
            ScrollViewFactory scrollViewFactory,
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
            _textSegmentedControlFactory = textSegmentedControlFactory;
            _dropdownFactory = dropdownFactory;
            _buttonFactory = buttonFactory;
            _imageFactory = imageFactory;
            _clickableImageFactory = clickableImageFactory;
            _stringInputFactory = stringInputFactory;
            _scrollViewFactory = scrollViewFactory;
            _levelCustomDataModel = levelCustomDataModel;
            _instantiator = instantiator;
            _addressableSignalBus = addressableSignalBus;
            _beatmapProjectManager = beatmapProjectManager;
            _colorCollector = colorCollector;
            _environmentsListModel = environmentsListModel;
            _customPlatformsListModel = customPlatformsListModel;
            _reactiveContainer = reactiveContainer;
        }

        private void MakeContributorCell(ContributorData contributorData)
        {
            var root = new GameObject($"Contributor {contributorData.Name}");
            root.transform.SetParent(_scrollView.contentTransform, false);
            var horizontal = root.AddComponent<HorizontalLayoutGroup>();

            horizontal.childForceExpandWidth = false;
            horizontal.spacing = 15f;

            var layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.minHeight = 160f;

            string input = Path.Combine(_beatmapProjectManager.Value._workingBeatmapProject, contributorData.IconPath);

            EditorClickableImage image = null;
            image = _clickableImageFactory.Create(root.transform, input, new LayoutData(null, null), _ =>
            {
                string text = NativeFileDialogs.OpenFileDialog("",
                [
                    new("", "png", "jpg", "jpeg")
                ], null);

                if (string.IsNullOrEmpty(text) || text == contributorData.IconPath)
                {
                    return;
                }

                BeatmapProjectFileHelper.SaveBeatmapAssetFile(
                    Path.Combine(_beatmapProjectManager.Value._workingBeatmapProject, contributorData.IconPath),
                    _beatmapProjectManager.Value._workingBeatmapProject,
                    text);

                contributorData.IconPath = Path.GetFileName(text);

                _levelCustomDataModel.UpdateWith(null, null, null, null, null, null, null, _levelCustomDataModel.Contributors);

                image.SetImageAsync(text, false);
            });
            var imageLayoutElement = image.gameObject.AddComponent<LayoutElement>();
            imageLayoutElement.minWidth = 160f;
            imageLayoutElement.minHeight = 160f;
            imageLayoutElement.preferredWidth = 160f;
            imageLayoutElement.preferredHeight = 160f;

            _addressableSignalBus.Subscribe<Material>("rounded-corners", null, x =>
            {
                image.material = x.GetValue();
            });

            var vertical = new GameObject($"Vertical");
            vertical.transform.SetParent(root.transform, false);
            vertical.AddComponent<VerticalLayoutGroup>();

            _stringInputFactory.Create(vertical.transform, "Name", 450f, x =>
            {
                contributorData.Name = x;
                _levelCustomDataModel.UpdateWith(null, null, null, null, null, null, null, _levelCustomDataModel.Contributors);
            }).SetTextWithoutNotify(contributorData.Name);

            _stringInputFactory.Create(vertical.transform, "Role", 450f, x =>
            {
                contributorData.Role = x;
                _levelCustomDataModel.UpdateWith(null, null, null, null, null, null, null, _levelCustomDataModel.Contributors);
            }).SetTextWithoutNotify(contributorData.Role);
        }

        private void ReloadContributors()
        {
            foreach (Transform child in _scrollView.contentTransform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (var contributor in _levelCustomDataModel.Contributors)
            {
                MakeContributorCell(contributor);
            }

            var newContributor = new GameObject("New Contributor");
            newContributor.transform.SetParent(_scrollView.contentTransform, false);
            var horizontal = newContributor.AddComponent<HorizontalLayoutGroup>();

            var button = _buttonFactory.Create(horizontal.transform, "New Contributor", () =>
            {
                _levelCustomDataModel.Contributors.Add(new ContributorData("New Contributor", "", "Contributor Role"));
                _levelCustomDataModel.UpdateWith(null, null, null, null, null, null, null, _levelCustomDataModel.Contributors);

                ReloadContributors();
            });
            button.transform.GetChild(0).GetComponent<ImageView>()._colorSo = _colorCollector.GetColor("BpmRegion/Handle");
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.RefreshData))]
        private void RefreshData(EditBeatmapLevelViewController __instance, bool clearModifiedState)
        {
            if (!__instance._beatmapLevelDataModel.isLoaded || !__instance._audioDataModel.isLoaded)
            {
                return;
            }
            _levelAuthorInputValidator.transform.parent.gameObject.SetActive(LevelContext.Version.Major < 4);
            //_environmentDropdown.transform.parent.gameObject.SetActive(LevelContext.Version.Major < 4);
            //_allDirectionsEnvironmentDropdown.transform.parent.gameObject.SetActive(LevelContext.Version.Major < 4);

            _songInfoRoot.SetActive(false);
            _songInfoRoot.SetActive(true); // Fix layout

            if (LevelContext.Version.Major < 4)
            {
                _levelAuthorInputValidator.SetValueWithoutNotify(_levelCustomDataModel.LevelAuthorName, clearModifiedState);
                //_environmentDropdown.SelectCellWithIdx(_environmentsListModel.GetAllEnvironmentInfosWithType(EnvironmentType.Normal).Select(x => x.serializedName).ToList().IndexOf(_levelCustomDataModel.EnvironmentName), clearModifiedState);
                //_allDirectionsEnvironmentDropdown.SelectCellWithIdx(_environmentsListModel.GetAllEnvironmentInfosWithType(EnvironmentType.Circle).Select(x => x.serializedName).ToList().IndexOf(_levelCustomDataModel.AllDirectionsEnvironmentName), clearModifiedState);
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

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.DidActivate))]
        private void ModifyUI(EditBeatmapLevelViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                _rootSegmentedControl = _textSegmentedControlFactory.Create(__instance.transform, new string[] { "Song Info | 1", "Beatmaps | 2" }, RootSelected);
                (_rootSegmentedControl.transform as RectTransform).anchoredPosition = new Vector2(0f, 500f);
                _rootTabbingSegmentedControlController = _rootSegmentedControl.gameObject.AddComponent<TabbingSegmentedControlController>();
                _rootTabbingSegmentedControlController.Setup(_rootSegmentedControl, false);

                __instance.gameObject.SetActive(false);

                _songInfoRoot = new GameObject("SongInfoRoot");
                _songInfoRoot.transform.SetParent(__instance.transform, false);
                _songInfoRoot.transform.localPosition = new Vector3(-500f, 430f, 0f);

                var difficultyBeatmapSetContainer = __instance.transform.Find("DifficultyBeatmapSetContainer").gameObject;
                difficultyBeatmapSetContainer.SetActive(false);
                _beatmapsRoot = difficultyBeatmapSetContainer;

                var beatmapInfoContainer = __instance.transform.Find("BeatmapInfoContainer");

                beatmapInfoContainer.SetParent(_songInfoRoot.transform, false);

                var infoContainerVerticalLayoutGroup = beatmapInfoContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                infoContainerVerticalLayoutGroup.spacing = 50f;
                infoContainerVerticalLayoutGroup.childForceExpandWidth = true;
                infoContainerVerticalLayoutGroup.childForceExpandHeight = false;
                infoContainerVerticalLayoutGroup.childControlWidth = false;
                infoContainerVerticalLayoutGroup.childControlHeight = false;

                var infoContainerSizeFitter = beatmapInfoContainer.gameObject.AddComponent<ContentSizeFitter>();
                infoContainerSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                infoContainerSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var songImageInfo = beatmapInfoContainer.Find("SongImageInfo");
                var songImageInfoHorizontalLayoutGroup = songImageInfo.gameObject.AddComponent<HorizontalLayoutGroup>();
                songImageInfoHorizontalLayoutGroup.spacing = 25f;
                songImageInfoHorizontalLayoutGroup.childForceExpandWidth = true;
                songImageInfoHorizontalLayoutGroup.childForceExpandHeight = false;
                songImageInfoHorizontalLayoutGroup.childControlWidth = false;
                songImageInfoHorizontalLayoutGroup.childControlHeight = false;

                var songImageInfoInputs = songImageInfo.Find("Inputs");
                songImageInfoInputs.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
                (songImageInfoInputs.transform as RectTransform).sizeDelta = new Vector2(380f, 0f);
                (songImageInfoInputs.Find("SongWrapper").transform as RectTransform).sizeDelta = new Vector2(380f, 70f);
                (songImageInfoInputs.Find("CoverImageWrapper").transform as RectTransform).sizeDelta = new Vector2(380f, 70f);
                (songImageInfo.Find("CoverImage").transform as RectTransform).sizeDelta = new Vector2(160f, 160f);

                var songImageInfoSizeFitter = songImageInfo.gameObject.AddComponent<ContentSizeFitter>();
                songImageInfoSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                songImageInfoSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var songInfo = beatmapInfoContainer.Find("SongInfo");

                var songInfoSizeFitter = songInfo.gameObject.AddComponent<ContentSizeFitter>();
                songInfoSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                songInfoSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                _levelAuthorInputValidator = GameObject.Instantiate(__instance._songAuthorNameInputValidator.transform.parent, songInfo.transform).GetComponentInChildren<StringInputFieldValidator>();

                _levelAuthorInputValidator.onInputValidated += x =>
                {
                    __instance._signalBus.Fire(new BeatmapDataModelSignals.UpdateBeatmapDataSignal(null, null, null, x));
                };

                _levelAuthorInputValidator.transform.parent.Find("Label").GetComponent<TextMeshProUGUI>().text = "Level Author Name";

                var timingInfo = beatmapInfoContainer.Find("TimingInfo");

                var timingInfoSizeFitter = timingInfo.gameObject.AddComponent<ContentSizeFitter>();
                timingInfoSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                timingInfoSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var previewWaveform = new GameObject("PreviewWaveform");
                previewWaveform.transform.SetParent(beatmapInfoContainer);

                var tab = ValueUtils.Remember(0);

                EditorImage? image1 = null;
                EditorImage? image2 = null;

                YogaVector x;
                EditorImage? image3 = null;
                new Layout
                {
                    Children = {
                        new EditorImage {
                            Source = "https://picsum.photos/200",
                        }.AsFlexItem(aspectRatio: 1f)
                        .Bind(ref image1)
                        .Animate(tab, () => image1.Enabled = tab == 0),

                        new EditorImage {
                            Source = "https://picsum.photos/200"
                        }.AsFlexItem(aspectRatio: 1f)
                        .Bind(ref image2)
                        .Animate(tab, () => image2.Enabled = tab == 1),

                        new EditorImage {
                            Source = "https://picsum.photos/200"
                        }.AsFlexItem(aspectRatio: 1f)
                        .Bind(ref image3)
                        .Animate(tab, () => image3.Enabled = tab == 2),

                        new EditorLabelButton {
                            Text = "Pic 1 SKIBIDI TOILET!!!!!!!!!!!!!",
                            FontSize = 20f,
                            OnClick = () => {
                                tab.Value = 0;
                            }
                        }.AsFlexItem(size: "fit-content"),

                        new EditorBackground {
                            ColorSO = _colorCollector.GetColor("Navbar/Background/Normal"),
                            UseScriptableObjectColors = true,
                            Source = "#Background8px",
                            ImageType = Image.Type.Sliced,
                            Children = {
                                new Layout {
                                    Children = {
                                        new EditorLabelButton {
                                            Text = "Pic 1 SKIBIDI TOILET!!!!!!!!!!!!!",
                                            FontSize = 20f,
                                            OnClick = () => {
                                                tab.Value = 0;
                                            }
                                        }.AsFlexItem(size: "auto"),
                                        new EditorLabelButton {
                                            Text = "Pic 2",
                                            FontSize = 20f,
                                            OnClick = () => {
                                                tab.Value = 1;
                                            }
                                        }.AsFlexItem(size: "auto"),
                                        new EditorLabelButton {
                                            Text = "Pic 3",
                                            FontSize = 20f,
                                            OnClick = () => {
                                                tab.Value = 2;
                                            }
                                        }.AsFlexItem(size: "auto")
                                    }
                                }.AsFlexGroup(gap: 5f).AsFlexItem(),
                            }
                        }.AsFlexItem(minSize: new () {x = 800f, y = 800f})
                    }
                }.AsFlexGroup(gap: 10f, direction: Reactive.Yoga.FlexDirection.Column)
                .AsRectItem()
                .WithReactiveContainer(_reactiveContainer)
                .Use(_songInfoRoot);

                __instance.gameObject.SetActive(true);
            }
            else
            {
                _rootTabbingSegmentedControlController.AddBindings(false);
                //_extraSongInfoTabbingSegmentedControlController.AddBindings(true);
            }

            _rootTabbingSegmentedControlController.ClickCell(0);
            //_extraSongInfoTabbingSegmentedControlController.ClickCell(0);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.DidDeactivate))]
        private void DidDeactivate()
        {
            _rootTabbingSegmentedControlController.ClearBindings();
            //_extraSongInfoTabbingSegmentedControlController.ClearBindings();
        }

        private void RootSelected(SegmentedControl segmentedControl, int idx)
        {
            _songInfoRoot.SetActive(idx == 0);
            _beatmapsRoot.SetActive(idx == 1);

            if (idx == 1)
            {
                _extraSongInfoTabbingSegmentedControlController.ClearBindings();
            }
            else
            {
                _extraSongInfoTabbingSegmentedControlController.AddBindings(true);
            }
        }

        private void ExtraSongInfoSelected(SegmentedControl segmentedControl, int idx)
        {
            _environmentRoot.SetActive(idx == 0);
            _contributorsRoot.SetActive(idx == 1);
        }
    }
}
