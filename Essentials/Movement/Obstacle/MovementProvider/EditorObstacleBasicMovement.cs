using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using SiraUtil.Logging;
using System;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Obstacle.MovementProvider
{
    public class EditorObstacleBasicMovement : MonoBehaviour, IObjectMovement
    {
        private ObstacleEditorData? _editorData;

        private BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper = null!;
        private SiraLog _siraLog = null!;

        [Inject]
        public void Construct(
            BeatmapObjectPlacementHelper beatmapObjectPlacementHelper,
            SiraLog siraLog)
        {
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;
            _siraLog = siraLog;
        }

        public void Init(BaseEditorData? editorData, IVariableMovementDataProvider variableMovementDataProvider, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals>? getVisualRoot)
        {
            _editorData = editorData as ObstacleEditorData;
            if (_editorData == null)
            {
                _siraLog.Error("EditorObstacleBasicMovement: Null editorData");
                return;
            }
            float z = _beatmapObjectPlacementHelper.BeatToPosition(_editorData.beat);
            transform.localPosition = new Vector3((_editorData.column - 2f + _editorData.width / 2f) * 0.8f, 0.5f + _editorData.row * 0.8f - 0.4f, z);
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
            if (_editorData == null)
            {
                return;
            }
            Vector3 localPosition = transform.localPosition;
            localPosition.z = _beatmapObjectPlacementHelper.BeatToPosition(_editorData.beat);
            transform.localPosition = localPosition;
        }
    }
}
