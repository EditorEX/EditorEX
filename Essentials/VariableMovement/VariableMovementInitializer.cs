using System.Linq;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Patches;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.VariableMovement
{
    public class VariableMovementInitializer
    {
        [Inject]
        public void Construct(
            IVariableMovementDataProvider originalVariableMovementDataProvider,
            PopulateBeatmap populateBeatmap,
            EditorBasicBeatmapObjectSpawnMovementData editorBasicBeatmapObjectSpawnMovementData)
        {
            var num = populateBeatmap._beatmapObjectsDataModel.noteJumpSpeedEvents.Cast<NoteJumpSpeedEditorData>().Aggregate(0f, (current, noteJumpSpeedEventData) => Mathf.Min(current, noteJumpSpeedEventData.noteJumpSpeedDelta));
            originalVariableMovementDataProvider.Init(
                editorBasicBeatmapObjectSpawnMovementData._startHalfJumpDurationInBeats, 
                editorBasicBeatmapObjectSpawnMovementData._maxHalfJumpDistance, 
                editorBasicBeatmapObjectSpawnMovementData.noteJumpMovementSpeed,
                num,
                populateBeatmap._beatmapLevelDataModel.beatsPerMinute,
                editorBasicBeatmapObjectSpawnMovementData._noteJumpValueType,
                editorBasicBeatmapObjectSpawnMovementData._noteJumpValue,
                editorBasicBeatmapObjectSpawnMovementData.centerPos,
                Vector3.forward);
        }
    }
}