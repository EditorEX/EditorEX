using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Reactive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents.SegmentedControl
{
    public class EditorSegmentedControlButton(int position) : ReactiveComponent
    {
        public string Text
        {
            get => _label.Text;
            set
            {
                _label.Text = value;
                NotifyPropertyChanged();
            }
        }

        private EditorLabel _label = null!;
        private EditorBackground _background = null!;
        private NoTransitionsButton _button = null!;
        private NoTransitionButtonSelectableStateController _selectableStateController = null!;

        protected override void Construct(RectTransform rect)
        {
            string source = position switch
            {
                0 => "#Background8pxLeft",
                1 => "#WhitePixel",
                2 => "#Background8pxLeft",
                _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
            };

            new EditorBackground() {
                    Source = source,
                    ImageType = Image.Type.Sliced,
                }.WithRectExpand()
                .Bind(ref _background)
                .Use(rect);
            
            new EditorLabel() {
                    FontSize = 10,
                }
                .AsFlexItem(size: "auto")
                .Bind(ref _label)
                .Use(rect);

            if (position == 2)
                _background.ContentTransform.localScale = new Vector3(-1, 1, 1);

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
            stateTransition._transition = container.TransitionCollector.GetTransition<ColorTransitionSO>("SelectableCell/Background");
            stateTransition._selectableStateController = _selectableStateController;
            stateTransition._component = _background.WrappedImage.ImageView;

            Content.SetActive(true);
            base.OnStart();
        }
    }
}