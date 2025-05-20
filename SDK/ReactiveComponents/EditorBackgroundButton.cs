using System;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Reactive;
using Reactive.Components;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorBackgroundButton : ComponentLayout<EditorImageButton>, IComponentHolder<EditorImageButton>
    {
        public Action OnClick
        {
            set => Component.Button.onClick.AddListener(() => value?.Invoke());
        }

        public float Skew
        {
            get => Component.Skew;
            set => Component.Skew = value;
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

        public bool Visible {
            get => Component.Visible;
            set => Component.Visible = value;
        }

        public string Source {
            set => Component.Source = value;
        }
    }
}