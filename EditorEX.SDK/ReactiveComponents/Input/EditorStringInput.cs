using EditorEX.SDK.ReactiveComponents.Attachable;
using EditorEX.Util;
using IPA.Utilities;
using Reactive;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorStringInput : ReactiveComponent
    {
        public string Text
        {
            get => _inputField.text;
            private set
            {
                _inputField.text = value;
                NotifyPropertyChanged();
            }
        }

        public TMP_InputField InputField => _inputField;
        private TMP_InputField _inputField = null!;
        private EditorLabel _text = null!;
        private TextMeshProUGUI? _placeholder;

        /// <summary>
        /// Grey hint text shown while the field is empty. Matches the input's own font and
        /// size so typed text and the hint line up.
        /// </summary>
        public string Placeholder
        {
            set
            {
                EnsurePlaceholder();
                _placeholder!.text = value;
            }
        }

        private void EnsurePlaceholder()
        {
            if (_placeholder != null)
                return;
            var parent =
                _inputField.textViewport != null ? _inputField.textViewport : _inputField.transform;
            var go = new GameObject("Placeholder");
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _placeholder = go.AddComponent<TextMeshProUGUI>();
            _placeholder.alignment = TextAlignmentOptions.Left;
            var src = _inputField.textComponent;
            if (src != null)
            {
                _placeholder.font = src.font;
                _placeholder.fontSize = src.fontSize;
                var c = src.color;
                c.a = 0.4f;
                _placeholder.color = c;
            }
            else
            {
                _placeholder.fontSize = 18f;
                _placeholder.color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            }
            _inputField.placeholder = _placeholder;
        }

        protected override GameObject Construct()
        {
            return new LayoutChildren
            {
                new LayoutChildren
                {
                    new EditorLabel()
                    {
                        Text = "",
                        FontSize = 18f,
                        Alignment = TextAlignmentOptions.Left,
                    }
                        .Attach<ColorSOAttachable>("Input/Text/Normal")
                        .Export(out _text)
                        .AsFlexItem(minSize: new YogaVector(100.pct, 20f)),
                }
                    .AsLayout()
                    .Export(out var viewport)
                    .WithNativeComponent(out RectMask2D _)
                    .AsFlexItem(size: new YogaVector(100.pct, 22f)),
                new EditorImage() { Source = "#WhitePixel" }
                    .Attach<ColorSOAttachable>("Input/Background")
                    .AsFlexItem(size: new YogaVector(100.pct, 1f)),
            }
                .AsLayout()
                .WithNativeComponent(out _inputField)
                .With(x =>
                {
                    _inputField.textComponent = _text.TextMesh;
                    _inputField.textViewport = viewport.ContentTransform;
                    _inputField.onSubmit.AddListener(_ =>
                    {
                        NotifyPropertyChanged(nameof(Text));
                    });
                })
                .AsFlexItem(size: new YogaVector("auto", 20f))
                .AsFlexGroup(FlexDirection.Column, gap: 6f)
                .Use();
        }

        protected override void OnStart()
        {
            Content.SetActive(false);
            Content.SetActive(true);
            if (_inputField.caretRectTrans == null)
            {
                return;
            }
            _inputField.caretRectTrans.sizeDelta = new Vector2(
                _text.ContentTransform.sizeDelta.x,
                _text.ContentTransform.sizeDelta.y
            );
        }

        protected override void OnLayoutApply()
        {
            var yogaModifier = LayoutModifier as YogaModifier;
            var getNode = typeof(YogaModifier).GetField(
                "_node",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var node = getNode?.GetValue(yogaModifier);
            var getWidth = node
                ?.GetType()
                .GetMethod(
                    "LayoutGetWidth",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
                );
            var width = getWidth?.Invoke(node, null) as float? ?? 0f;
            _text.ContentTransform.sizeDelta = new Vector2(
                width,
                _text.ContentTransform.sizeDelta.y
            );
            if (_inputField.caretRectTrans == null)
            {
                return;
            }
            _inputField.caretRectTrans.sizeDelta = new Vector2(
                width,
                _text.ContentTransform.sizeDelta.y
            );
        }
    }
}
