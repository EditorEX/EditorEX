using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;

namespace EditorEX.MapData.SerializedSaveData
{
    public class SerializedCustomLevelInfoSaveData
    {
        public SerializedCustomLevelInfoSaveData(
            string songName,
            string songSubName,
            string songAuthorName,
            string levelAuthorName,
            float beatsPerMinute,
            float songTimeOffset,
            float shuffle,
            float shufflePeriod,
            float previewStartTime,
            float previewDuration,
            string songFilename,
            string coverImageFilename,
            string environmentName,
            string allDirectionsEnvironmentName,
            string[] environmentNames,
            BeatmapLevelColorSchemeSaveData[] colorSchemes,
            SerializedDifficultyBeatmapSet[] difficultyBeatmapSets,
            CustomData customData
        )
        {
            _version = "2.1.0";
            _songName = songName;
            _songSubName = songSubName;
            _songAuthorName = songAuthorName;
            _levelAuthorName = levelAuthorName;
            _beatsPerMinute = beatsPerMinute;
            _songTimeOffset = songTimeOffset;
            _shuffle = shuffle;
            _shufflePeriod = shufflePeriod;
            _previewStartTime = previewStartTime;
            _previewDuration = previewDuration;
            _songFilename = songFilename;
            _coverImageFilename = coverImageFilename;
            _environmentName = environmentName;
            _allDirectionsEnvironmentName = allDirectionsEnvironmentName;
            _environmentNames = environmentNames;
            _colorSchemes = colorSchemes;
            _difficultyBeatmapSets = difficultyBeatmapSets;
            _customData = customData;
        }

        [JsonProperty]
        private string _version;

        [JsonProperty]
        private string _songName;

        [JsonProperty]
        private string _songSubName;

        [JsonProperty]
        private string _songAuthorName;

        [JsonProperty]
        private string _levelAuthorName;

        [JsonProperty]
        private float _beatsPerMinute;

        [JsonProperty]
        private float _songTimeOffset;

        [JsonProperty]
        private float _shuffle;

        [JsonProperty]
        private float _shufflePeriod;

        [JsonProperty]
        private float _previewStartTime;

        [JsonProperty]
        private float _previewDuration;

        [JsonProperty]
        private string _songFilename;

        [JsonProperty]
        private string _coverImageFilename;

        [JsonProperty]
        private string _environmentName;

        [JsonProperty]
        private string _allDirectionsEnvironmentName;

        [JsonProperty]
        private string[] _environmentNames;

        [JsonProperty]
        private BeatmapLevelColorSchemeSaveData[] _colorSchemes;

        [JsonProperty]
        private CustomData _customData;

        [JsonProperty]
        private SerializedDifficultyBeatmapSet[] _difficultyBeatmapSets;

        public class SerializedDifficultyBeatmap
        {
            public SerializedDifficultyBeatmap(
                string difficultyName,
                int difficultyRank,
                string beatmapFilename,
                float noteJumpMovementSpeed,
                float noteJumpStartBeatOffset,
                int beatmapColorSchemeIdx,
                int environmentNameIdx,
                CustomData customData
            )
            {
                _difficulty = difficultyName;
                _difficultyRank = difficultyRank;
                _beatmapFilename = beatmapFilename;
                _noteJumpMovementSpeed = noteJumpMovementSpeed;
                _noteJumpStartBeatOffset = noteJumpStartBeatOffset;
                _beatmapColorSchemeIdx = beatmapColorSchemeIdx;
                _environmentNameIdx = environmentNameIdx;
                _customData = customData;
            }

            [JsonProperty]
            private string _difficulty;

            [JsonProperty]
            private int _difficultyRank;

            [JsonProperty]
            private string _beatmapFilename;

            [JsonProperty]
            private float _noteJumpMovementSpeed;

            [JsonProperty]
            private float _noteJumpStartBeatOffset;

            [JsonProperty]
            private int _beatmapColorSchemeIdx;

            [JsonProperty]
            private int _environmentNameIdx;

            [JsonProperty]
            private CustomData _customData;
        }

        public class SerializedDifficultyBeatmapSet
        {
            public SerializedDifficultyBeatmapSet(
                string beatmapCharacteristicName,
                SerializedDifficultyBeatmap[] difficultyBeatmaps
            )
            {
                _beatmapCharacteristicName = beatmapCharacteristicName;
                _difficultyBeatmaps = difficultyBeatmaps;
            }

            [JsonProperty]
            public string _beatmapCharacteristicName;

            [JsonProperty]
            public SerializedDifficultyBeatmap[] _difficultyBeatmaps;
        }
    }
}
