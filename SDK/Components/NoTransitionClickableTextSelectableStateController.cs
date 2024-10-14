namespace EditorEX.SDK.Components
{
    public class NoTransitionClickableTextSelectableStateController : SelectableStateController<EditorClickableText>
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

        private void HandleNoTransitionButtonSelectionStateDidChange(EditorClickableText.SelectionState state)
        {
            ResolveSelectionState(state, true);
        }

        private void ResolveSelectionState(EditorClickableText.SelectionState state, bool animated = true)
        {
            switch (state)
            {
                case EditorClickableText.SelectionState.Highlighted:
                    SetState(ViewState.Highlighted, animated);
                    return;
                case EditorClickableText.SelectionState.Pressed:
                    SetState(ViewState.Pressed, animated);
                    return;
                case EditorClickableText.SelectionState.Disabled:
                    SetState(ViewState.Disabled, animated);
                    return;
                default:
                    SetState(ViewState.Normal, animated);
                    return;
            }
        }
    }
}
