using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using UnityEngine;

namespace EditorEX.Essentials.Movement
{
    public interface IObjectMovement
    {
        void Init(BaseEditorData editorData, EditorBasicBeatmapObjectSpawnMovementData movementData);

        void Setup(BaseEditorData editorData);

        void ManualUpdate();
    }
}