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
    public class EditorLabelButton : ReactiveComponent, IComponentHolder<EditorLabelButton>
    {
        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public float FontSize
        {
            get => _label.FontSize;
            set =>_label.FontSize = value;
        }

        public Action OnClick
        {
            set => _button.onClick.AddListener(() => value?.Invoke());
        }

        EditorLabelButton IComponentHolder<EditorLabelButton>.Component => this;

        private EditorLabel _label = null!;
        private EditorBackground _background = null!;
        private NoTransitionsButton _button = null!;
        private NoTransitionButtonSelectableStateController _selectableStateController = null!;

        protected override void Construct(RectTransform rect)
        {
            new EditorBackground() {
                Source = "#Background8px",
                ImageType = Image.Type.Sliced,
            }
                .WithRectExpand()
                .Bind(ref _background);
            _background.Use(rect);
            
            new EditorLabel()
                .WithRectExpand()
                .Bind(ref _label);
            _label.Use(rect);

            _button = rect.gameObject.AddComponent<NoTransitionsButton>();

            base.Construct(rect);
        }

        protected override void OnStart()
        {
            var container = Content.transform.GetComponentInParent<ReactiveContainerHolder>().ReactiveContainer;
            Content.SetActive(false);
            _selectableStateController = container.Instantiator.InstantiateComponent<NoTransitionButtonSelectableStateController>(Content);
            _selectableStateController._component = _button;

            var stateTransition = _background.Content.gameObject.AddComponent<ColorGraphicStateTransition>();
            stateTransition._transition = container.TransitionCollector.GetTransition<ColorTransitionSO>("Button/Background");
            stateTransition._selectableStateController = _selectableStateController;
            stateTransition._component = _background.WrappedImage.ImageView;

            var textStateTransition = _label.Content.gameObject.AddComponent<ColorTMPTextStateTransition>();
            textStateTransition._transition = container.TransitionCollector.GetTransition<ColorTransitionSO>("Button/Text");
            textStateTransition._selectableStateController = _selectableStateController;
            textStateTransition._component = _label.TextMesh;
            Content.SetActive(true);
            base.OnStart();
        }
    }
}