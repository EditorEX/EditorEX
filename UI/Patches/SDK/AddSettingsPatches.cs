using BeatmapEditor3D;
using EditorEX.SDK.Factories;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using HMUI;
using SiraUtil.Affinity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches.SDK
{
    internal class AddSettingsPatches : IAffinity
    {
        private readonly List<IViewContent<SettingsViewData>> _viewContents;
        private readonly List<string> _viewNames;
        private readonly List<GameObject> _viewObjects;
        private readonly TextSegmentedControlFactory _textSegmentedControlFactory;

        private int currentSelected = 0;

        private AddSettingsPatches(
            List<IViewContent<SettingsViewData>> viewContents,
            TextSegmentedControlFactory textSegmentedControlFactory) 
        { 
            _viewContents = viewContents;
            _viewNames = _viewContents.Select(x => x.GetViewData().Name).ToList();
            _viewNames.Insert(0, "Official");
            _viewObjects = new List<GameObject>();

            _textSegmentedControlFactory = textSegmentedControlFactory;
        }

        [AffinityPatch(typeof(BeatmapEditorSettingsViewController), nameof(BeatmapEditorSettingsViewController.DidActivate))]
        [AffinityPostfix]
        private void AddUI(BeatmapEditorSettingsViewController __instance, bool firstActivation)
        { 
            if (firstActivation)
            {
                var segmentedControl = _textSegmentedControlFactory.Create(__instance.transform, _viewNames.ToArray(), Selected);
                segmentedControl.transform.SetAsFirstSibling();
                segmentedControl.transform.localPosition = new Vector3(0f, 500f);

                var officialSettings = new GameObject("Official");
                officialSettings.transform.SetParent(__instance.transform, false);
                officialSettings.transform.localPosition = new Vector3(0f, 500f);

                var container = __instance.transform.Find("Container");
                container.SetParent(officialSettings.transform, false);
                container.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, -40f);

                _viewObjects.Add(officialSettings);

                for (int i = 0; i < _viewContents.Count; i++)
                {
                    var settingsObject = new GameObject(_viewNames[i+1]);
                    settingsObject.transform.SetParent(container.transform, false);
                    settingsObject.transform.localPosition = new Vector3(0f, 500f);

                    _viewContents[i].Create(settingsObject);

                    _viewObjects.Add(settingsObject);
                }

                container.gameObject.SetActive(false);
                container.gameObject.SetActive(true);

                segmentedControl.SelectCellWithNumber(0);
            }
        }

        private void Selected(SegmentedControl segmentedControl, int idx)
        {
            var toDisable = _viewObjects[currentSelected];
            if (currentSelected != 0)
            {
                _viewContents[currentSelected - 1].OnHide();
            }
            toDisable.SetActive(false);

            var toEnable = _viewObjects[idx];
            if (idx != 0)
            {
                _viewContents[idx - 1].OnEnable();
            }
            toEnable.SetActive(true);
        }
    }
}
