using System;
using BeatmapEditor3D.InputSystem;
using BeatmapEditor3D.Utils;
using Zenject;

namespace EditorEX.Essentials.Features.ViewMode
{
    public class ViewModeInputBinder
    {
        private readonly SingleDisposable _singleDisposable = new();

        private ViewModeInputBinder(SignalBus signalBus, InputActionsStreamContainer inputActionsStreamContainer)
        {
            var streamForBindingGroup = inputActionsStreamContainer.GetStreamForBindingGroup(
                InputRef.EssentialsGroup.GetKeyBindingGroupType());

            var compositeDisposable = new CompositeDisposable();
            _singleDisposable.disposable = compositeDisposable;

            for (int i = 0; i < InputRef.ViewModeBindings.Count; i++)
            {
                int index = i;
                streamForBindingGroup.Subscribe(
                    InputRef.ViewModeBindings[i].GetInputAction(),
                    InputEventType.KeyDown,
                    new Action(() => signalBus.Fire(new ViewModeSwitchedSignal(ViewModeRepository.GetViewModes()[index]))))
                    .AddTo(compositeDisposable);
            }
        }

        public void Dispose()
        {
            _singleDisposable?.Dispose();
        }
    }
}