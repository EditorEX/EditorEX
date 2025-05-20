using EditorEX.SDK.ReactiveComponents.Attachable;
using Reactive;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorStringInput : ReactiveComponent
    {
        private TMP_InputField _inputField = null!;
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
                            .Export(out var _text)
                            .AsFlexItem()
                        }
                    }
                    .Export(out var viewport)
                    .WithNativeComponent(out RectMask2D _)
                    .AsFlexItem(size: new YogaVector("stretch", 39f)),
                    new EditorImage()
                    {
                        Source = "#WhitePixel",
                    }
                    .Attach<ColorSOAttachable>("Input/Background")
                    .AsFlexItem(size: new YogaVector("stretch", 1f))
                }
            }
            .WithNativeComponent(out _inputField)
            .With(x =>
            {
                _inputField.textComponent = _text.TextMesh;
                _inputField.textViewport = viewport.ContentTransform;
            })
            .AsFlexItem(size: new YogaVector("auto", 20f))
            .AsFlexGroup(FlexDirection.Column)
            .Use();
        }
    }
}