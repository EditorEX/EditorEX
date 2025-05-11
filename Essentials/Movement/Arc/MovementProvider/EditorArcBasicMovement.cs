using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using BetterEditor.Util;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using System;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Arc.MovementProvider
{
    public class EditorArcBasicMovement : MonoBehaviour, IObjectMovement
    {
        private BaseSliderEditorData _editorData;

        private BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper;

        [Inject]
        public void Construct(BeatmapObjectPlacementHelper beatmapObjectPlacementHelper)
        {
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;
        }

        public void Init(BaseEditorData? editorData, IVariableMovementDataProvider variableMovementDataProvider, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot)
        {
            _editorData = editorData as BaseSliderEditorData;
            Vector3 localPosition = transform.localPosition;
            localPosition.z = _beatmapObjectPlacementHelper.BeatToPosition(editorData.beat);
            transform.localPosition = localPosition;

            var material = GetComponent<ArcView>()._arcMeshController.GetComponent<MeshRenderer>().sharedMaterial;

            material.enabledKeywords.DisableGameArc(material);
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
