using System;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.ChainHead.MovementProvider
{
    public class EditorChainHeadBasicMovement : MonoBehaviour, IObjectMovement
    {
        private ChainEditorData _editorData;

        private BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper;

        [Inject]
        public void Construct(BeatmapObjectPlacementHelper beatmapObjectPlacementHelper)
        {
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;
        }

        public void Init(BaseEditorData? editorData, IVariableMovementDataProvider variableMovementDataProvider, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot)
        {
            if (editorData == null || editorData is not ChainEditorData)
            {
                throw new ArgumentNullException(nameof(editorData));
            }
            _editorData = editorData as ChainEditorData;
            Vector3 localPosition = transform.localPosition;
            localPosition.z = _beatmapObjectPlacementHelper.BeatToPosition(editorData.beat);
            transform.localPosition = localPosition;
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