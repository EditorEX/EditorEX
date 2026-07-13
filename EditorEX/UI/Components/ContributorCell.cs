using EditorEX.SDK.ReactiveComponents;
using Reactive;
using UnityEngine;

namespace EditorEX.UI.Components;

public class ContributorCell : ReactiveComponent
{
    protected override GameObject Construct()
    {
        var value = Remember("Hello world!");

        return new EditorLabel { sText = value }.Use();
    }
}
