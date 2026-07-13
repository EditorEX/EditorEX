using System.Collections.Generic;
using Reactive;

namespace EditorEX.SDK.ReactiveComponents;

// Based on Reactive.BeatSaber.Components.Modal
public class EditorModal : EditorModalBase, ILayoutDriver
{
    #region Impl

    public ICollection<ILayoutItem> Children => _content.Children;

    public ILayoutController? LayoutController
    {
        get => _content.LayoutController;
        set => _content.LayoutController = value;
    }

    private Layout _content = null!;

    protected override IReactiveComponent ConstructContent()
    {
        return new Layout
        {
            Name = "Content",

            FlexController = { ConstrainHorizontal = false, ConstrainVertical = false },
        }.Bind(ref _content);
    }

    #endregion
}
