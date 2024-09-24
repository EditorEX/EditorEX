using BeatmapEditor3D.DataModels;
using BetterEditor.Essentials.SpawnProcessing;
using SiraUtil.Affinity;

namespace BetterEditor.Essentials.Patches
{
    internal class ProcessNewEditorData : IAffinity
    {
        private EditorBeatmapObjectsInTimeRowProcessor _processor;

        public ProcessNewEditorData(EditorBeatmapObjectsInTimeRowProcessor processor)
        {
            _processor = processor;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>), nameof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>.Add))]
        private void AddData()
        {
            _processor.Reset();
            _processor.Construct();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>), nameof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>.Add))]
        private void RemoveData(BaseEditorData data)
        {
            EditorSpawnDataRepository.RemoveSpawnData(data);
            _processor.Reset();
            _processor.Construct();
        }
    }
}
