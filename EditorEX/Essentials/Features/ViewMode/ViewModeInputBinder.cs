using System;
using BeatmapEditor3D.InputSystem;
using BGLib.DotnetExtension.Disposables;
using Zenject;

namespace EditorEX.Essentials.Features.ViewMode
{
    public class ViewModeInputBinder : IDisposable
    {
        private readonly SingleDisposable _singleDisposable = new();

        public ViewModeInputBinder(
            SignalBus signalBus,
            InputActionsStreamContainer inputActionsStreamContainer
        )
        {
            var streamForBindingGroup = inputActionsStreamContainer.GetStreamForBindingGroup(
                InputRef.EssentialsGroup!.GetKeyBindingGroupType()
            );

            var compositeDisposable = new CompositeDisposable();
            _singleDisposable.disposable = compositeDisposable;

            streamForBindingGroup
                .Subscribe(
                    InputRef.ShiftNextViewingMode!.GetInputAction(),
                    InputEventType.KeyDown,
                    () => signalBus.Fire(new ShiftNextViewingModeSignal())
                )
                .AddTo(compositeDisposable);
            streamForBindingGroup
                .Subscribe(
                    InputRef.ShiftPreviousViewingMode!.GetInputAction(),
                    InputEventType.KeyDown,
                    () => signalBus.Fire(new ShiftPreviousViewingModeSignal())
                )
                .AddTo(compositeDisposable);
        }

        public void Dispose()
        {
            _singleDisposable.Dispose();
        }
    }
}
