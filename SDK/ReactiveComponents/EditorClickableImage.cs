using EditorEX.SDK.Components;
using EditorEX.SDK.ReactiveComponents.Native;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorClickableImage : EditorImage
    {
        protected override void Construct(RectTransform rect)
        {
            _image = rect.gameObject.AddComponent<EditorNativeClickableImage>();
        }

        protected override void OnStart()
        {
            var container = Content.transform.GetComponentInParent<ReactiveContainerHolder>().ReactiveContainer;
            Content.SetActive(false);

            var selectableStateController = container.Instantiator.InstantiateComponent<NoTransitionClickableImageSelectableStateController>(Content);
            selectableStateController._component = (ImageView as EditorNativeClickableImage)!;

            var transition = Content.AddComponent<ColorGraphicStateTransition>();
            transition._transition = container.TransitionCollector.GetTransition<ColorTransitionSO>("ClickableImage/Image");
            transition._selectableStateController = selectableStateController;
            transition._component = (ImageView as EditorNativeClickableImage)!;

            Content.SetActive(true);
            base.OnStart();
        }
    }
}