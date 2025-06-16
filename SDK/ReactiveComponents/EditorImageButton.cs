using System;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Reactive;
using Reactive.Components;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents
{
    //TODO: Make this use Reactive animated states?
    public class EditorImageButton : ReactiveComponent
    {
        public Action OnClick
        {
            set => _button.onClick.AddListener(() => value?.Invoke());
        }

        public bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        public bool RaycastTarget
        {
            get => _background.RaycastTarget;
            set => _background.RaycastTarget = value;
        }

        public bool Visible
        {
            get => _background.WrappedImage.ImageView.enabled;
            set => _background.WrappedImage.ImageView.enabled = value;
        }

        public string Source
        {
            set => _background.Source = value;
        }

        public NoTransitionsButton Button => _button;

        protected EditorBackground _background = null!;
        protected NoTransitionsButton _button = null!;
        protected NoTransitionButtonSelectableStateController _selectableStateController = null!;

        protected override GameObject Construct()
        {
            return new EditorBackground()
            {
                Source = "#Background4px",
                ImageType = Image.Type.Sliced,
            }
                .AsFlexItem(size: new Reactive.Yoga.YogaVector("fit-content", "fit-content"))
                .Bind(ref _background)
                .WithNativeComponent(out _button)
                .Use();
        }

        protected override void OnStart()
        {
            var container = Content
                .transform.GetComponentInParent<ReactiveContainerHolder>()
                .ReactiveContainer;
            Content.SetActive(false);
            _selectableStateController =
                container.Instantiator.InstantiateComponent<NoTransitionButtonSelectableStateController>(
                    Content
                );
            _selectableStateController._component = _button;

            var stateTransition =
                _background.Content.gameObject.AddComponent<ColorGraphicStateTransition>();
            stateTransition._transition =
                container.TransitionCollector.GetTransition<ColorTransitionSO>("Button/Background");
            stateTransition._selectableStateController = _selectableStateController;
            stateTransition._component = _background.WrappedImage.ImageView;

            Content.SetActive(true);
            base.OnStart();
        }
    }
}
