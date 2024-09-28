using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using EditorEX.MapData.Contexts;
using EditorEX.UI.Factories;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches
{
    internal class EditDifficultyBeatmapPatches : IAffinity
    {
        private IconButtonFactory _iconButtonFactory;

        public EditDifficultyBeatmapPatches(IconButtonFactory iconButtonFactory) 
        {
            _iconButtonFactory = iconButtonFactory;
        }

        [AffinityPatch(typeof(DifficultyBeatmapView), nameof(DifficultyBeatmapView.SetData))]
        [AffinityPostfix]
        private void SetVersionAvailableOptions(DifficultyBeatmapView __instance)
        {
            var v4 = LevelContext.Version >= BeatmapProjectFileHelper.version400;

            __instance._colorSchemeDropdown.transform.parent.gameObject.SetActive(v4);
            __instance._environmentDropdown.transform.parent.gameObject.SetActive(v4);
            __instance._lightshowDropdown.transform.parent.gameObject.SetActive(v4);
            __instance._lightersInputValidator.transform.parent.gameObject.SetActive(v4);
            __instance._mappersInputValidator.transform.parent.gameObject.SetActive(v4);

            var difficultyLabel = __instance.transform.Find("DifficultyLabel");
            var button = __instance.transform?.Find("LabelWrapper")?.Find("ExIconButton")?.GetComponent<Button>();

            if(difficultyLabel == null)
            {
                difficultyLabel = __instance.transform.Find("LabelWrapper").Find("DifficultyLabel");
            }
            else
            {
                var wrapper = new GameObject("LabelWrapper");
                wrapper.transform.parent = __instance.transform;
                wrapper.transform.localPosition = new Vector3(0f, 305f, 0f);

                var fitter = wrapper.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                var layout = wrapper.AddComponent<HorizontalLayoutGroup>();

                layout.spacing = 10f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = false;
                layout.childControlHeight = false;
                layout.childControlWidth = false;

                difficultyLabel.transform.SetParent(wrapper.transform, false);

                var labelFitter = difficultyLabel.gameObject.AddComponent<ContentSizeFitter>();
                labelFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                button = _iconButtonFactory.Create(wrapper.transform, "#IconOpen", () =>
                {
                    Plugin.Log.Info("silly");
                });
            }

            button.interactable = __instance._beatmapData != null;
        }
    }
}
