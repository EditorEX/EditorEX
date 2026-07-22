using System.Collections.Generic;
using BeatmapEditor3D.Views;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.UI.Components;
using Reactive;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace EditorEX.UI.Patches
{
    // Owns the per-view label input lifecycle (creation/wiring) plus the
    // begin/end/refresh edit flow that swaps the static label for an input field.
    internal class DifficultyLabelEditor
    {
        private readonly DifficultyLabelStore _store;
        private readonly IReactiveContainer _reactiveContainer;

        private readonly Dictionary<DifficultyBeatmapView, EditorStringInput> _labelInputs = new();
        private readonly Dictionary<DifficultyBeatmapView, string> _activeFilenameByView = new();
        private readonly Dictionary<DifficultyBeatmapView, TextMeshProUGUI> _labelTexts = new();
        private readonly Dictionary<DifficultyBeatmapView, string> _stockNames = new();

        public DifficultyLabelEditor(
            DifficultyLabelStore store,
            IReactiveContainer reactiveContainer
        )
        {
            _store = store;
            _reactiveContainer = reactiveContainer;
        }

        public void SetActiveFilename(DifficultyBeatmapView view, string filename)
        {
            _activeFilenameByView[view] = filename;
        }

        // Creates and wires the themed input for this view's label the first time it's seen.
        // Starts hidden; a double-click on the label reveals it.
        public void EnsureLabelInput(
            DifficultyBeatmapView view,
            Transform difficultyLabel,
            TextMeshProUGUI? labelTmp
        )
        {
            _labelInputs.TryGetValue(view, out var labelInput);
            if (labelInput != null)
                return;

            var labelWrapper = difficultyLabel.parent;
            labelInput = new EditorStringInput
            {
                Placeholder = labelTmp != null ? labelTmp.text : string.Empty,
            };
            labelInput.InputField.onEndEdit.AddListener(value => EndEdit(view, value));
            new LayoutChildren { labelInput.AsFlexItem(size: new YogaVector(200f, 30f)) }
                .AsLayout()
                .AsFlexGroup()
                .WithReactiveContainer(_reactiveContainer)
                .Use(labelWrapper);
            _labelInputs[view] = labelInput;

            if (labelTmp != null)
            {
                labelTmp.raycastTarget = true;
                _labelTexts[view] = labelTmp;
                _stockNames[view] = labelTmp.text;
                var handler =
                    difficultyLabel.gameObject.GetComponent<DoubleClickHandler>()
                    ?? difficultyLabel.gameObject.AddComponent<DoubleClickHandler>();
                handler.OnDoubleClick = () => BeginEdit(view);
            }
        }

        // Shows the static label with the custom label if set, else the stock difficulty name.
        public void RefreshLabelDisplay(DifficultyBeatmapView view)
        {
            if (!_labelTexts.TryGetValue(view, out var tmp) || tmp == null)
                return;
            if (!_activeFilenameByView.TryGetValue(view, out var filename))
                return;
            var custom = _store.Read(filename);
            _stockNames.TryGetValue(view, out var stock);
            tmp.text = string.IsNullOrWhiteSpace(custom) ? (stock ?? tmp.text) : custom;
        }

        // Double-click: hide the label, show the input focused with the current value.
        public void BeginEdit(DifficultyBeatmapView view)
        {
            if (!_labelInputs.TryGetValue(view, out var input) || input == null)
                return;
            if (!_activeFilenameByView.TryGetValue(view, out var filename))
                return;
            input.InputField.SetTextWithoutNotify(_store.Read(filename) ?? string.Empty);
            if (_labelTexts.TryGetValue(view, out var tmp) && tmp != null)
                tmp.gameObject.SetActive(false);
            input.Content.SetActive(true);
            input.InputField.ActivateInputField();
            input.InputField.Select();
        }

        // Commit: persist, hide the input, restore the label.
        public void EndEdit(DifficultyBeatmapView view, string value)
        {
            if (_activeFilenameByView.TryGetValue(view, out var filename))
                _store.Write(filename, value);
            ResetToLabelView(view);
        }

        // Reset to the label view for the (possibly different) beatmap now shown.
        public void ResetToLabelView(DifficultyBeatmapView view)
        {
            if (_labelInputs.TryGetValue(view, out var input) && input != null)
                input.Content.SetActive(false);
            if (_labelTexts.TryGetValue(view, out var tmp) && tmp != null)
                tmp.gameObject.SetActive(true);
            RefreshLabelDisplay(view);
        }
    }
}
