using BeatmapLevelSaveDataVersion4;
using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;
using System;

namespace EditorEX.MapData.SerializedSaveData
{
    public class SerializedCustomBeatmapLevelSaveData
    {
        public string version = "4.0.0";

        public SongData song;

        public AudioData audio;

        public string songPreviewFilename;

        public string coverImageFilename;

        public string[] environmentNames;

        public ColorScheme[] colorSchemes;

        public CustomData customData;

        public DifficultyBeatmap[] difficultyBeatmaps;

        [Serializable]
        public struct SongData
        {
            public string title;

            public string subTitle;

            public string author;
        }

        [Serializable]
        public struct AudioData
        {
            public string songFilename;

            public float songDuration;

            public string audioDataFilename;

            public float bpm;

            public float lufs;

            public float previewStartTime;

            public float previewDuration;
        }

        [Serializable]
        public class ColorScheme
        {
            public string colorSchemeName;

            public string saberAColor;

            public string saberBColor;

            public string obstaclesColor;

            public string environmentColor0;

            public string environmentColor1;

            public string environmentColor0Boost;

            public string environmentColor1Boost;
        }

        [Serializable]
        public struct BeatmapAuthors
        {
            public string[] mappers;

            public string[] lighters;
        }

        [Serializable]
        public class DifficultyBeatmap
        {
            public string characteristic;

            public string difficulty;

            public BeatmapLevelSaveData.BeatmapAuthors beatmapAuthors;

            public int environmentNameIdx;

            public int beatmapColorSchemeIdx;

            public float noteJumpMovementSpeed;

            public float noteJumpStartBeatOffset;

            public string lightshowDataFilename;

            public string beatmapDataFilename;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public CustomData customData;
        }
    }
}
