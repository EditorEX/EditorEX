using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using System;
using System.Collections.Generic;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.ObjectData
{
    internal class EditorNoodleBaseNoteData : EditorNoodleObjectData
    {
        internal float? InternalFlipYSide { get; set; }

        internal float? InternalFlipLineIndex { get; set; }

        internal float InternalStartNoteLineLayer { get; set; }

        internal bool DisableGravity { get; }

        internal bool DisableLook { get; }

        internal bool DisableBadCutDirection { get; }

        internal bool DisableBadCutSpeed { get; }

        internal bool DisableBadCutSaberType { get; }

        internal float InternalEndRotation { get; set; }

        internal EditorNoodleBaseNoteData(EditorNoodleBaseNoteData original)
            : base(original)
        {
            DisableGravity = original.DisableGravity;
            DisableLook = original.DisableLook;
        }

        internal EditorNoodleBaseNoteData(BaseEditorData noteData, CustomData customData, Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> beatmapTracks, bool v2, bool leftHanded)
            : base(noteData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
        {
            try
            {
                if (!v2)
                {
                    DisableBadCutDirection = customData.Get<bool?>("disableBadCutDirection").GetValueOrDefault();
                    DisableBadCutSpeed = customData.Get<bool?>("disableBadCutSpeed").GetValueOrDefault();
                    DisableBadCutSaberType = customData.Get<bool?>("disableBadCutSaberType").GetValueOrDefault();
                }
                InternalFlipYSide = customData.Get<float?>("NE_flipYSide");
                InternalFlipLineIndex = customData.Get<float?>("NE_flipLineIndex");
                InternalStartNoteLineLayer = customData.Get<float?>("NE_startNoteLineLayer").GetValueOrDefault();
                DisableGravity = customData.Get<bool?>(v2 ? "_disableNoteGravity" : "disableNoteGravity").GetValueOrDefault();
                DisableLook = customData.Get<bool?>(v2 ? "_disableNoteLook" : "disableNoteLook").GetValueOrDefault();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
