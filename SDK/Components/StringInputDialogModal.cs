using System;
using UnityEngine;

namespace EditorEX.SDK.Components
{
    internal class StringInputDialogModal : MonoBehaviour
    {
        private Action<string>? _onConfirm = null;
        private Action? _onDeny = null;

        private void Start() { }

        public void Prompt(
            string title,
            string inputTitle,
            string initialValue,
            Action<string>? onConfirm,
            Action? onDeny
        )
        {
            // _title.text = title;

            // _inputField.text = initialValue;
            // _inputTitle.text = inputTitle;

            // _modalView.Show(true, true);

            _onConfirm = onConfirm;
            _onDeny = onDeny;
        }

        private void Deny()
        {
            _onDeny?.Invoke();
            // _modalView.Hide();

            _onDeny = null;
            _onConfirm = null;
        }

        private void Confirm()
        {
            // _onConfirm?.Invoke(_inputField.text);
            // _modalView.Hide();

            _onDeny = null;
            _onConfirm = null;
        }
    }
}
