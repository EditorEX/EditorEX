using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.Factories;
using HMUI;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;
using Zenject;

// :dread:
namespace EditorEX.UI.Patches
{
    internal class EditBeatmapLevelPatches : IAffinity, ITickable
    {
        private readonly BeatmapLevelDataModel _beatmapLevelDataModel;
        private readonly TextSegmentedControlFactory _textSegmentedControlFactory;
        private readonly KeyboardBinder _keyboardBinder;
        private readonly LevelCustomDataModel _levelCustomDataModel;

        private TextSegmentedControl _segmentedControl;

        private GameObject SongInfoRoot;
        private GameObject BeatmapsRoot;

        private StringInputFieldValidator _levelAuthorInputValidator;

        private EditBeatmapLevelPatches(
            BeatmapLevelDataModel beatmapLevelDataModel,
            TextSegmentedControlFactory textSegmentedControlFactory,
            LevelCustomDataModel levelCustomDataModel)
        {
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _textSegmentedControlFactory = textSegmentedControlFactory;
            _levelCustomDataModel = levelCustomDataModel;
            _keyboardBinder = new KeyboardBinder();
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
                _segmentedControl = _textSegmentedControlFactory.Create(__instance.transform, new string[] { "Song Info | 1", "Beatmaps | 2" }, Selected);
                (_segmentedControl.transform as RectTransform).anchoredPosition = new Vector2(0f, 500f);

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

                __instance.gameObject.SetActive(true);
            }

            _keyboardBinder.AddBinding(KeyCode.Alpha1, KeyboardBinder.KeyBindingType.KeyDown, x =>
            {
                _segmentedControl.cells[0].OnPointerClick(new PointerEventData(null));
            });

            _keyboardBinder.AddBinding(KeyCode.Alpha2, KeyboardBinder.KeyBindingType.KeyDown, x =>
            {
                _segmentedControl.cells[1].OnPointerClick(new PointerEventData(null));
            });

            _keyboardBinder.AddBinding(KeyCode.Mouse5, KeyboardBinder.KeyBindingType.KeyDown, x =>
            {
                _segmentedControl.cells[0].OnPointerClick(new PointerEventData(null));
            });

            _keyboardBinder.AddBinding(KeyCode.Mouse6, KeyboardBinder.KeyBindingType.KeyDown, x =>
            {
                _segmentedControl.cells[1].OnPointerClick(new PointerEventData(null));
            });
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(EditBeatmapLevelViewController), nameof(EditBeatmapLevelViewController.DidDeactivate))]
        private void DidDeactivate(EditBeatmapLevelViewController __instance)
        {
            _keyboardBinder.ClearBindings();
        }

        private void Selected(SegmentedControl segmentedControl, int idx)
        {
            SongInfoRoot.SetActive(idx == 0);
            BeatmapsRoot.SetActive(idx == 1);
        }

        public void Tick()
        {
            _keyboardBinder.ManualUpdate();
        }
    }
}
