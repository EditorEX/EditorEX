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
        private IReadonlyBeatmapState _state;

        private IObjectMovement _noteMovement;
        private IObjectVisuals _noteVisuals;
        private MovementTypeProvider _movementTypeProvider;
        private VisualsTypeProvider _visualsTypeProvider;
        private ActiveViewMode _activeViewMode;

        private NoteEditorData _data;

        private EditorBasicBeatmapObjectSpawnMovementData _movementData;

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

        private bool GetProvidedComponent<T>(ITypeProvider typeProvider, T existing, out T newComponent) where T : IObjectComponent
        {
            var components = gameObject?.GetComponents<T>();
            var types = components?.Select(x => x.GetType())?.ToArray();
            var type = typeProvider.GetProvidedType(types, false);

            T newToUse = (T)Convert.ChangeType(GetComponent(type), typeof(T));

            // We can't use != with generic values, can't use a type constraint for IEquatible as then the interface would need to inherit such also.
            if (!EqualityComparer<T>.Default.Equals(existing, newToUse))
            {
                existing.Disable();
                newComponent = newToUse;
                return true;
            }

            newComponent = default;
            return false;
        }

        private void RefreshNoteMovementVisuals()
        {
            if (GetProvidedComponent(_movementTypeProvider, _noteMovement, out IObjectMovement newNoteMovement))
            {
                _noteMovement = newNoteMovement;
                _noteMovement.Enable();
            }

            if (GetProvidedComponent(_visualsTypeProvider, _noteVisuals, out IObjectVisuals newNoteVisuals))
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

            _noteMovement.Init(noteData, _movementData);

            _noteVisuals.Init(noteData);

            ManualUpdate();
        }

        float _prevBeat = 9999f;

        public void Update()
        {
            if (!_state.isPlaying && _prevBeat == _state.beat) return;
            if (_prevBeat > _state.beat)
            {
                Init(_data);
            }
            _prevBeat = _state.beat;

            ManualUpdate();
        }

        public void ManualUpdate()
        {
            if (_noteMovement == null)
            {
                RefreshNoteMovementVisualsAndInit();
            }

            _noteMovement.Setup(_data);

            _noteMovement.ManualUpdate();

            _noteMovement.ManualUpdate();
        }
    }
}
