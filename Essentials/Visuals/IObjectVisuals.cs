using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using UnityEngine;

namespace EditorEX.Essentials.Visuals
{
    public interface IObjectVisuals
    {
        void Init(BaseEditorData editorData);

        void Enable();

        void Disable();

        void ManualUpdate();
    }
}