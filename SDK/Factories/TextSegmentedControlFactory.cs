using EditorEX.SDK.Collectors;
using HMUI;
using System;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class TextSegmentedControlFactory
    {
        private PrefabCollector _prefabCollector;
        private DiContainer _container;

        [Inject]
        private void Construct(
            PrefabCollector prefabCollector,
            DiContainer container)
        {
            _prefabCollector = prefabCollector;
            _container = container;
        }

        public TextSegmentedControl Create(Transform parent, string[] strings, Action<SegmentedControl, int> onSelected)
        {
            var segmentedControl = _container.InstantiatePrefab(_prefabCollector.GetSegmentedControlPrefab()).GetComponent<TextSegmentedControl>();
            segmentedControl.transform.SetParent(parent, false);

            segmentedControl.SetTexts(strings);

            segmentedControl.didSelectCellEvent += onSelected;

            return segmentedControl;
        }
    }
}