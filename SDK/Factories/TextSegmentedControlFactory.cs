using EditorEX.SDK.Collectors;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class TextSegmentedControlFactory
    {
        private PrefabCollector _prefabCollector = null!;
        private DiContainer _container = null!;

        [Inject]
        private void Construct(
            PrefabCollector prefabCollector,
            DiContainer container)
        {
            _prefabCollector = prefabCollector;
            _container = container;
        }

        public TextSegmentedControl Create(Transform parent, IEnumerable<string> strings, Action<SegmentedControl, int> onSelected)
        {
            var segmentedControl = _container.InstantiatePrefab(_prefabCollector.GetSegmentedControlPrefab()).GetComponent<TextSegmentedControl>();
            segmentedControl.transform.SetParent(parent, false);

            segmentedControl.SetTexts(strings.ToArray());

            segmentedControl.didSelectCellEvent += onSelected;

            return segmentedControl;
        }
    }
}