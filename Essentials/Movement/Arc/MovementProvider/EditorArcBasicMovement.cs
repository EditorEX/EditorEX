using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using BetterEditor.Essentials.Movement.Data;
using IPA.Config.Data;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Zenject;

namespace BetterEditor.Essentials.Movement.Arc.MovementProvider
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

        public void Init(BaseEditorData editorData, EditorBasicBeatmapObjectSpawnMovementData movementData)
        {
            _editorData = editorData as BaseSliderEditorData;
            Vector3 localPosition = transform.localPosition;
            localPosition.z = _beatmapObjectPlacementHelper.BeatToPosition(editorData.beat);
            transform.localPosition = localPosition;

            var material = GetComponent<ArcView>()._arcMeshController.GetComponent<MeshRenderer>().sharedMaterial;

            if (!material.enabledKeywords.Any(x => x.name == "BEATMAP_EDITOR_ONLY"))
            {
                var list = material.enabledKeywords.ToList();
                list.Add(new LocalKeyword(material.shader, "BEATMAP_EDITOR_ONLY"));
                material.enabledKeywords = list.ToArray();
            }
        }

        public void Setup(BaseEditorData editorData)
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
