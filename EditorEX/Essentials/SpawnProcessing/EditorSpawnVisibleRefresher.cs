using System;
using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using Zenject;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal sealed class EditorSpawnVisibleRefresher : IDisposable
    {
        private readonly EditorSpawnProcessor _processor;
        private readonly Dictionary<BeatmapEditorObjectId, Action> _visible = new();

        internal static EditorSpawnVisibleRefresher? Instance { get; private set; }

        [Inject]
        internal EditorSpawnVisibleRefresher(EditorSpawnProcessor processor)
        {
            _processor = processor;
            Instance = this;
        }

        public void Dispose()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Register(BaseEditorData? data, Action refresh)
        {
            if (data == null)
            {
                return;
            }

            _visible[data.id] = refresh;
        }

        public void Unregister(BaseEditorData? data)
        {
            if (data == null)
            {
                return;
            }

            _visible.Remove(data.id);
        }

        public void RefreshLastRebuilt()
        {
            foreach (BaseEditorData obj in _processor.LastRebuiltObjects)
            {
                if (_visible.TryGetValue(obj.id, out Action refresh))
                {
                    refresh();
                }
            }
        }
    }
}
