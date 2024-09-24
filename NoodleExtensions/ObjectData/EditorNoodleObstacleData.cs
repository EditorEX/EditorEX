using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditorEX.NoodleExtensions.ObjectData
{
    internal class EditorNoodleObstacleData : EditorNoodleObjectData
    {
        internal Vector3 InternalBoundsSize { get; set; }

        internal float? Width { get; }

        internal float? Height { get; }

        internal float? Length { get; }

        internal bool InternalDoUnhide { get; set; }

        internal EditorNoodleObstacleData(ObstacleEditorData obstacleData, CustomData customData, Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> beatmapTracks, bool v2, bool leftHanded)
            : base(obstacleData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
        {
            try
            {
                IEnumerable<float?> nullableFloats = customData.GetNullableFloats(v2 ? "_scale" : "size");
                IEnumerable<float?> scale = (nullableFloats != null) ? nullableFloats.ToList() : null;
                Width = (scale != null) ? scale.ElementAtOrDefault(0) : null;
                Height = (scale != null) ? scale.ElementAtOrDefault(1) : null;
                Length = (scale != null) ? scale.ElementAtOrDefault(2) : null;
                if (leftHanded)
                {
                    float width = Width ?? obstacleData.width;
                    if (StartX != null)
                    {
                        StartX = new float?((StartX.Value + width) * -1f);
                    }
                    else if (Width != null)
                    {
                        float lineIndex = obstacleData.column - 2;
                        StartX = new float?(lineIndex - width);
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
        }
    }
}
