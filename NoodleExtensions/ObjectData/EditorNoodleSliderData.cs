using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Deserialize;
using IPA.Utilities;
using NoodleExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.ObjectData
{
    internal class EditorNoodleSliderData : EditorNoodleBaseNoteData, ICopyable<IObjectCustomData>
    {
        internal float? TailStartX { get; }

        internal float? TailStartY { get; }

        internal float InternalTailStartNoteLineLayer { get; }

        public IObjectCustomData Copy()
        {
            return new EditorNoodleBaseNoteData(this);
        }

        internal EditorNoodleSliderData(ArcEditorData? sliderData, CustomData customData, Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> beatmapTracks, bool v2, bool leftHanded)
            : base(sliderData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
        {
            try
            {
                InternalTailStartNoteLineLayer = customData.Get<float?>("NE_tailStartNoteLineLayer").GetValueOrDefault();
                IEnumerable<float?> nullableFloats = customData.GetNullableFloats("tailCoordinates");
                IEnumerable<float?> position = ((nullableFloats != null) ? nullableFloats.ToList() : null);
                TailStartX = ((position != null) ? position.ElementAtOrDefault(0) : null);
                TailStartY = ((position != null) ? position.ElementAtOrDefault(1) : null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}