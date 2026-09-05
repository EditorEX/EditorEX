using System;

namespace EditorEX.UI.Patches
{
    public enum NewMapFormatPreset
    {
        V2 = 0,
        V3 = 1,
        V4 = 2,
    }

    public readonly struct NewMapFormat
    {
        public NewMapFormat(Version infoVersion, Version beatmapVersion)
        {
            InfoVersion = infoVersion;
            BeatmapVersion = beatmapVersion;
        }

        public Version InfoVersion { get; }

        public Version BeatmapVersion { get; }

        public static NewMapFormat FromPreset(NewMapFormatPreset preset)
        {
            return preset switch
            {
                NewMapFormatPreset.V2 => new NewMapFormat(
                    new Version(2, 1, 0),
                    new Version(2, 6, 0)
                ),
                NewMapFormatPreset.V3 => new NewMapFormat(
                    new Version(2, 1, 0),
                    new Version(3, 3, 0)
                ),
                NewMapFormatPreset.V4 => new NewMapFormat(
                    new Version(4, 0, 0),
                    new Version(4, 0, 0)
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, null),
            };
        }

        public static Version? ResolveBeatmapVersion(
            Version? stamped,
            Version? mapContext,
            Version? levelContext
        )
        {
            if (stamped != null)
            {
                return stamped;
            }

            if (mapContext != null)
            {
                return mapContext;
            }

            if (levelContext != null && levelContext.Major < 4)
            {
                return new Version(3, 3, 0);
            }

            return null;
        }
    }
}
