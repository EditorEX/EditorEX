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
        private ImageFactory _imageFactory;
        private PrefabCollector _prefabCollector;
        private DiContainer _container;

        [Inject]
        private void Construct(
            ImageFactory imageFactory,
            PrefabCollector prefabCollector,
            DiContainer container)
        {
            _imageFactory = imageFactory;
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

            var content = new GameObject("Content");
            content.transform.SetParent(gameObject.transform, false);

            var textObj = gameObject.transform.Find("BeatmapEditorLabel").GetComponent<CurvedTextMeshPro>();
            textObj.text = text;
            textObj.richText = true;

            textObj.transform.SetParent(content.transform, false);

            ContentSizeFitter textSizeFitter = textObj.gameObject.AddComponent<ContentSizeFitter>();
            textSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            textSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var contentStackLayoutGroup = content.gameObject.AddComponent<StackLayoutGroup>();
            contentStackLayoutGroup.padding = new RectOffset(8, 8, 8, 8);

            var background = gameObject.transform.Find("Background8px").gameObject;
            GameObject.Destroy(background);

            _imageFactory.Create(gameObject.transform, "#Background8px", new Base.LayoutData());

            ContentSizeFitter buttonSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            buttonSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var stackLayoutGroup = gameObject.AddComponent<StackLayoutGroup>();

            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 0;

            return button;
        }
    }
}
