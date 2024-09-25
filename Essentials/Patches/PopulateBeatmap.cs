using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using HarmonyLib;
using Zenject;

namespace EditorEX.Essentials.Patches
{
    [HarmonyPatch]
    public static class PopulateBeatmap
    {
        internal static BeatmapObjectsDataModel _beatmapObjectsDataModel;
        internal static BeatmapLevelDataModel _beatmapLevelDataModel;
        internal static BeatmapDataModel _beatmapDataModel;
        internal static AudioDataModel _audioDataModel;
        internal static DiContainer _Container;

        [HarmonyPatch(typeof(BeatmapEditorDataModelsInstaller), nameof(BeatmapEditorDataModelsInstaller.Install))]
        [HarmonyPostfix]
        public static void Postfix(DiContainer Container)
        {
            _Container = Container;
            _beatmapDataModel = Container.Resolve<BeatmapDataModel>();
            _audioDataModel = Container.Resolve<AudioDataModel>();
            _beatmapLevelDataModel = Container.Resolve<BeatmapLevelDataModel>();
        }
    }
}
