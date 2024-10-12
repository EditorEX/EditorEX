using EditorEX.SDK.Collectors;
using HMUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class ButtonFactory
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

        public Button Create(Transform parent, string text, UnityAction onClick)
        {
            var button = _container.InstantiatePrefab(_prefabCollector.GetButtonPrefab()).GetComponent<Button>();
            button.transform.SetParent(parent, false);

            button.name = "ExButton";
            button.interactable = true;

            GameObject gameObject = button.gameObject;
            gameObject.SetActive(true);

            button.onClick.AddListener(onClick);

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
