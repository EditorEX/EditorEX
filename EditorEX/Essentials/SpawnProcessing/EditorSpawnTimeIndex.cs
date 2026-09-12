using System;
using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal sealed class EditorSpawnTimeIndex
    {
        public const float Epsilon = 0.001f;

        private readonly SortedDictionary<int, EditorSpawnTimeRow> _rows = new();

        public static int Quantize(float beat)
        {
            return (int)Math.Round(beat / Epsilon);
        }

        public IEnumerable<int> Keys => _rows.Keys;

        public static bool IsColorNote(NoteEditorData note)
        {
            return note.type != ColorType.None && note.cutDirection != NoteCutDirection.None;
        }

        public void Add(BaseEditorData data)
        {
            switch (data)
            {
                case NoteEditorData note:
                    AddNote(note);
                    break;
                case BaseSliderEditorData slider:
                    AddSlider(slider);
                    break;
            }
        }

        public void Remove(BaseEditorData data)
        {
            switch (data)
            {
                case NoteEditorData note:
                    RemoveNote(note);
                    break;
                case BaseSliderEditorData slider:
                    RemoveSlider(slider);
                    break;
            }
        }

        public void Clear()
        {
            _rows.Clear();
        }

        public bool TryGetRow(int key, out EditorSpawnTimeRow row)
        {
            return _rows.TryGetValue(key, out row);
        }

        public bool TryGetPreviousColorNoteKey(int key, out int prevKey)
        {
            prevKey = 0;
            bool found = false;
            foreach (KeyValuePair<int, EditorSpawnTimeRow> pair in _rows)
            {
                if (pair.Key >= key)
                {
                    break;
                }

                if (pair.Value.ColorNotesList.Count > 0)
                {
                    prevKey = pair.Key;
                    found = true;
                }
            }

            return found;
        }

        public bool TryGetNextColorNoteKey(int key, out int nextKey)
        {
            nextKey = 0;
            foreach (KeyValuePair<int, EditorSpawnTimeRow> pair in _rows)
            {
                if (pair.Key <= key)
                {
                    continue;
                }

                if (pair.Value.ColorNotesList.Count > 0)
                {
                    nextKey = pair.Key;
                    return true;
                }
            }

            return false;
        }

        private void AddNote(NoteEditorData note)
        {
            EditorSpawnTimeRow row = GetOrCreate(Quantize(note.beat), note.beat);
            if (ContainsId(row.NotesList, note.id))
            {
                return;
            }

            row.NotesList.Add(note);
            if (IsColorNote(note))
            {
                row.ColorNotesList.Add(note);
            }
        }

        private void RemoveNote(NoteEditorData note)
        {
            int key = Quantize(note.beat);
            if (!_rows.TryGetValue(key, out EditorSpawnTimeRow row))
            {
                return;
            }

            RemoveById(row.NotesList, note.id);
            RemoveById(row.ColorNotesList, note.id);
            DropIfEmpty(key, row);
        }

        private void AddSlider(BaseSliderEditorData slider)
        {
            EditorSpawnTimeRow head = GetOrCreate(Quantize(slider.beat), slider.beat);
            if (!ContainsId(head.SliderHeadsList, slider.id))
            {
                head.SliderHeadsList.Add(slider);
            }

            EditorSpawnTimeRow tail = GetOrCreate(Quantize(slider.tailBeat), slider.tailBeat);
            if (!ContainsId(tail.SliderTailsList, slider.id))
            {
                tail.SliderTailsList.Add(slider);
            }
        }

        private void RemoveSlider(BaseSliderEditorData slider)
        {
            RemoveSliderFrom(Quantize(slider.beat), slider.id, heads: true);
            RemoveSliderFrom(Quantize(slider.tailBeat), slider.id, heads: false);
        }

        private void RemoveSliderFrom(int key, BeatmapEditorObjectId id, bool heads)
        {
            if (!_rows.TryGetValue(key, out EditorSpawnTimeRow row))
            {
                return;
            }

            if (heads)
            {
                RemoveById(row.SliderHeadsList, id);
            }
            else
            {
                RemoveById(row.SliderTailsList, id);
            }

            DropIfEmpty(key, row);
        }

        private EditorSpawnTimeRow GetOrCreate(int key, float beat)
        {
            if (_rows.TryGetValue(key, out EditorSpawnTimeRow row))
            {
                return row;
            }

            row = new EditorSpawnTimeRow(key, beat);
            _rows[key] = row;
            return row;
        }

        private void DropIfEmpty(int key, EditorSpawnTimeRow row)
        {
            if (row.IsEmpty)
            {
                _rows.Remove(key);
            }
        }

        private static bool ContainsId<T>(List<T> list, BeatmapEditorObjectId id)
            where T : BaseEditorData
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveById<T>(List<T> list, BeatmapEditorObjectId id)
            where T : BaseEditorData
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].id == id)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
