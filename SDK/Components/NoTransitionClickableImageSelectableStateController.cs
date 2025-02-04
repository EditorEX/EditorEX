namespace EditorEX.SDK.Components
{
    public class NoTransitionClickableImageSelectableStateController : SelectableStateController<EditorClickableImage>
    {
        protected void OnEnable()
        {
            _component.selectionStateDidChangeEvent += HandleNoTransitionButtonSelectionStateDidChange;
            ResolveSelectionState(_component.state, false);
        }

        protected void OnDisable()
        {
            _component.selectionStateDidChangeEvent -= HandleNoTransitionButtonSelectionStateDidChange;
        }

        private void HandleNoTransitionButtonSelectionStateDidChange(EditorClickableImage.SelectionState state)
        {
            ResolveSelectionState(state);
        }

        private void ResolveSelectionState(EditorClickableImage.SelectionState state, bool animated = true)
        {
            switch (state)
            {
                case EditorClickableImage.SelectionState.Highlighted:
                    SetState(ViewState.Highlighted, animated);
                    return;
                case EditorClickableImage.SelectionState.Pressed:
                    SetState(ViewState.Pressed, animated);
                    return;
                case EditorClickableImage.SelectionState.Disabled:
                    SetState(ViewState.Disabled, animated);
                    return;
                default:
                    SetState(ViewState.Normal, animated);
                    return;
            }
        }
    }
}
