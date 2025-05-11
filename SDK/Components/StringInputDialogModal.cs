using EditorEX.SDK.Factories;
using HMUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Components
{
    internal class StringInputDialogModal : MonoBehaviour
    {
        private StringInputFactory _stringInputFactory;
        private ModalFactory _modalFactory;
        private ButtonFactory _buttonFactory;
        private TextFactory _textFactory;

        private TMP_InputField _inputField;
        private CurvedTextMeshPro _inputTitle;
        private EditorModalView _modalView;
        private CurvedTextMeshPro _title;

        private Action<string> _onConfirm;
        private Action _onDeny;

        [Inject]
        private void Construct(
            StringInputFactory stringInputFactory,
            ModalFactory modalFactory,
            ButtonFactory buttonFactory,
            TextFactory textFactory)
        {
            _stringInputFactory = stringInputFactory;
            _modalFactory = modalFactory;
            _buttonFactory = buttonFactory;
            _textFactory = textFactory;
        }

        private void Start()
        {
            var mainScreen = GameObject.Find("MainScreen");
            transform.SetParent(mainScreen.transform, false);

            _modalView = _modalFactory.Create(transform);

            var vertical = new GameObject().AddComponent<VerticalLayoutGroup>();
            vertical.transform.SetParent(_modalView.transform);
            vertical.spacing = 80f;
            vertical.padding = new RectOffset(30, 30, 15, 30);

            var verticalFitter = vertical.gameObject.AddComponent<ContentSizeFitter>();
            verticalFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            verticalFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _title = _textFactory.Create(vertical.transform, "", 20f, "Button/Text/Normal");

            var titleFitter = _title.gameObject.AddComponent<ContentSizeFitter>();
            titleFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            titleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _inputField = _stringInputFactory.Create(vertical.transform, "", 200f, null);
            _inputTitle = _inputField.transform.parent.Find("Label").GetComponent<CurvedTextMeshPro>();

            var horizontal = new GameObject("Horizontal").AddComponent<HorizontalLayoutGroup>();
            horizontal.spacing = 30f;
            horizontal.transform.SetParent(vertical.transform, false);

            var horizontalFitter = horizontal.gameObject.AddComponent<ContentSizeFitter>();
            horizontalFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            horizontalFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _buttonFactory.Create(horizontal.transform, "Cancel", Deny);
            _buttonFactory.Create(horizontal.transform, "Confirm", Confirm);

            _modalView.gameObject.SetActive(false);
        }

        public void Prompt(string title, string inputTitle, string initialValue, Action<string> onConfirm, Action onDeny)
        {
            _title.text = title;

            _inputField.text = initialValue;
            _inputTitle.text = inputTitle;

            _modalView.Show(true, true);

            _onConfirm = onConfirm;
            _onDeny = onDeny;
        }

        private void Deny()
        {
            _onDeny?.Invoke();
            _modalView.Hide();

            _onDeny = null;
            _onConfirm = null;
        }

        private void Confirm()
        {
            _onConfirm?.Invoke(_inputField.text);
            _modalView.Hide();

            _onDeny = null;
            _onConfirm = null;
        }
    }
}
