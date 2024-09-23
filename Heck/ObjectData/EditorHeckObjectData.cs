using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterEditor.Heck.ObjectData
{
	internal class EditorHeckObjectData : IObjectCustomData
	{
		internal List<Track> Track { get; }

		internal EditorHeckObjectData(CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
		{
			try
			{
				IEnumerable<Track> nullableTrackArray = customData.GetNullableTrackArray(beatmapTracks, v2);
				Track = ((nullableTrackArray != null) ? nullableTrackArray.ToList<Track>() : null);
			}
			catch (Exception e)
			{
				Plugin.Log.Error(e);
			}
		}
	}
}
