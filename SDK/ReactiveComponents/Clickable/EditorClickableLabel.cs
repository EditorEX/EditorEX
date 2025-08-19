using EditorEX.SDK.Components;
using EditorEX.SDK.ReactiveComponents.Native;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorClickableLabel : EditorLabel
    {
        protected override void Construct(RectTransform rect)
        {
            _text = rect.gameObject.AddComponent<EditorClickableText>();
            _text.RegisterDirtyLayoutCallback(RequestLeafRecalculation);
        }

        protected override void OnStart()
        {
            var container = Content
                .transform.GetComponentInParent<ReactiveContainerHolder>()
                .ReactiveContainer;
            Content.SetActive(false);

            var selectableStateController =
                container.Instantiator.InstantiateComponent<NoTransitionClickableTextSelectableStateController>(
                    Content
                );
            selectableStateController._component = (_text as EditorClickableText)!;

            var transition = Content.AddComponent<ColorTMPTextStateTransition>();
            transition._transition = container.TransitionCollector.GetTransition<ColorTransitionSO>(
                "Button/Text"
            );
            transition._selectableStateController = selectableStateController;
            transition._component = (_text as EditorClickableText)!;

            Content.SetActive(true);
            base.OnStart();
        }
    }
}
