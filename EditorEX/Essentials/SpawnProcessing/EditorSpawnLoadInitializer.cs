using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Patches;
using Zenject;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal sealed class EditorSpawnLoadInitializer : IInitializable
    {
        private readonly EditorSpawnTimeIndex _index;
        private readonly EditorSpawnProcessor _processor;
        private readonly EditorSpawnDirtyTracker _tracker;
        private readonly IEditorBeatmapModels _models;

        [Inject]
        internal EditorSpawnLoadInitializer(
            EditorSpawnTimeIndex index,
            EditorSpawnProcessor processor,
            EditorSpawnDirtyTracker tracker,
            IEditorBeatmapModels models
        )
        {
            _index = index;
            _processor = processor;
            _tracker = tracker;
            _models = models;
        }

        public void Initialize()
        {
            _index.Clear();
            foreach (BaseEditorData obj in _models.BeatmapObjectsDataModel.allBeatmapObjects)
            {
                _index.Add(obj);
            }

            _tracker.SwapDirtyKeys();
            _processor.RebuildAll();
            _tracker.IsReady = true;
        }
    }
}
