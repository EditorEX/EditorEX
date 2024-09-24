using BeatmapEditor3D.DataModels;
using HarmonyLib;
using Zenject;

namespace BetterEditor.Essentials.Patches
{
    [HarmonyPatch]
    public static class PopulateBeatmap
    {
        internal static BeatmapLevelDataModel _beatmapLevelDataModel;
        internal static BeatmapDataModel _beatmapDataModel;
        internal static DiContainer _Container;

        [HarmonyPatch(typeof(BeatmapEditorDataModelsInstaller), nameof(BeatmapEditorDataModelsInstaller.Install))]
        [HarmonyPostfix]
        public static void Postfix(DiContainer Container)
        {
            _Container = Container;
            _beatmapDataModel = Container.Resolve<BeatmapDataModel>();
            _beatmapLevelDataModel = Container.Resolve<BeatmapLevelDataModel>();
        }
    }
}
