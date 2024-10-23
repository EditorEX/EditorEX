using HMUI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.Components
{
    public class EditorScrollViewContent : MonoBehaviour
    {
        public ScrollView ScrollView
        {
            get
            {
                return scrollView;
            }
            set
            {
                scrollView = value;
            }
        }

        protected void Start()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            StopAllCoroutines();
            StartCoroutine(SetupScrollView());
        }

        protected void OnEnable()
        {
            UpdateScrollView();
        }

        protected void OnRectTransformDimensionsChange()
        {
            dirty = true;
        }

        protected void Update()
        {
            if (dirty)
            {
                dirty = false;
                UpdateScrollView();
            }
        }

        private IEnumerator SetupScrollView()
        {
            RectTransform rectTransform = scrollView.contentTransform;
            yield return new WaitWhile(() => rectTransform.sizeDelta.y == -1f);
            UpdateScrollView();
            yield break;
        }

        private void UpdateScrollView()
        {
            scrollView.SetContentSize(scrollView.contentTransform.rect.height);
            scrollView.RefreshButtons();
        }

        [SerializeField]
        private ScrollView scrollView;

        private bool dirty;
    }
}
