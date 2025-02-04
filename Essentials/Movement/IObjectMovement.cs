using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using System;
using UnityEngine;

namespace EditorEX.Essentials.Movement
{
    public interface IObjectMovement : IObjectComponent
    {
        void Init(BaseEditorData? editorData, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot);

        void Setup(BaseEditorData? editorData);

        void ManualUpdate();
    }
}