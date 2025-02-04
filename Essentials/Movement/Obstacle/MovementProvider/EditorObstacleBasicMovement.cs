using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace EditorEX.Essentials.Movement.Obstacle.MovementProvider
{
    public class EditorObstacleBasicMovement : MonoBehaviour, IObjectMovement
    {
        private ObstacleEditorData _editorData;

        private BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper;

        [Inject]
        public void Construct(
            BeatmapObjectPlacementHelper beatmapObjectPlacementHelper)
        {
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;
        }

        public void Init(BaseEditorData? editorData, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot)
        {
            _editorData = editorData as ObstacleEditorData;
            float z = _beatmapObjectPlacementHelper.BeatToPosition(editorData.beat);
            transform.localPosition = new Vector3(((float)_editorData.column - 2f + (float)_editorData.width / 2f) * 0.8f, 0.5f + (float)_editorData.row * 0.8f - 0.4f, z);
        }

        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public void Setup(BaseEditorData? editorData)
        {

        }

        public void ManualUpdate()
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.z = _beatmapObjectPlacementHelper.BeatToPosition(_editorData.beat);
            transform.localPosition = localPosition;
        }
    }
}
