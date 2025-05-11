using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.SpawnProcessing;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.Essentials.Patches
{
    internal class ProcessNewEditorData : IAffinity
    {
        private SiraLog _siraLog;
        private EditorBeatmapObjectsInTimeRowProcessor _processor;
        private PopulateBeatmap _populateBeatmap;

        public ProcessNewEditorData(
            EditorBeatmapObjectsInTimeRowProcessor processor,
            SiraLog siraLog,
            PopulateBeatmap populateBeatmap)
        {
            _siraLog = siraLog;
            _processor = processor;
            _populateBeatmap = populateBeatmap;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>), nameof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>.Add))]
        private void AddData()
        {
            _processor.Reset();
            _processor.Construct(_siraLog, _processor.editorDeserializedData, _populateBeatmap);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>), nameof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>.Add))]
        private void RemoveData(BaseEditorData? data)
        {
            EditorSpawnDataRepository.RemoveSpawnData(data);
            _processor.Reset();
            _processor.Construct(_siraLog, _processor.editorDeserializedData, _populateBeatmap);
        }
    }
}
