using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.Components;
using EditorEX.SDK.Factories;
using HMUI;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

// :dread:
namespace EditorEX.UI.Patches
{
    internal class EditBeatmapLevelPatches : IAffinity
    {
        private readonly BeatmapLevelDataModel _beatmapLevelDataModel;
        private readonly TextSegmentedControlFactory _textSegmentedControlFactory;
        private readonly ImageFactory _imageFactory;
        private readonly LevelCustomDataModel _levelCustomDataModel;
        private readonly IInstantiator _instantiator;

        private TextSegmentedControl _rootSegmentedControl;
        private TabbingSegmentedControlController _rootTabbingSegmentedControlController;

        private GameObject SongInfoRoot;
        private GameObject BeatmapsRoot;

        private StringInputFieldValidator _levelAuthorInputValidator;
        private StaticAudioWaveformView _staticAudioWaveformView;



        private TextSegmentedControl _extraSongInfoSegmentedControl;
        private TabbingSegmentedControlController _extraSongInfoTabbingSegmentedControlController;

        private GameObject EnvironmentRoot;
        private GameObject ContributorsRoot;
        private GameObject ModhcartRoot;

        private EditBeatmapLevelPatches(
            BeatmapLevelDataModel beatmapLevelDataModel,
            TextSegmentedControlFactory textSegmentedControlFactory,
            ImageFactory imageFactory,
            LevelCustomDataModel levelCustomDataModel,
            IInstantiator instantiator)
        {
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _textSegmentedControlFactory = textSegmentedControlFactory;
            _imageFactory = imageFactory;
            _levelCustomDataModel = levelCustomDataModel;
            _instantiator = instantiator;
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

            SongInfoRoot.SetActive(false);
            SongInfoRoot.SetActive(true); // Fix layout

            _levelAuthorInputValidator.SetValueWithoutNotify(_levelCustomDataModel.LevelAuthorName, clearModifiedState);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.HandleBeatmapProjectSaved))]
        private void HandleBeatmapProjectSaved(EditBeatmapLevelViewController __instance)
        {
            _levelAuthorInputValidator.ClearDirtyState();
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

                SongInfoRoot = new GameObject("SongInfoRoot");
                SongInfoRoot.transform.SetParent(__instance.transform, false);
                SongInfoRoot.transform.localPosition = new Vector3(-500f, 430f, 0f);

                var difficultyBeatmapSetContainer = __instance.transform.Find("DifficultyBeatmapSetContainer").gameObject;
                difficultyBeatmapSetContainer.SetActive(false);
                BeatmapsRoot = difficultyBeatmapSetContainer;

                var beatmapInfoContainer = __instance.transform.Find("BeatmapInfoContainer");

                beatmapInfoContainer.SetParent(SongInfoRoot.transform, false);

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
                    __instance._signalBus.Fire(new BeatmapDataModelSignals.UpdateBeatmapDataSignal(null, null, null, x, null, null, null, null, null, null, null, null, null));
                };

                _levelAuthorInputValidator.transform.parent.Find("Label").GetComponent<TextMeshProUGUI>().text = "Level Author Name";

                var timingInfo = beatmapInfoContainer.Find("TimingInfo");

                var timingInfoSizeFitter = timingInfo.gameObject.AddComponent<ContentSizeFitter>();
                timingInfoSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                timingInfoSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var previewWaveform = new GameObject("PreviewWaveform");
                previewWaveform.transform.SetParent(beatmapInfoContainer);

                var vertical = new GameObject("ExtraSongInfoRoot").AddComponent<VerticalLayoutGroup>();
                vertical.transform.SetParent(SongInfoRoot.transform, false);
                (vertical.transform as RectTransform).anchoredPosition = new Vector2(950f, -100f);

                vertical.childAlignment = TextAnchor.MiddleCenter;
                vertical.spacing = 10f;

                _extraSongInfoSegmentedControl = _textSegmentedControlFactory.Create(vertical.transform, new string[] { "Environments | Q", "Contributors | W" }, ExtraSongInfoSelected);
                _extraSongInfoTabbingSegmentedControlController = _extraSongInfoSegmentedControl.gameObject.AddComponent<TabbingSegmentedControlController>();
                _extraSongInfoTabbingSegmentedControlController.Setup(_extraSongInfoSegmentedControl, true);

                var extraSongInfo = _imageFactory.Create(vertical.transform, "#Background8px", new() { anchoredPosition = new Vector2(0f, 0f), sizeDelta = new Vector2(800f, 800f) });
                extraSongInfo.transform.SetAsLastSibling();
                var layoutElement = extraSongInfo.gameObject.AddComponent<LayoutElement>();
                layoutElement.minWidth = 800f;
                layoutElement.minHeight = 800f;

                EnvironmentRoot = new GameObject("EnvironmentRoot");
                EnvironmentRoot.transform.SetParent(extraSongInfo.transform);
                ContributorsRoot = new GameObject("ContributorsRoot");
                ContributorsRoot.transform.SetParent(extraSongInfo.transform);
                //ModhcartRoot = new GameObject("ModhcartRoot");
                //ModhcartRoot.transform.SetParent(extraSongInfo.transform);

                __instance.gameObject.SetActive(true);
            }
            else
            {
                _rootTabbingSegmentedControlController.AddBindings(false);
                _extraSongInfoTabbingSegmentedControlController.AddBindings(true);
            }

            _rootTabbingSegmentedControlController.ClickCell(0); 
            _extraSongInfoTabbingSegmentedControlController.ClickCell(0);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.DidDeactivate))]
        private void DidDeactivate()
        {
            _rootTabbingSegmentedControlController.ClearBindings();
            _extraSongInfoTabbingSegmentedControlController.ClearBindings();
        }

        private void RootSelected(SegmentedControl segmentedControl, int idx)
        {
            SongInfoRoot.SetActive(idx == 0);
            BeatmapsRoot.SetActive(idx == 1);

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
            EnvironmentRoot.SetActive(idx == 0);
            ContributorsRoot.SetActive(idx == 1);
            //ModhcartRoot.SetActive(idx == 2);
        }
    }
}
