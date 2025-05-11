using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.Essentials.Patches
{
    [AffinityPatch]
    public class PopulateBeatmap : IAffinity
    {
        internal BeatmapObjectsDataModel _beatmapObjectsDataModel;
        internal BeatmapLevelDataModel _beatmapLevelDataModel;
        internal BeatmapDataModel _beatmapDataModel;
        internal AudioDataModel _audioDataModel;
        internal DiContainer _Container;

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
