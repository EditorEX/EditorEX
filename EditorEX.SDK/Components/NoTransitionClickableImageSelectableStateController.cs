namespace EditorEX.SDK.Components
{
    public class NoTransitionClickableImageSelectableStateController
        : SelectableStateController<EditorNativeClickableImage>
    {
        protected void OnEnable()
        {
            _component.selectionStateDidChangeEvent +=
                HandleNoTransitionButtonSelectionStateDidChange;
            ResolveSelectionState(_component.state, false);
        }

        protected void OnDisable()
        {
            _component.selectionStateDidChangeEvent -=
                HandleNoTransitionButtonSelectionStateDidChange;
        }

        private void HandleNoTransitionButtonSelectionStateDidChange(
            EditorNativeClickableImage.SelectionState state
        )
        {
            ResolveSelectionState(state);
        }

        private void ResolveSelectionState(
            EditorNativeClickableImage.SelectionState state,
            bool animated = true
        )
        {
            switch (state)
            {
                case EditorNativeClickableImage.SelectionState.Highlighted:
                    SetState(ViewState.Highlighted, animated);
                    return;
                case EditorNativeClickableImage.SelectionState.Pressed:
                    SetState(ViewState.Pressed, animated);
                    return;
                case EditorNativeClickableImage.SelectionState.Disabled:
                    SetState(ViewState.Disabled, animated);
                    return;
                default:
                    SetState(ViewState.Normal, animated);
                    return;
            }
        }
    }
}
