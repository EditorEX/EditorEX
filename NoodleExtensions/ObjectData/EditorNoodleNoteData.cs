using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using System;
using System.Collections.Generic;

namespace EditorEX.NoodleExtensions.ObjectData
{
    internal class EditorNoodleNoteData : EditorNoodleBaseNoteData
    {
        internal string Link { get; }

        internal EditorNoodleNoteData(NoteEditorData noteData, CustomData customData, Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> beatmapTracks, bool v2, bool leftHanded)
            : base(noteData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
        {
            try
            {
                if (v2)
                {
                    float? cutDir = customData.Get<float?>("_cutDirection");
                    if (cutDir != null)
                    {
                        noteData.SetField("angle", (int)cutDir.Value.Mirror(leftHanded));
                        if (noteData.cutDirection != NoteCutDirection.Any)
                        {
                            noteData.SetField("cutDirection", NoteCutDirection.Down);
                        }
                    }
                }
                else
                {
                    Link = customData.Get<string>("link");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
        }
    }
}
