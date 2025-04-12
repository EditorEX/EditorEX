using BeatmapEditor3D.DataModels;
using UnityEngine;

namespace BetterEditor.Essentials
{
    public interface IVivifyDataProvider
    {
        BaseEditorData EditorData { get; }
        NoteData.GameplayType GameplayType { get; }
        NoteCutDirection CutDirection { get; }
        Component Component { get; }
        float StartTime { get; }
    }
}