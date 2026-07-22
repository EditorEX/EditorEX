using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.ReactiveComponents;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Patches
{
    internal class EditDifficultyBeatmapPatches : IAffinity
    {
        private readonly LazyInject<BeatmapProjectManager> _beatmapProjectManager;
        private readonly DifficultyLabelEditor _labelEditor;

        public EditDifficultyBeatmapPatches(
            LazyInject<BeatmapProjectManager> beatmapProjectManager,
            ILevelCustomDataModel levelCustomDataModel,
            IReactiveContainer reactiveContainer
        )
        {
            _beatmapProjectManager = beatmapProjectManager;
            _labelEditor = new DifficultyLabelEditor(
                new DifficultyLabelStore(levelCustomDataModel),
                reactiveContainer
            );
        }

        [AffinityPatch(typeof(DifficultyBeatmapView), nameof(DifficultyBeatmapView.SetData))]
        [AffinityPostfix]
        private void Modify(DifficultyBeatmapView __instance, DifficultyBeatmapData beatmapData)
        {
            if (beatmapData == null)
                return;
            var v4 = LevelContext.Version >= BeatmapProjectFileHelper.version400;
            var beatmapVersion = BeatmapProjectFileHelper.GetVersionedJSONVersion(
                _beatmapProjectManager.Value._workingBeatmapProject,
                beatmapData.beatmapFilename
            );
            var v4Map = beatmapVersion >= BeatmapProjectFileHelper.version400;

            __instance._colorSchemeDropdown.transform.parent.gameObject.SetActive(v4);
            __instance._environmentDropdown.transform.parent.gameObject.SetActive(v4);
            __instance._lightshowDropdown.transform.parent.gameObject.SetActive(v4 && v4Map);
            __instance._lightersInputValidator.transform.parent.gameObject.SetActive(v4 && v4Map);
            __instance._mappersInputValidator.transform.parent.gameObject.SetActive(v4 && v4Map);

            var difficultyLabel = __instance.transform.Find("DifficultyLabel");
            var button = __instance
                .transform?.Find("LabelWrapper")
                ?.Find("ExIconButton")
                ?.GetComponent<Button>();

            if (difficultyLabel == null)
            {
                difficultyLabel = __instance
                    .transform!.Find("LabelWrapper")
                    .Find("DifficultyLabel");
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
            }

            if (difficultyLabel == null)
                return;

            var labelTmp = difficultyLabel.GetComponent<TextMeshProUGUI>();
            _labelEditor.EnsureLabelInput(__instance, difficultyLabel, labelTmp);

            var filename = beatmapData.beatmapFilename;
            _labelEditor.SetActiveFilename(__instance, filename);
            _labelEditor.ResetToLabelView(__instance);
        }
    }
}
