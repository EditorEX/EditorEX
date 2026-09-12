using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using SiraUtil.Logging;
using Zenject;

namespace EditorEX.Essentials.SpawnProcessing
{
    public sealed class RebuildEditorSpawnDataCommand : IBeatmapEditorCommand
    {
        private readonly EditorSpawnDirtyTracker _tracker;
        private readonly EditorSpawnProcessor _processor;
        private readonly SiraLog? _siraLog;

        internal RebuildEditorSpawnDataCommand(
            EditorSpawnDirtyTracker tracker,
            EditorSpawnProcessor processor
        )
            : this(tracker, processor, null) { }

        [Inject]
        internal RebuildEditorSpawnDataCommand(
            EditorSpawnDirtyTracker tracker,
            EditorSpawnProcessor processor,
            SiraLog? siraLog
        )
        {
            _tracker = tracker;
            _processor = processor;
            _siraLog = siraLog;
        }

        public void Execute()
        {
            HashSet<int> keys = _tracker.SwapDirtyKeys();
            if (keys.Count == 0)
            {
                _processor.ClearLastRebuilt();
                return;
            }

            bool first = true;
            foreach (int key in keys.OrderBy(k => k))
            {
                try
                {
                    _processor.Rebuild(key, append: !first);
                    first = false;
                }
                catch (Exception e)
                {
                    _siraLog?.Error(e.ToString());
                }
            }

            EditorSpawnVisibleRefresher.Instance?.RefreshLastRebuilt();
        }
    }
}
