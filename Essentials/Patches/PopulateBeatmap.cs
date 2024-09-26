using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using HarmonyLib;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.Essentials.Patches
{
    [AffinityPatch]
    public class PopulateBeatmap : IAffinity
    {
        internal static BeatmapObjectsDataModel _beatmapObjectsDataModel;   
        internal static BeatmapLevelDataModel _beatmapLevelDataModel;
        internal static BeatmapDataModel _beatmapDataModel;
        internal static AudioDataModel _audioDataModel;
        internal static DiContainer _Container;

        [AffinityPatch(typeof(BeatmapEditorDataModelsInstaller), nameof(BeatmapEditorDataModelsInstaller.Install))]
        [AffinityPostfix]
        private void Postfix(DiContainer Container)
        {
            _Container = Container;
            _beatmapDataModel = Container.Resolve<BeatmapDataModel>();
            _audioDataModel = Container.Resolve<AudioDataModel>();
            _beatmapLevelDataModel = Container.Resolve<BeatmapLevelDataModel>();
            _beatmapObjectsDataModel = Container.Resolve<BeatmapObjectsDataModel>();
        }
    }
}
