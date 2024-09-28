using BeatSaberMarkupLanguage;
using EditorEX.UI.Collectors;
using HMUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class IconButtonFactory
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

        public Button Create(Transform parent, Sprite sprite, UnityAction onClick)
        {
            var button = Create(parent, onClick);

            button.transform.Find("Icon").GetComponent<ImageView>().sprite = sprite;

            return button;
        }

        public Button Create(Transform parent, string location, UnityAction onClick)
        {
            var button = Create(parent, onClick);

            button.transform.Find("Icon").GetComponent<ImageView>().SetImageAsync(location);

            return button;
        }

        private Button Create(Transform parent, UnityAction onClick)
        {
            var button = _container.InstantiatePrefab(_prefabCollector.GetIconButtonPrefab()).GetComponent<Button>();
            button.transform.SetParent(parent, false);

            button.name = "ExIconButton";
            button.interactable = true;

            button.onClick.AddListener(onClick);

            GameObject gameObject = button.gameObject;
            gameObject.SetActive(true);

            return button;
        }
    }
}
