using EditorEX.SDK.Collectors;
using HMUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class ToggleFactory
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

        public Toggle Create(Transform parent, string text, UnityAction<bool> onClick)
        {
            var toggle = _container.InstantiatePrefab(_prefabCollector.GetTogglePrefab()).GetComponent<Toggle>();
            toggle.transform.SetParent(parent, false);
            toggle.name = "ExToggle";
            toggle.interactable = true;

            GameObject gameObject = toggle.gameObject;
            gameObject.SetActive(true);

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(onClick);

            var textObj = gameObject.transform.Find("BeatmapEditorLabel").GetComponent<CurvedTextMeshPro>();
            textObj.text = text;
            textObj.richText = true;

            ContentSizeFitter buttonSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            buttonSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            return toggle;
        }
    }
}
