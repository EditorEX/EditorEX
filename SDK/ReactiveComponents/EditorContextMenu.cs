using System;
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
        private bool _opened;

        protected override GameObject Construct()
        {
            // Provide an empty layout so children can be added through the fluent API.
            return new LayoutChildren {
                new LayoutChildren
                {
                    new EditorImage { Source = "https://picsum.photos/200" }.AsFlexItem(
                    size: new YogaVector(200, 200)
                ),
                }
                .As<EditorBackground>(x =>
                {
                    x.Source = "#Background4px";
                    x.ImageType = UnityEngine.UI.Image.Type.Sliced;
                })
                .With(x => x.WrappedImage.Attach<ColorSOAttachable>("Button/Background/Normal"))
                .AsFlexGroup(padding: 2f)
                .AsFlexItem(flexGrow: 1f)
            
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
