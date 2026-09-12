using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using NoodleExtensions;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.ObjectData
{
    internal class EditorNoodleBaseNoteData : EditorNoodleObjectData
    {
        internal float? FlipX { get; }

        internal float? FlipY { get; }

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
            FlipX = original.FlipX;
            FlipY = original.FlipY;
        }

        internal EditorNoodleBaseNoteData(
            BaseEditorData? noteData,
            CustomData customData,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2,
            bool leftHanded
        )
            : base(noteData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
        {
            try
            {
                if (!v2)
                {
                    DisableBadCutDirection = customData
                        .Get<bool?>("disableBadCutDirection")
                        .GetValueOrDefault();
                    DisableBadCutSpeed = customData
                        .Get<bool?>("disableBadCutSpeed")
                        .GetValueOrDefault();
                    DisableBadCutSaberType = customData
                        .Get<bool?>("disableBadCutSaberType")
                        .GetValueOrDefault();
                }
                IEnumerable<float?>? flip = customData
                    .GetNullableFloats(v2 ? NoodleController.V2_FLIP : NoodleController.FLIP)
                    ?.ToList();
                FlipX = flip?.ElementAtOrDefault(0);
                FlipY = flip?.ElementAtOrDefault(1);
                DisableGravity = customData
                    .Get<bool?>(v2 ? "_disableNoteGravity" : "disableNoteGravity")
                    .GetValueOrDefault();
                DisableLook = customData
                    .Get<bool?>(v2 ? "_disableNoteLook" : "disableNoteLook")
                    .GetValueOrDefault();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
