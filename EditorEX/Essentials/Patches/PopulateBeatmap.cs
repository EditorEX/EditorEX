using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.Essentials.Patches
{
    [AffinityPatch]
    public class PopulateBeatmap : IAffinity, IEditorBeatmapModels
    {
        public BeatmapObjectsDataModel BeatmapObjectsDataModel { get; private set; }
        public BeatmapLevelDataModel BeatmapLevelDataModel { get; private set; }
        public BeatmapDataModel BeatmapDataModel { get; private set; }
        public AudioDataModel AudioDataModel { get; private set; }

        internal DiContainer _Container;

        [AffinityPatch(
            typeof(BeatmapEditorDataModelsInstaller),
            nameof(BeatmapEditorDataModelsInstaller.Install)
        )]
        [AffinityPostfix]
        private void Postfix(DiContainer Container)
        {
            _Container = Container;
            BeatmapDataModel = Container.Resolve<BeatmapDataModel>();
            AudioDataModel = Container.Resolve<AudioDataModel>();
            BeatmapLevelDataModel = Container.Resolve<BeatmapLevelDataModel>();
            BeatmapObjectsDataModel = Container.Resolve<BeatmapObjectsDataModel>();
        }
    }
}
