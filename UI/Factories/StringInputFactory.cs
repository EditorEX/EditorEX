using BeatmapEditor3D;
using EditorEX.UI.Collectors;
using HMUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class StringInputFactory
    {
        private PrefabCollector _prefabCollector;

        [Inject]
        private void Construct(PrefabCollector prefabCollector)
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

            ContentSizeFitter buttonSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            buttonSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var inputField = field.transform.Find("InputField").GetComponent<TMP_InputField>();
            inputField.text = string.Empty;
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(onChange);

            UnityEngine.Object.Destroy(inputField.GetComponent<StringInputFieldValidator>());
            UnityEngine.Object.Destroy(field.transform.Find("ModifiedHint").gameObject);

            return inputField;
        }
    }
}
