using CustomJSONData.CustomBeatmap;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;

namespace EditorEX.UI.Patches
{
    // Reads/writes the per-difficulty custom label against the level's custom data model,
    // keyed by the version-appropriate custom data key.
    internal class DifficultyLabelStore
    {
        private readonly ILevelCustomDataModel _levelCustomDataModel;

        public DifficultyLabelStore(ILevelCustomDataModel levelCustomDataModel)
        {
            _levelCustomDataModel = levelCustomDataModel;
        }

        private static string DifficultyLabelKey =>
            LevelContext.Version.Major >= 4 ? "difficultyLabel" : "_difficultyLabel";

        public string? Read(string filename)
        {
            var datas = _levelCustomDataModel.BeatmapCustomDatasByFilename;
            if (datas != null && datas.TryGetValue(filename, out var cd) && cd != null)
                return cd.Get<string>(DifficultyLabelKey);
            return null;
        }

        public void Write(string filename, string value)
        {
            var datas = _levelCustomDataModel.BeatmapCustomDatasByFilename;
            if (datas == null)
                return;
            if (!datas.TryGetValue(filename, out var cd) || cd == null)
            {
                cd = new CustomData();
                datas[filename] = cd;
            }
            if (string.IsNullOrWhiteSpace(value))
                cd.TryRemove(DifficultyLabelKey, out _);
            else
                cd[DifficultyLabelKey] = value;
        }
    }
}
