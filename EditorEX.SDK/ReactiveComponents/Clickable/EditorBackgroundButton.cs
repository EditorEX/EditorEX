using System;
using Reactive.Components;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorBackgroundButton : ComponentLayout<EditorImageButton>
    {
        public Action OnClick
        {
            set => Component.Button.onClick.AddListener(() => value?.Invoke());
        }

        public bool Interactable
        {
            get => Component.Interactable;
            set => Component.Interactable = value;
        }

        public bool RaycastTarget
        {
            get => Component.RaycastTarget;
            set => Component.RaycastTarget = value;
        }

        public bool Visible
        {
            get => Component.Visible;
            set => Component.Visible = value;
        }

        public string Source
        {
            set => Component.Source = value;
        }

        public new EditorImageButton Component => base.Component;
    }
}
