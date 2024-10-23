using EditorEX.SDK.Base;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using HMUI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class ScrollViewFactory
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

        public ScrollView Create(Transform parent, LayoutData layoutData)
        {
            var scrollView = _container.InstantiatePrefab(_prefabCollector.GetScrollViewPrefab()).GetComponent<ScrollView>();
            scrollView.transform.SetParent(parent, false);

            scrollView.name = "ExScrollView";

            var rectTransform = scrollView.GetComponent<RectTransform>();
            rectTransform.sizeDelta = layoutData.sizeDelta ?? rectTransform.sizeDelta;
            rectTransform.anchoredPosition = layoutData.anchoredPosition ?? rectTransform.anchoredPosition;

            scrollView.viewportTransform.sizeDelta = new Vector2(-20f, -20f);
            scrollView.viewportTransform.anchoredPosition = new Vector2(0f, 0f);

            foreach (Transform child in scrollView.contentTransform)
            {
                GameObject.Destroy(child.gameObject);
            }

            GameObject.Destroy(scrollView.GetComponent<TableView>());


            scrollView.contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            scrollView.contentTransform.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollViewContent = scrollView.contentTransform.gameObject.AddComponent<EditorScrollViewContent>();
            scrollViewContent.ScrollView = scrollView;

            return scrollView;
        }
    }
}
