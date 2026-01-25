using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
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
        public bool IsShown => _opened;
        public Layout Layout =>
            _layoutChildren ?? throw new InvalidOperationException("Layout not initialized.");
        private bool _opened;
        private Layout? _layoutChildren;

        protected override GameObject Construct()
        {
            Console.WriteLine(
                Resources
                    .FindObjectsOfTypeAll<Sprite>()
                    .FirstOrDefault(x => x.name == "Background8px")
                    .pixelsPerUnit
            );
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
                                        "EditorEX.UI.Resources.Background6px.png"
                                    );
                                    foreach (
                                        var thing in Assembly
                                            .GetExecutingAssembly()
                                            .GetManifestResourceNames()
                                    )
                                    {
                                        Console.WriteLine(thing);
                                    }
                                    Console.WriteLine(bytes.Length);
                                    var texture = await Utilities.LoadImageAsync(
                                        bytes,
                                        true,
                                        false
                                    );
                                    Console.WriteLine(texture.GetPixel(4, 0).r);
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
                    .AsFlexGroup(padding: 2f)
                    .AsFlexItem(flexGrow: 1f),
            }
                .AsLayout()
                .AsFlexGroup(gap: 2f, constrainHorizontal: false, constrainVertical: false)
                .Use();
        }

        protected override void OnOpen(bool opened)
        {
            _opened = opened;
        }

        protected override void OnClose(bool closed)
        {
            _opened = !closed;
        }
    }
}
