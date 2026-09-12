using System.Collections.Generic;
using BeatmapEditor3D.DataModels;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal sealed class EditorSpawnDirtyTracker
    {
        private readonly EditorSpawnTimeIndex _index;
        private HashSet<int> _dirty = new();

        internal EditorSpawnDirtyTracker(EditorSpawnTimeIndex index)
        {
            _index = index;
        }

        public bool IsReady { get; set; }

        public void NoteAdded(NoteEditorData note)
        {
            _index.Add(note);
            DirtyNoteKeys(note);
        }

        public void NoteRemoved(NoteEditorData note)
        {
            CaptureColorNeighbors(EditorSpawnTimeIndex.Quantize(note.beat));
            _index.Remove(note);
            _dirty.Add(EditorSpawnTimeIndex.Quantize(note.beat));
            EditorSpawnDataRepository.RemoveSpawnData(note);
        }

        public void SliderAdded(BaseSliderEditorData slider)
        {
            _index.Add(slider);
            DirtySliderKeys(slider);
        }

        public void SliderRemoved(BaseSliderEditorData slider)
        {
            CaptureColorNeighbors(EditorSpawnTimeIndex.Quantize(slider.beat));
            CaptureColorNeighbors(EditorSpawnTimeIndex.Quantize(slider.tailBeat));
            _index.Remove(slider);
            _dirty.Add(EditorSpawnTimeIndex.Quantize(slider.beat));
            _dirty.Add(EditorSpawnTimeIndex.Quantize(slider.tailBeat));
            EditorSpawnDataRepository.RemoveSpawnData(slider);
        }

        public void MarkExistingDirty(BaseEditorData data)
        {
            switch (data)
            {
                case NoteEditorData note:
                    DirtyNoteKeys(note);
                    break;
                case BaseSliderEditorData slider:
                    DirtySliderKeys(slider);
                    break;
            }
        }

        public HashSet<int> SwapDirtyKeys()
        {
            HashSet<int> current = _dirty;
            _dirty = new HashSet<int>();
            return current;
        }

        private void DirtyNoteKeys(NoteEditorData note)
        {
            int key = EditorSpawnTimeIndex.Quantize(note.beat);
            _dirty.Add(key);
            CaptureColorNeighbors(key);
        }

        private void DirtySliderKeys(BaseSliderEditorData slider)
        {
            int head = EditorSpawnTimeIndex.Quantize(slider.beat);
            int tail = EditorSpawnTimeIndex.Quantize(slider.tailBeat);
            _dirty.Add(head);
            _dirty.Add(tail);
            CaptureColorNeighbors(head);
            CaptureColorNeighbors(tail);
        }

        private void CaptureColorNeighbors(int key)
        {
            if (_index.TryGetPreviousColorNoteKey(key, out int prev))
            {
                _dirty.Add(prev);
            }

            if (_index.TryGetNextColorNoteKey(key, out int next))
            {
                _dirty.Add(next);
            }
        }
    }
}
