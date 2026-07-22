using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using Reactive;
using Reactive.Yoga;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Patches
{
    internal class EditDifficultyBeatmapPatches : IAffinity
    {
        private LazyInject<BeatmapProjectManager> _beatmapProjectManager;
        private readonly LevelCustomDataModel _levelCustomDataModel;
        private readonly IReactiveContainer _reactiveContainer;

        private readonly Dictionary<DifficultyBeatmapView, EditorStringInput> _labelInputs = new();
        private readonly Dictionary<DifficultyBeatmapView, string> _activeFilenameByView = new();
        private readonly Dictionary<DifficultyBeatmapView, TextMeshProUGUI> _labelTexts = new();
        private readonly Dictionary<DifficultyBeatmapView, string> _stockNames = new();

        public EditDifficultyBeatmapPatches(
            LazyInject<BeatmapProjectManager> beatmapProjectManager,
            LevelCustomDataModel levelCustomDataModel,
            IReactiveContainer reactiveContainer
        )
        {
            _beatmapProjectManager = beatmapProjectManager;
            _levelCustomDataModel = levelCustomDataModel;
            _reactiveContainer = reactiveContainer;
        }

        private string DifficultyLabelKey =>
            LevelContext.Version.Major >= 4 ? "difficultyLabel" : "_difficultyLabel";

        private string? ReadDifficultyLabel(string filename)
        {
            var datas = _levelCustomDataModel.BeatmapCustomDatasByFilename;
            if (datas != null && datas.TryGetValue(filename, out var cd) && cd != null)
                return cd.Get<string>(DifficultyLabelKey);
            return null;
        }

        private void WriteDifficultyLabel(DifficultyBeatmapView view, string value)
        {
            if (!_activeFilenameByView.TryGetValue(view, out var filename))
                return;
            var datas = _levelCustomDataModel.BeatmapCustomDatasByFilename;
            if (datas == null)
                return;
            if (!datas.TryGetValue(filename, out var cd) || cd == null)
            {
                cd = new CustomData();
                datas[filename] = cd;
            }
            if (string.IsNullOrWhiteSpace(value))
                cd.TryRemove(DifficultyLabelKey, out _);
            else
                cd[DifficultyLabelKey] = value;
        }

        // Shows the static label with the custom label if set, else the stock difficulty name.
        private void RefreshLabelDisplay(DifficultyBeatmapView view)
        {
            if (!_labelTexts.TryGetValue(view, out var tmp) || tmp == null)
                return;
            if (!_activeFilenameByView.TryGetValue(view, out var filename))
                return;
            var custom = ReadDifficultyLabel(filename);
            _stockNames.TryGetValue(view, out var stock);
            tmp.text = string.IsNullOrWhiteSpace(custom) ? (stock ?? tmp.text) : custom;
        }

        // Double-click: hide the label, show the input focused with the current value.
        private void BeginEdit(DifficultyBeatmapView view)
        {
            if (!_labelInputs.TryGetValue(view, out var input) || input == null)
                return;
            if (!_activeFilenameByView.TryGetValue(view, out var filename))
                return;
            input.InputField.SetTextWithoutNotify(ReadDifficultyLabel(filename) ?? string.Empty);
            if (_labelTexts.TryGetValue(view, out var tmp) && tmp != null)
                tmp.gameObject.SetActive(false);
            input.Content.SetActive(true);
            input.InputField.ActivateInputField();
            input.InputField.Select();
        }

        // Commit: persist, hide the input, restore the label.
        private void EndEdit(DifficultyBeatmapView view, string value)
        {
            WriteDifficultyLabel(view, value);
            if (_labelInputs.TryGetValue(view, out var input) && input != null)
                input.Content.SetActive(false);
            if (_labelTexts.TryGetValue(view, out var tmp) && tmp != null)
                tmp.gameObject.SetActive(true);
            RefreshLabelDisplay(view);
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

            _labelInputs.TryGetValue(__instance, out var labelInput);
            if (labelInput == null)
            {
                var labelWrapper = difficultyLabel.parent;
                // A themed Reactive input (placeholder = the stock difficulty name), mounted
                // into the label wrapper. Starts hidden; a double-click on the label reveals it.
                labelInput = new EditorStringInput
                {
                    Placeholder = labelTmp != null ? labelTmp.text : string.Empty,
                };
                labelInput.InputField.onEndEdit.AddListener(value => EndEdit(__instance, value));
                new LayoutChildren { labelInput.AsFlexItem(size: new YogaVector(200f, 30f)) }
                    .AsLayout()
                    .AsFlexGroup()
                    .WithReactiveContainer(_reactiveContainer)
                    .Use(labelWrapper);
                _labelInputs[__instance] = labelInput;

                if (labelTmp != null)
                {
                    labelTmp.raycastTarget = true;
                    _labelTexts[__instance] = labelTmp;
                    _stockNames[__instance] = labelTmp.text;
                    var handler =
                        difficultyLabel.gameObject.GetComponent<DoubleClickHandler>()
                        ?? difficultyLabel.gameObject.AddComponent<DoubleClickHandler>();
                    handler.OnDoubleClick = () => BeginEdit(__instance);
                }
            }

            var filename = beatmapData.beatmapFilename;
            _activeFilenameByView[__instance] = filename;
            // Reset to the label view for the (possibly different) beatmap now shown.
            labelInput.Content.SetActive(false);
            if (labelTmp != null)
                labelTmp.gameObject.SetActive(true);
            RefreshLabelDisplay(__instance);
        }
    }

    internal class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public System.Action? OnDoubleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
                OnDoubleClick?.Invoke();
        }
    }
}
