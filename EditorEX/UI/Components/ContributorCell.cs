using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using UnityEngine;

namespace EditorEX.UI.Components;

public class ContributorCell : ReactiveComponent
{
    protected override GameObject Construct()
    {
        var value = Remember("Hello world!");

        return new Label { sText = value }.Use();
    }
}
