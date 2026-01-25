using System;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Note.MovementProvider
{
    public class EditorNoteBasicMovement : MonoBehaviour, IObjectMovement
    {
        private NoteEditorData? _editorData;

        private BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper = null!;

        [Inject]
        public void Construct(BeatmapObjectPlacementHelper beatmapObjectPlacementHelper)
        {
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;
        }

        public void Init(
            BaseEditorData? editorData,
            IVariableMovementDataProvider variableMovementDataProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData,
            Func<IObjectVisuals>? getVisualRoot
        )
        {
            _editorData = editorData as NoteEditorData;
            if (editorData == null)
                return;
            float z = _beatmapObjectPlacementHelper.BeatToPosition(editorData.beat);
            transform.localPosition = new Vector3(
                (_editorData.column - 1.5f) * 0.8f,
                0.5f + _editorData.row * 0.8f,
                z
            );
        }

        public void Enable() { }

        public void Disable() { }

        public void Setup(BaseEditorData? editorData) { }

        public void ManualUpdate()
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.z = _beatmapObjectPlacementHelper.BeatToPosition(_editorData.beat);
            transform.localPosition = localPosition;
        }
    }
}
