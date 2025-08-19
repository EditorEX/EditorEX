using System;
using EditorEX.SDK.ReactiveComponents.Native;
using EditorEX.Util;
using HMUI;
using Reactive;
using Reactive.Components;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorToggle : ReactiveComponent
    {
        public Action<bool> OnClick
        {
            set => _toggle.onValueChanged.AddListener(x => value?.Invoke(x));
        }

        private EditorImage _checkmark = null!;
        private EditorBackground _background = null!;
        private NoTransitionsToggle _toggle = null!;
        private NoTransitionToggleSelectableStateController _selectableStateController = null!;

        protected override GameObject Construct()
        {
            return new LayoutChildren
            {
                new EditorImage() { Source = "#IconCheckmark" }
                    .AsFlexItem(size: 20f)
                    .Bind(ref _checkmark),
            }
                .As<EditorBackground>(x =>
                {
                    x.Source = "#Background4px";
                    x.ImageType = Image.Type.Sliced;
                })
                .AsFlexGroup(padding: 8f)
                .AsFlexItem(size: 36f, maxSize: 36f)
                .Bind(ref _background)
                .WithNativeComponent(out _toggle)
                .Use();
        }

        protected override void OnStart()
        {
            var container = Content
                .transform.GetComponentInParent<ReactiveContainerHolder>()
                .ReactiveContainer;
            Content.SetActive(false);
            _selectableStateController =
                container.Instantiator.InstantiateComponent<NoTransitionToggleSelectableStateController>(
                    Content
                );
            _selectableStateController._component = _toggle;

            var stateTransition =
                _background.Content.gameObject.AddComponent<ColorGraphicStateTransition>();
            stateTransition._transition =
                container.TransitionCollector.GetTransition<ColorTransitionSO>("Button/Background");
            stateTransition._selectableStateController = _selectableStateController;
            stateTransition._component = _background.WrappedImage.ImageView;

            _toggle.graphic = _checkmark.ImageView;
            Content.SetActive(true);
            base.OnStart();
        }
    }
}
