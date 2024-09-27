using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.ViewMode;
using EditorEX.Essentials.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Note
{
    internal class EditorNoteController : MonoBehaviour, IDisposable
    {
        private IObjectMovement _noteMovement;
        private IObjectVisuals _noteVisuals;

        private IReadonlyBeatmapState _state;
        private MovementTypeProvider _movementTypeProvider;
        private VisualsTypeProvider _visualsTypeProvider;
        private ActiveViewMode _activeViewMode;
        private EditorBasicBeatmapObjectSpawnMovementData _movementData;

        private NoteEditorData _data;

        [Inject]
        private void Construct(
            IReadonlyBeatmapState state,
            ActiveViewMode activeViewMode,
            MovementTypeProvider movementTypeProvider,
            VisualsTypeProvider visualsTypeProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData)
        {
            _state = state;
            _movementTypeProvider = movementTypeProvider;
            _visualsTypeProvider = visualsTypeProvider;
            _movementData = movementData;

            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += RefreshNoteMovementVisualsAndInit;
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= RefreshNoteMovementVisualsAndInit;
        }

        private void RefreshNoteMovementVisualsAndInit()
        {
            try
            {
                RefreshNoteMovementVisuals();
                Init(_data);
            }
            catch
            {

            }
        }

        private void RefreshNoteMovementVisuals()
        {
            if (TypeProviderUtils.GetProvidedComponent(gameObject, _movementTypeProvider, _noteMovement, out IObjectMovement newNoteMovement))
            {
                _noteMovement = newNoteMovement;
                _noteMovement.Enable();
            }

            if (TypeProviderUtils.GetProvidedComponent(gameObject, _visualsTypeProvider, _noteVisuals, out IObjectVisuals newNoteVisuals))
            {
                _noteVisuals = newNoteVisuals;
                _noteVisuals.Enable();
            }
        }

        public void Init(NoteEditorData noteData)
        {
            if (noteData == null) return;
            _data = noteData;

            RefreshNoteMovementVisuals();

            _noteMovement.Init(noteData, _movementData, () => _noteVisuals);
            _noteVisuals.Init(noteData);

            ManualUpdate();
        }

        // Use our own prevBeat field as _state.prevBeat only updates when playing or scrubbing which will cause constant updates after scrubbing while paused.
        float _prevBeat = 9999f;

        public void Update()
        {
            if (!_state.isPlaying && _prevBeat == _state.beat) return; //Don't update if not playing for performance, but force an update if scrubbing manually.

            // If we rewind we should reinit the note to stop issues
            if (_prevBeat > _state.beat)
            {
                Init(_data);
            }

            _prevBeat = _state.beat;

            ManualUpdate();
        }

        public void ManualUpdate()
        {
            if (_noteMovement == null || _noteVisuals == null)
            {
                RefreshNoteMovementVisualsAndInit();
            }

            _noteMovement.Setup(_data);

            _noteMovement.ManualUpdate();
            _noteVisuals.ManualUpdate();
        }
    }
}
