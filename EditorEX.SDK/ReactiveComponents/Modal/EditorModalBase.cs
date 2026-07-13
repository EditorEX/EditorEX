using System;
using JetBrains.Annotations;
using Reactive;
using Reactive.Components;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents;

//Based on Reactive.BeatSaber.Components.ModalBase
public abstract class EditorModalBase : ReactiveComponent
{
    #region Public API

    /// <summary>
    /// Defines placement params for <see cref="PlacementAnchor"/>.
    /// </summary>
    public PlacementData PlacementData { get; set; } =
        new() { Placement = RelativePlacement.Center };

    /// <summary>
    /// An anchor to place against. If not set, <see cref="PlacementData"/> won't do anything.
    /// </summary>
    public RectTransform? PlacementAnchor { get; set; }

    /// <summary>
    /// Called when user clicks outside the modal view.
    /// </summary>
    public Action? OnClickOutside { get; set; }

    #endregion

    #region Overlay API

    /// <inheritdoc cref="Overlay.ZIndex"/>
    public int ZIndex
    {
        get => _overlay.ZIndex;
        set => _overlay.ZIndex = value;
    }

    /// <inheritdoc cref="Overlay.OnPushed"/>
    public Action? OnPushed
    {
        get => _overlay.OnPushed;
        set => _overlay.OnPushed = value;
    }

    /// <inheritdoc cref="Overlay.OnPopped"/>
    public Action? OnPopped
    {
        get => _overlay.OnPopped;
        set => _overlay.OnPopped = value;
    }

    /// <inheritdoc cref="Overlay.IsPushed"/>
    public bool IsPushed
    {
        get => _overlay.IsPushed;
        set
        {
            if (_overlay.IsPushed == value)
            {
                return;
            }

            _overlay.IsPushed = value;

            if (value && PlacementAnchor != null)
            {
                PlacementTool.Place(_content.ContentTransform, PlacementAnchor, PlacementData);
            }
        }
    }

    #endregion

    #region Impl

    private IReactiveComponent _content = null!;
    private Overlay _overlay = null!;
    private EditorImage _blocker = null!;

    protected sealed override GameObject Construct()
    {
        return new Overlay
        {
            Enabled = false,

            Children =
            {
                new EditorImage
                {
                    Name = "Blocker",
                    Sprite = ReactiveResources.TransparentPixel,
                    RaycastTarget = true,
                }
                    .Bind(ref _blocker)
                    .WithRectExpand()
                    .WithPointerEvents(onDown: _ => OnClickOutside?.Invoke()),
                ConstructContent().Bind(ref _content),
            },
        }
            .Bind(ref _overlay)
            .Use();
    }

    protected abstract IReactiveComponent ConstructContent();

    #endregion
}
