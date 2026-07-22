using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.SpawnProcessing;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.Essentials.Patches
{
    internal class ProcessNewEditorData : IAffinity
    {
        private SiraLog _siraLog;
        private ICustomDataRepository _customDataRepository;
        private EditorBeatmapObjectsInTimeRowProcessor _processor;
        private IEditorBeatmapModels _populateBeatmap;

        public ProcessNewEditorData(
            EditorBeatmapObjectsInTimeRowProcessor processor,
            SiraLog siraLog,
            ICustomDataRepository customDataRepository,
            IEditorBeatmapModels populateBeatmap
        )
        {
            _siraLog = siraLog;
            _customDataRepository = customDataRepository;
            _processor = processor;
            _populateBeatmap = populateBeatmap;
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>),
            nameof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>.Add)
        )]
        private void AddData()
        {
            _processor.Reset();
            _processor.Construct(
                _siraLog,
                _customDataRepository,
                _processor.editorDeserializedData,
                _populateBeatmap
            );
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>),
            nameof(BeatFrameSortedCollection<BeatmapObjectsFrameDataContainer>.Add)
        )]
        private void RemoveData(BaseEditorData? data)
        {
            EditorSpawnDataRepository.RemoveSpawnData(data);
            _processor.Reset();
            _processor.Construct(
                _siraLog,
                _customDataRepository,
                _processor.editorDeserializedData,
                _populateBeatmap
            );
        }
    }
}
