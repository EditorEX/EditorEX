using System;
using BeatmapEditor3D.InputSystem;
using BeatmapEditor3D.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace BetterEditor.Essentials.Features.HideUI
{
    public class HideUIInputBinder : IDisposable
    {
        private readonly SingleDisposable _singleDisposable = new();

        private HideUIInputBinder(SignalBus signalBus, InputActionsStreamContainer inputActionsStreamContainer)
        {
            var streamForBindingGroup = inputActionsStreamContainer.GetStreamForBindingGroup(
                InputRef.EssentialsGroup.GetKeyBindingGroupType());
                
            var compositeDisposable = new CompositeDisposable();
            _singleDisposable.disposable = compositeDisposable;

            streamForBindingGroup.Subscribe(
                InputRef.ToggleEditorGUI.GetInputAction(), InputEventType.KeyDown, new Action(signalBus.Fire<HideUIFeatureToggledSignal>)).AddTo(compositeDisposable);
        }

        public void Dispose()
        {
            _singleDisposable?.Dispose();
        }
    }
}