using System.Collections.Generic;
using BeatmapEditor3D.DataModels;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal sealed class EditorSpawnTimeRow
    {
        internal EditorSpawnTimeRow(int key, float beat)
        {
            Key = key;
            Beat = beat;
        }

        public int Key { get; }

        public float Beat { get; }

        internal List<NoteEditorData> NotesList { get; } = new();

        internal List<NoteEditorData> ColorNotesList { get; } = new();

        internal List<BaseSliderEditorData> SliderHeadsList { get; } = new();

        internal List<BaseSliderEditorData> SliderTailsList { get; } = new();

        public IReadOnlyList<NoteEditorData> Notes => NotesList;

        public IReadOnlyList<NoteEditorData> ColorNotes => ColorNotesList;

        public IReadOnlyList<BaseSliderEditorData> SliderHeads => SliderHeadsList;

        public IReadOnlyList<BaseSliderEditorData> SliderTails => SliderTailsList;

        internal bool IsEmpty =>
            NotesList.Count == 0 && SliderHeadsList.Count == 0 && SliderTailsList.Count == 0;
    }
}
