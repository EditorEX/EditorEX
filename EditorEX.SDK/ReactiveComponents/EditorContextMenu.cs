using System;
using System.Reflection;
using BeatSaberMarkupLanguage;
using EditorEX.SDK.ReactiveComponents.Attachable;
using EditorEX.Util;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorContextMenu : BlockingModalBase
    {
        public bool IsShown => IsPushed;
        public Layout Layout =>
            _layoutChildren ?? throw new InvalidOperationException("Layout not initialized.");
        public Transform ViewTransform =>
            _viewLayout?.ContentTransform
            ?? throw new InvalidOperationException("View transform not initialized.");

        private Layout? _layoutChildren;
        private Layout? _viewLayout;

        protected override IReactiveComponent ConstructContent()
        {
            return new LayoutChildren
            {
                new LayoutChildren
                {
                    new LayoutChildren
                    {
                        new Layout()
                            .AsFlexItem()
                            .AsFlexGroup(FlexDirection.Column, gap: 5f, padding: new YogaFrame(2f))
                            .Bind(ref _layoutChildren),
                    }
                        .As<EditorBackground>(x =>
                        {
                            x.ImageType = UnityEngine.UI.Image.Type.Sliced;
                        })
                        .With(x =>
                            x.WrappedImage.Attach<ColorSOAttachable>(
                                "VerticalList/Background/Pressed"
                            )
                        )
                        .With(x =>
                        {
                            var c = async () =>
                            {
                                try
                                {
                                    var bytes = Utilities.GetResource(
                                        Assembly.GetExecutingAssembly(),
                                        "EditorEX.SDK.Resources.Background6px.png"
                                    );

                                    var texture = await Utilities.LoadImageAsync(
                                        bytes,
                                        true,
                                        false
                                    );

                                    var sprite = Sprite.Create(
                                        texture,
                                        new Rect(0, 0, texture.width, texture.height),
                                        new Vector2(0.5f, 0.5f),
                                        100f,
                                        0,
                                        SpriteMeshType.Tight,
                                        new Vector4(7f, 7f, 7f, 7f)
                                    );
                                    x.Sprite = sprite;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error loading image: {ex.Message}");
                                }
                            };
                            c();
                        })
                        .AsFlexGroup(padding: new YogaFrame(5f, 8f))
                        .AsFlexItem(flexGrow: 1f),
                }
                    .As<EditorBackground>(x =>
                    {
                        x.Source = "#Background8px";
                        x.ImageType = UnityEngine.UI.Image.Type.Sliced;
                    })
                    .With(x => x.WrappedImage.Attach<ColorSOAttachable>("Button/Text/Highlighted"))
                    .Bind(ref _viewLayout)
                    .AsFlexGroup(padding: 2f)
                    .AsFlexItem(flexGrow: 1f),
            }
                .AsLayout()
                .AsFlexGroup(gap: 2f, constrainHorizontal: false, constrainVertical: false);
        }
    }
}
