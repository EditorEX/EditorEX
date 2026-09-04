using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using Heck.Animation;

namespace EditorEX.Heck.Codecs
{
    internal sealed class CustomDataCodecContext
    {
        public Version SourceVersion { get; set; } = new(3, 0, 0);

        public Version TargetVersion { get; set; } = new(3, 0, 0);

        public Dictionary<string, Track> Tracks { get; set; } = new();

        public Dictionary<string, List<object>> PointDefinitions { get; set; } = new();

        public Dictionary<string, CustomEventData> EventDefinitions { get; set; } = new();

        public TrackBuilder? TrackBuilder { get; set; }

        public ICustomDataRepository? Repository { get; set; }

        public object? Extra { get; set; }

        public bool LeftHanded { get; set; }

        public float Bpm { get; set; }

        public bool SourceIsV2 => SourceVersion.Major < 3;

        public bool TargetIsV2 => TargetVersion.Major < 3;
    }
}
