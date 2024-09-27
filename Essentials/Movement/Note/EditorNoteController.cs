using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.ViewMode;
using EditorEX.Essentials.Visuals;
using System;
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

        private T GetProvidedComponent<T>(ITypeProvider typeProvider)
        {
            var components = gameObject?.GetComponents<T>();
            var types = components?.Select(x => x.GetType())?.ToArray();
            var type = typeProvider.GetProvidedType(types, false);

            return (T)Convert.ChangeType(GetComponent(type), typeof(T));
        }

        private void RefreshNoteMovementVisuals()
        {
            _noteMovement = GetProvidedComponent<IObjectMovement>(_movementTypeProvider);
            _noteVisuals = GetProvidedComponent<IObjectVisuals>(_visualsTypeProvider);
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
