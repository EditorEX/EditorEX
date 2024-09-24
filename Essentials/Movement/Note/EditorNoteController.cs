using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BetterEditor.Essentials.Movement.Data;
using BetterEditor.Essentials.Movement.Note.MovementProvider;
using BetterEditor.Essentials.SpawnProcessing;
using BetterEditor.Essentials.ViewMode;
using BetterEditor.Heck.Deserializer;
using BetterEditor.NoodleExtensions.ObjectData;
using IPA.Utilities;
using NoodleExtensions.Animation;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BetterEditor.Essentials.Movement.Note
{
    internal class EditorNoteController : MonoBehaviour, IDisposable
    {
        private IReadonlyBeatmapState _state;

        private IObjectMovement _noteMovement;
        private MovementTypeProvider _movementTypeProvider;
        private ActiveViewMode _activeViewMode;

        private NoteEditorData _data;

        private EditorBasicBeatmapObjectSpawnMovementData _movementData;

        [Inject]
        private void Construct(IReadonlyBeatmapState state, ActiveViewMode activeViewMode, MovementTypeProvider movementTypeProvider, EditorBasicBeatmapObjectSpawnMovementData movementData)
        {
            _state = state;
            _movementTypeProvider = movementTypeProvider;
            _movementData = movementData;

            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += RefreshNoteMovementAndInit;
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= RefreshNoteMovementAndInit;
        }

        private void RefreshNoteMovementAndInit()
        {
            try
            {
                RefreshNoteMovement();
                Init(_data);
            }
            catch
            {

            }
        }

        private void RefreshNoteMovement()
        {
            var components = gameObject?.GetComponents<IObjectMovement>();
            if (components == null) return;
            var types = components?.Select(x => x.GetType())?.ToArray();
            var type = _movementTypeProvider.GetNoteMovement(types);

            if (_noteMovement != null && type == _noteMovement.GetType())
            {
                return; // No need to refresh if the type is the same
            }

            _noteMovement = GetComponent(type) as IObjectMovement;
        }

        public void Init(NoteEditorData noteData)
        {
            if (noteData == null) return;
            _data = noteData;

            RefreshNoteMovement();

            _noteMovement.Init(noteData, _movementData);

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
                RefreshNoteMovementAndInit();
            }

            _noteMovement.Setup(_data);

            _noteMovement.ManualUpdate();
        }
    }
}
