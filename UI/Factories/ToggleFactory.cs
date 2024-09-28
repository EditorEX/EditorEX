using EditorEX.UI.Collectors;
using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class ToggleFactory
    {
        private PrefabCollector _prefabCollector;

        [Inject]
        private void Construct(PrefabCollector prefabCollector)
        {
            _prefabCollector = prefabCollector;
        }

        public Button Create(Transform parent, string text, Action onClick)
        {
            var button = GameObject.Instantiate(_prefabCollector.GetButtonPrefab(), parent, false);
            button.name = "ExButton";
            button.interactable = true;

            GameObject gameObject = button.gameObject;
            gameObject.SetActive(true);

            var textObj = gameObject.transform.Find("BeatmapEditorLabel").GetComponent<CurvedTextMeshPro>();
            textObj.text = text;
            textObj.richText = true;

            ContentSizeFitter buttonSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            buttonSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            return button;
        }
    }
}
