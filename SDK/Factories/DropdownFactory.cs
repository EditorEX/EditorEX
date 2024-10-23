using BeatmapEditor3D;
using EditorEX.SDK.Collectors;
using EditorEX.Util;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Zenject;

namespace EditorEX.SDK.Factories
{
    public class DropdownFactory
    {
        private ColorCollector _colorCollector;
        private TransitionCollector _transitionCollector;
        private PrefabCollector _prefabCollector;
        private DiContainer _container;

        [Inject]
        private void Construct(
            ColorCollector colorCollector,
            TransitionCollector transitionCollector,
            PrefabCollector prefabCollector,
            DiContainer container)
        {
            _colorCollector = colorCollector;
            _transitionCollector = transitionCollector;
            _prefabCollector = prefabCollector;
            _container = container;
        }

        public SimpleTextEditorDropdownView Create(Transform parent, string text, bool accent, float inputWidth, IEnumerable<string> options, Action<DropdownEditorView, int> onClick)
        {
            var dropdown = _container.InstantiatePrefab(_prefabCollector.GetTextDropdownPrefab().transform.parent).GetComponentInChildren<SimpleTextEditorDropdownView>();
            dropdown.transform.parent.SetParent(parent, false);

            dropdown.transform.parent.gameObject.name = "ExTextDropdown";
            dropdown.interactable = true;

            var rootSizeFitter = dropdown.transform.parent.gameObject.AddComponent<ContentSizeFitter>();
            rootSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            rootSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rootHorizontal = dropdown.transform.parent.gameObject.AddComponent<HorizontalLayoutGroup>();
            rootHorizontal.spacing = 20f;
            rootHorizontal.childControlWidth = false;

            EventInvokeUtil.ClearEventInvocations(dropdown, "didSelectCellWithIdxEvent");
            dropdown.didSelectCellWithIdxEvent += onClick;

            var textObj = dropdown.transform.parent.Find("BeatmapEditorLabel").GetComponent<CurvedTextMeshPro>();
            textObj.text = text;
            textObj.richText = true;

            var sizeFitter = textObj.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            textObj.rectTransform.sizeDelta = new Vector2(inputWidth / 5f, 50f);

            var layoutElement = dropdown.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = inputWidth;
            layoutElement.preferredHeight = 50f;
            layoutElement.minHeight = 50f;

            if (accent)
            {
                var color = _colorCollector.GetColor("Dropdown/Background/Normal");
                var transition = _transitionCollector.GetTransition<ColorTransitionSO>("EditorDropdown/CellBackground");

                dropdown._tableView.transform.Find("Background4px").Find("Background").GetComponent<ImageView>()._colorSo = color;

                dropdown._text.transform.parent.GetComponentInChildren<ColorGraphicStateTransition>()._transition = transition;

                dropdown._cellPrefab = GameObject.Instantiate(dropdown._cellPrefab);
                dropdown._cellPrefab.GetComponentInChildren<ColorGraphicStateTransition>()._transition = transition;

                dropdown._tableView.LazyInit();
                foreach (Transform child in dropdown._tableView.contentTransform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                dropdown._tableView._reusableCells.Clear();
            }

            dropdown.SetTexts(options.ToList());

            return dropdown;
        }
    }
}