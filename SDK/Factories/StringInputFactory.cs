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
        private PrefabCollector _prefabCollector;

        [Inject]
        private void Construct(
            PrefabCollector prefabCollector)
        {
            _prefabCollector = prefabCollector;
        }

        public TMP_InputField Create(Transform parent, string text, UnityAction<string> onChange)
        {
            var field = GameObject.Instantiate(_prefabCollector.GetInputFieldPrefab().transform.parent, parent, false);
            field.name = "ExStringInput";

            GameObject gameObject = field.gameObject;
            gameObject.SetActive(true);

            var textObj = gameObject.transform.Find("Label").GetComponent<CurvedTextMeshPro>();
            textObj.text = text;
            textObj.richText = true;

            ContentSizeFitter sizeFitter = textObj.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childAlignment = TextAnchor.LowerCenter;

            var inputField = field.transform.Find("InputField").GetComponent<TMP_InputField>();
            inputField.text = string.Empty;
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(onChange);

            Object.Destroy(inputField.GetComponent<StringInputFieldValidator>());
            Object.Destroy(field.transform.Find("ModifiedHint").gameObject);

            return inputField;
        }
    }
}
