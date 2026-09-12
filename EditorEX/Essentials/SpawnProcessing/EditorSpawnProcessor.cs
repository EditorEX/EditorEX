using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.Heck.Deserialize;
using EditorEX.MapData.Contexts;
using EditorEX.NoodleExtensions.ObjectData;
using EditorEX.Util;
using NoodleExtensions;
using NoodleExtensions.Managers;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal sealed class EditorSpawnProcessor
    {
        private const int NumberOfLines = 4;
        private const float MaxNotesAlignmentAngle = 40f;
        private const float LineOffset = 2f;

        private readonly EditorSpawnTimeIndex _index;
        private readonly EditorDeserializedData? _noodleData;
        private readonly List<BaseEditorData> _lastRebuiltObjects = new();

        [Inject]
        internal EditorSpawnProcessor(
            EditorSpawnTimeIndex index,
            [InjectOptional(Id = NoodleController.ID)] EditorDeserializedData? noodleData = null
        )
        {
            _index = index;
            _noodleData = noodleData;
        }

        public IReadOnlyList<BaseEditorData> LastRebuiltObjects => _lastRebuiltObjects;

        public void ClearLastRebuilt()
        {
            _lastRebuiltObjects.Clear();
        }

        public void RebuildAll()
        {
            _lastRebuiltObjects.Clear();
            foreach (int key in _index.Keys)
            {
                Rebuild(key, append: true);
            }
        }

        public void Rebuild(int beatKey)
        {
            Rebuild(beatKey, append: false);
        }

        internal void Rebuild(int beatKey, bool append)
        {
            if (!append)
            {
                _lastRebuiltObjects.Clear();
            }

            if (!_index.TryGetRow(beatKey, out EditorSpawnTimeRow row))
            {
                return;
            }

            foreach (NoteEditorData note in row.Notes)
            {
                EditorSpawnDataRepository.GetSpawnData(note).ResetToIdentity(note);
                _lastRebuiltObjects.Add(note);
            }

            foreach (BaseSliderEditorData slider in row.SliderHeads)
            {
                ResetSliderHead(slider);
                if (!_lastRebuiltObjects.Contains(slider))
                {
                    _lastRebuiltObjects.Add(slider);
                }
            }

            foreach (BaseSliderEditorData slider in row.SliderTails)
            {
                ResetSliderTail(slider);
                if (!_lastRebuiltObjects.Contains(slider))
                {
                    _lastRebuiltObjects.Add(slider);
                }
            }

            ApplyVanillaStack(row);
            ApplyColorGaps(row, beatKey);
            ApplySameColorCutAngles(row);

            int major = MapContext.Version?.Major ?? 4;
            bool noodleRules = major < 4;
            if (noodleRules)
            {
                ApplyNoodleStack(row);
                ApplyNoodleSliderOverlap(row);
            }
            else
            {
                ApplyV4SliderOverlap(row);
            }

            ApplyFlip(row);
            ApplyAuthoredFlip(row);
        }

        private static void ResetSliderHead(BaseSliderEditorData slider)
        {
            EditorObjectSpawnData spawn = EditorSpawnDataRepository.GetSpawnData(slider);
            spawn.hasHeadNote = false;
            spawn.headBeforeJumpLineLayer = (NoteLineLayer)slider.row;
            spawn.headCutDirectionAngleOffset = 0f;
            spawn.startNoteLineLayer = slider.row;
        }

        private static void ResetSliderTail(BaseSliderEditorData slider)
        {
            EditorObjectSpawnData spawn = EditorSpawnDataRepository.GetSpawnData(slider);
            spawn.hasTailNote = false;
            spawn.tailBeforeJumpLineLayer = (NoteLineLayer)slider.tailRow;
            spawn.tailCutDirectionAngleOffset = 0f;
            spawn.tailStartNoteLineLayer = slider.tailRow;
        }

        private static void ApplyVanillaStack(EditorSpawnTimeRow row)
        {
            var columns = new List<NoteEditorData>[NumberOfLines];
            for (int i = 0; i < NumberOfLines; i++)
            {
                columns[i] = new List<NoteEditorData>(3);
            }

            foreach (NoteEditorData note in row.Notes)
            {
                List<NoteEditorData> list = columns[Mathf.Clamp(note.column, 0, 3)];
                bool inserted = false;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].row > note.row)
                    {
                        list.Insert(j, note);
                        inserted = true;
                        break;
                    }
                }

                if (!inserted)
                {
                    list.Add(note);
                }
            }

            for (int k = 0; k < columns.Length; k++)
            {
                List<NoteEditorData> list = columns[k];
                for (int l = 0; l < list.Count; l++)
                {
                    EditorSpawnDataRepository.GetSpawnData(list[l]).beforeJumpNoteLineLayer =
                        (NoteLineLayer)l;
                }
            }
        }

        private void ApplyColorGaps(EditorSpawnTimeRow row, int beatKey)
        {
            float? prevBeat = null;
            if (
                _index.TryGetPreviousColorNoteKey(beatKey, out int prevKey)
                && _index.TryGetRow(prevKey, out EditorSpawnTimeRow prevRow)
                && prevRow.ColorNotes.Count > 0
            )
            {
                prevBeat = prevRow.ColorNotes[0].beat;
            }

            float? nextBeat = null;
            if (
                _index.TryGetNextColorNoteKey(beatKey, out int nextKey)
                && _index.TryGetRow(nextKey, out EditorSpawnTimeRow nextRow)
                && nextRow.ColorNotes.Count > 0
            )
            {
                nextBeat = nextRow.ColorNotes[0].beat;
            }

            foreach (NoteEditorData note in row.ColorNotes)
            {
                EditorObjectSpawnData spawn = EditorSpawnDataRepository.GetSpawnData(note);
                spawn.timeToPrevColorNote = prevBeat.HasValue ? note.beat - prevBeat.Value : 0f;
                spawn.timeToNextColorNote = nextBeat.HasValue
                    ? nextBeat.Value - note.beat
                    : float.MaxValue;
            }
        }

        private static void ApplySameColorCutAngles(EditorSpawnTimeRow row)
        {
            var byColor = new Dictionary<ColorType, List<NoteEditorData>>();
            foreach (NoteEditorData note in row.Notes)
            {
                if (!byColor.TryGetValue(note.type, out List<NoteEditorData> list))
                {
                    list = new List<NoteEditorData>(2);
                    byColor[note.type] = list;
                }

                list.Add(note);
            }

            foreach (List<NoteEditorData> items in byColor.Values)
            {
                if (items.Count != 2)
                {
                    continue;
                }

                NoteEditorData noteData = items[0];
                NoteEditorData noteData2 = items[1];
                if (
                    noteData.cutDirection != noteData2.cutDirection
                    && noteData.cutDirection != NoteCutDirection.Any
                    && noteData2.cutDirection != NoteCutDirection.Any
                )
                {
                    continue;
                }

                NoteEditorData noteData3;
                NoteEditorData noteData4;
                if (noteData.cutDirection != NoteCutDirection.Any)
                {
                    noteData3 = noteData;
                    noteData4 = noteData2;
                }
                else
                {
                    noteData3 = noteData2;
                    noteData4 = noteData;
                }

                Vector2 vector =
                    StaticBeatmapObjectSpawnMovementData.Get2DNoteOffset(
                        noteData4.column,
                        NumberOfLines,
                        (NoteLineLayer)noteData4.row
                    )
                    - StaticBeatmapObjectSpawnMovementData.Get2DNoteOffset(
                        noteData3.column,
                        NumberOfLines,
                        (NoteLineLayer)noteData3.row
                    );
                float num = (
                    (noteData3.cutDirection == NoteCutDirection.Any)
                        ? new Vector2(0f, 1f)
                        : noteData3.cutDirection.Direction()
                ).SignedAngleToLine(vector);
                if (
                    noteData4.cutDirection == NoteCutDirection.Any
                    && noteData3.cutDirection == NoteCutDirection.Any
                )
                {
                    noteData3.SetCutDirectionAngleOffset(num);
                    noteData4.SetCutDirectionAngleOffset(num);
                    continue;
                }

                if (Mathf.Abs(num) > MaxNotesAlignmentAngle)
                {
                    continue;
                }

                noteData3.SetCutDirectionAngleOffset(num);
                if (
                    noteData4.cutDirection == NoteCutDirection.Any
                    && !noteData3.cutDirection.IsMainDirection()
                )
                {
                    noteData4.SetCutDirectionAngleOffset(num + 45f);
                }
                else
                {
                    noteData4.SetCutDirectionAngleOffset(num);
                }
            }
        }

        private void ApplyFlip(EditorSpawnTimeRow row)
        {
            if (row.ColorNotes.Count != 2)
            {
                return;
            }

            if (row.SliderHeads.Count > 0 || row.SliderTails.Count > 0)
            {
                return;
            }

            EditorColorNoteFlip.Apply(row.ColorNotes, PositionOf);
        }

        private void ApplyAuthoredFlip(EditorSpawnTimeRow row)
        {
            foreach (NoteEditorData note in row.Notes)
            {
                if (
                    !TryResolveNote(note, out EditorNoodleBaseNoteData? noodle)
                    || noodle == null
                    || (!noodle.FlipX.HasValue && !noodle.FlipY.HasValue)
                )
                {
                    continue;
                }

                EditorObjectSpawnData spawn = EditorSpawnDataRepository.GetSpawnData(note);
                if (noodle.FlipX.HasValue)
                {
                    spawn.flipLineIndex = noodle.FlipX.Value + LineOffset;
                }

                if (noodle.FlipY.HasValue)
                {
                    spawn.flipYSide = noodle.FlipY.Value;
                }
            }
        }

        private void ApplyNoodleStack(EditorSpawnTimeRow row)
        {
            var notesInColumns = new Dictionary<float, List<NoteEditorData>>();
            foreach (NoteEditorData noteData in row.Notes)
            {
                (float lineIndex, float lineLayer) = PositionOf(noteData);
                if (!notesInColumns.TryGetValue(lineIndex, out List<NoteEditorData> list))
                {
                    list = new List<NoteEditorData>(1);
                    notesInColumns.Add(lineIndex, list);
                }

                bool inserted = false;
                for (int k = 0; k < list.Count; k++)
                {
                    (_, float listLineLayer) = PositionOf(list[k]);
                    if (listLineLayer > lineLayer)
                    {
                        list.Insert(k, noteData);
                        inserted = true;
                        break;
                    }
                }

                if (!inserted)
                {
                    list.Add(noteData);
                }
            }

            foreach (KeyValuePair<float, List<NoteEditorData>> pair in notesInColumns)
            {
                List<NoteEditorData> list = pair.Value;
                for (int m = 0; m < list.Count; m++)
                {
                    EditorSpawnDataRepository.GetSpawnData(list[m]).startNoteLineLayer = m;
                }
            }
        }

        private void ApplyNoodleSliderOverlap(EditorSpawnTimeRow row)
        {
            foreach (BaseSliderEditorData sliderData in row.SliderHeads)
            {
                (float headX, float headY) = HeadPositionOf(sliderData);
                (float tailX, float tailY) = TailPositionOf(sliderData);
                foreach (NoteEditorData noteData in row.Notes)
                {
                    (float noteX, float noteY) = PositionOf(noteData);
                    if (!Mathf.Approximately(headX, noteX) || !Mathf.Approximately(headY, noteY))
                    {
                        continue;
                    }

                    sliderData.SetHasHeadNote(true);
                    EditorSpawnDataRepository.GetSpawnData(sliderData).startNoteLineLayer =
                        EditorSpawnDataRepository.GetSpawnData(noteData).startNoteLineLayer;
                    if (sliderData is not ChainEditorData)
                    {
                        continue;
                    }

                    noteData.ChangeToBurstSliderHead();
                    if (noteData.cutDirection != sliderData.cutDirection)
                    {
                        continue;
                    }

                    Vector2 line =
                        SpawnDataManager.Get2DNoteOffset(noteX, NumberOfLines, noteY)
                        - SpawnDataManager.Get2DNoteOffset(tailX, NumberOfLines, tailY);
                    float num = noteData.cutDirection.Direction().SignedAngleToLine(line);
                    if (Mathf.Abs(num) > MaxNotesAlignmentAngle)
                    {
                        continue;
                    }

                    noteData.SetCutDirectionAngleOffset(num);
                    sliderData.SetCutDirectionAngleOffset(num, num);
                }
            }

            foreach (BaseSliderEditorData sliderData in row.SliderTails)
            {
                (float tailX, float tailY) = TailPositionOf(sliderData);
                foreach (NoteEditorData noteData in row.Notes)
                {
                    (float noteX, float noteY) = PositionOf(noteData);
                    if (!Mathf.Approximately(tailX, noteX) || !Mathf.Approximately(tailY, noteY))
                    {
                        continue;
                    }

                    sliderData.SetHasTailNote(true);
                    EditorSpawnDataRepository.GetSpawnData(sliderData).tailStartNoteLineLayer =
                        EditorSpawnDataRepository.GetSpawnData(noteData).startNoteLineLayer;
                    sliderData.SetTailBeforeJumpLineLayer(
                        EditorSpawnDataRepository.GetSpawnData(noteData).beforeJumpNoteLineLayer
                    );
                }
            }
        }

        private static void ApplyV4SliderOverlap(EditorSpawnTimeRow row)
        {
            foreach (BaseSliderEditorData sliderData in row.SliderHeads)
            {
                foreach (NoteEditorData noteData in row.Notes)
                {
                    if (sliderData.column == noteData.column && sliderData.row == noteData.row)
                    {
                        sliderData.SetHasHeadNote(true);
                        sliderData.SetHeadBeforeJumpLineLayer(
                            EditorSpawnDataRepository.GetSpawnData(noteData).beforeJumpNoteLineLayer
                        );
                        if (sliderData is ChainEditorData)
                        {
                            noteData.ChangeToBurstSliderHead();
                        }
                    }
                }

                foreach (BaseSliderEditorData other in row.SliderHeads)
                {
                    if (sliderData != other && SliderHeadOverlapsBurstTail(sliderData, other))
                    {
                        sliderData.SetHasHeadNote(true);
                        sliderData.SetHeadBeforeJumpLineLayer(
                            EditorSpawnDataRepository.GetSpawnData(other).tailBeforeJumpLineLayer
                        );
                    }
                }

                foreach (BaseSliderEditorData tailSlider in row.SliderTails)
                {
                    if (SliderHeadOverlapsBurstTail(sliderData, tailSlider))
                    {
                        sliderData.SetHasHeadNote(true);
                        sliderData.SetHeadBeforeJumpLineLayer(
                            EditorSpawnDataRepository
                                .GetSpawnData(tailSlider)
                                .tailBeforeJumpLineLayer
                        );
                    }
                }
            }

            foreach (BaseSliderEditorData slider in row.SliderTails)
            {
                foreach (NoteEditorData noteData in row.Notes)
                {
                    if (slider.tailColumn == noteData.column && slider.tailRow == noteData.row)
                    {
                        slider.SetHasTailNote(true);
                        slider.SetTailBeforeJumpLineLayer(
                            EditorSpawnDataRepository.GetSpawnData(noteData).beforeJumpNoteLineLayer
                        );
                    }
                }
            }
        }

        private static bool SliderHeadOverlapsBurstTail(
            BaseSliderEditorData slider,
            BaseSliderEditorData sliderTail
        )
        {
            return slider.beatmapObjectType == BeatmapObjectType.Arc
                && sliderTail.beatmapObjectType == BeatmapObjectType.Chain
                && slider.column == sliderTail.tailColumn
                && slider.row == sliderTail.tailRow;
        }

        private (float x, float y) PositionOf(NoteEditorData note)
        {
            if (TryResolveNote(note, out EditorNoodleBaseNoteData? noodle) && noodle != null)
            {
                return (noodle.StartX + LineOffset ?? note.column, noodle.StartY ?? note.row);
            }

            return (note.column, note.row);
        }

        private (float x, float y) HeadPositionOf(BaseSliderEditorData slider)
        {
            if (TryResolveSlider(slider, out EditorNoodleSliderData? noodle) && noodle != null)
            {
                return (noodle.StartX + LineOffset ?? slider.column, noodle.StartY ?? slider.row);
            }

            return (slider.column, slider.row);
        }

        private (float x, float y) TailPositionOf(BaseSliderEditorData slider)
        {
            if (TryResolveSlider(slider, out EditorNoodleSliderData? noodle) && noodle != null)
            {
                return (
                    noodle.TailStartX + LineOffset ?? slider.tailColumn,
                    noodle.TailStartY ?? slider.tailRow
                );
            }

            return (slider.tailColumn, slider.tailRow);
        }

        private bool TryResolveNote(NoteEditorData note, out EditorNoodleBaseNoteData? noodle)
        {
            noodle = null;
            return _noodleData?.Resolve(note, out noodle) ?? false;
        }

        private bool TryResolveSlider(
            BaseSliderEditorData slider,
            out EditorNoodleSliderData? noodle
        )
        {
            noodle = null;
            return _noodleData?.Resolve(slider, out noodle) ?? false;
        }
    }
}
