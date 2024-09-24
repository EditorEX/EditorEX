using BeatmapEditor3D.DataModels;
using BetterEditor.Essentials.Movement.Data;
using UnityEngine;

namespace BetterEditor.Essentials.Movement
{
    public interface IObjectMovement
    {
        void Init(BaseEditorData editorData, EditorBasicBeatmapObjectSpawnMovementData movementData);

        void Setup(BaseEditorData editorData);

        void ManualUpdate();
    }
}