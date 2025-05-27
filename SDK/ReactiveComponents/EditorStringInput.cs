using System.Runtime.InteropServices;
using EditorEX.SDK.ReactiveComponents.Attachable;
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
        public TMP_InputField InputField => _inputField;
        private TMP_InputField _inputField = null!;
        private EditorLabel _text = null!;
        protected override GameObject Construct()
        {
            return new Layout()
            {
                Children = {
                    new Layout()
                    {
                        Children = {
                            new EditorLabel()
                            {
                                Text = "",
                                FontSize = 18f,
                                Alignment = TextAlignmentOptions.Left,
                            }
                            .Attach<ColorSOAttachable>("Input/Text/Normal")
                            .Export(out _text)
                            .AsFlexItem(minSize: new YogaVector(100.pct(), 20f))
                        }
                    }
                    .Export(out var viewport)
                    .WithNativeComponent(out RectMask2D _)
                    .AsFlexItem(size: new YogaVector(100.pct(), 22f)),
                    new EditorImage()
                    {
                        Source = "#WhitePixel",
                    }
                    .Attach<ColorSOAttachable>("Input/Background")
                    .AsFlexItem(size: new YogaVector(100.pct(), 1f))
                }
            }
            .WithNativeComponent(out _inputField)
            .With(x =>
            {
                _inputField.textComponent = _text.TextMesh;
                _inputField.textViewport = viewport.ContentTransform;
            })
            .AsFlexItem(size: new YogaVector("auto", 20f))
            .AsFlexGroup(FlexDirection.Column, gap: 6f)
            .Use();
        }

        protected override void OnStart()
        {
            Content.SetActive(false);
            Content.SetActive(true);
        }

        protected override void OnLayoutApply()
        {
            var yogaModifier = LayoutModifier as YogaModifier;
            var getNode = typeof(YogaModifier).GetField("_node", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var node = getNode?.GetValue(yogaModifier);
            var getWidth = node?.GetType().GetMethod("LayoutGetWidth", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var width = getWidth?.Invoke(node, null) as float? ?? 0f;
            _text.ContentTransform.sizeDelta = new Vector2(width, _text.ContentTransform.sizeDelta.y);
            _inputField.caretRectTrans.sizeDelta = new Vector2(width, _text.ContentTransform.sizeDelta.y);
        }
    }
}