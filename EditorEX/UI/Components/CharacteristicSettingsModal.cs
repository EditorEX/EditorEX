using System;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Attachable;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Components
{
    public class CharacteristicSettingsModal : BlockingModalBase
    {
        public Layout ContentLayout =>
            _content ?? throw new InvalidOperationException("Modal not initialized.");
        private Layout? _content;

        protected override IReactiveComponent ConstructContent()
        {
            return new LayoutChildren
            {
                new LayoutChildren
                {
                    new EditorHeaderLabel
                    {
                        Text = "Custom Characteristics",
                        FontSize = 22f,
                        Alignment = TextAlignmentOptions.Center,
                    }.AsFlexItem(size: new YogaVector(100.pct, 30f)),
                    new Layout()
                        .AsFlexItem(flexGrow: 1f)
                        .AsFlexGroup(FlexDirection.Column, gap: 8f)
                        .Bind(ref _content),
                }
                    .As<EditorBackground>(x =>
                    {
                        x.Source = "#Background8px";
                        x.ImageType = Image.Type.Sliced;
                    })
                    .With(x =>
                        x.WrappedImage.Attach<ColorSOAttachable>("VerticalList/Background/Pressed")
                    )
                    .AsFlexGroup(FlexDirection.Column, padding: 12f, gap: 10f)
                    .AsFlexItem(size: new YogaVector(560f, 420f)),
            }
                .AsLayout()
                .AsFlexGroup(constrainHorizontal: false, constrainVertical: false);
        }

        // Outside-click handling is delegated to BlockingModalBase via ModalBase.OnClickOutside.
    }
}
