using BeatmapEditor3D;
using EditorEX.SDK.Collectors;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class StringInputFactory
    {
        private PrefabCollector _prefabCollector = null!;

        [Inject]
        private void Construct(
            PrefabCollector prefabCollector)
        {
            _prefabCollector = prefabCollector;
        }

        public TMP_InputField Create(Transform parent, string text, float inputWidth, UnityAction<string> onChange)
        {
            var field = GameObject.Instantiate(_prefabCollector.GetInputFieldPrefab().transform.parent, parent, false);
            field.name = "ExStringInput";

            var gameObject = field.gameObject;
            gameObject.SetActive(false);

            var rootSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            rootSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            rootSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rootHorizontal = gameObject.AddComponent<HorizontalLayoutGroup>();
            rootHorizontal.spacing = 20f;
            rootHorizontal.childControlWidth = false;

            var textObj = gameObject.transform.Find("Label").GetComponent<CurvedTextMeshPro>();
            textObj.text = text;
            textObj.richText = true;

            var sizeFitter = textObj.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            textObj.rectTransform.sizeDelta = new Vector2(inputWidth / 5f, 25f);

            var inputField = field.transform.Find("InputField").GetComponent<TMP_InputField>();
            inputField.text = string.Empty;
            inputField.onValueChanged.RemoveAllListeners();
            if (onChange != null)
            {
                inputField.onValueChanged.AddListener(onChange);
            }

            var vertical = inputField.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.childForceExpandHeight = false;
            vertical.childControlHeight = false;
            vertical.spacing = 5f;

            inputField.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(inputWidth, 25f);

            var textArea = inputField.transform.Find("Text Area").GetComponent<RectTransform>();
            textArea.sizeDelta = new Vector2(200f, 20f);
            var border = inputField.transform.Find("Border").GetComponent<RectTransform>();
            border.sizeDelta = new Vector2(200f, 1f);

            border.GetComponent<ImageView>().maskable = true;

            Object.Destroy(inputField.GetComponent<StringInputFieldValidator>());
            Object.Destroy(field.transform.Find("ModifiedHint").gameObject);

            gameObject.SetActive(true);

            return inputField;
        }
    }
}
