using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Heck.Deserialize;

namespace EditorEX.Heck.ObjectData
{
    internal class EditorHeckObjectData : IObjectCustomData
    {
        internal List<Track> Track { get; }

        internal EditorHeckObjectData(
            CustomData customData,
            Dictionary<string, Track> beatmapTracks,
            bool v2
        )
        {
            try
            {
                IEnumerable<Track> nullableTrackArray = customData.GetNullableTrackArray(
                    beatmapTracks,
                    v2
                );
                Track = nullableTrackArray?.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
