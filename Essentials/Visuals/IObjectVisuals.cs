using BeatmapEditor3D.DataModels;
using UnityEngine;

namespace EditorEX.Essentials.Visuals
{
    public interface IObjectVisuals : IObjectComponent
    {
        void Init(BaseEditorData? editorData);

        void ManualUpdate();

        GameObject GetVisualRoot();
    }
}
