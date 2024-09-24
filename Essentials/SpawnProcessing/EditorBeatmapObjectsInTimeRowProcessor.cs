using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BetterEditor.CustomJSONData;
using BetterEditor.CustomJSONData.Util;
using BetterEditor.Essentials.Patches;
using BetterEditor.Essentials.SpawnProcessing;
using BetterEditor.NoodleExtensions.Util;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using NoodleExtensions.HarmonyPatches.ObjectProcessing;
using NoodleExtensions.Managers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;
using static BeatmapObjectsInTimeRowProcessor;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace BetterEditor.Essentials.SpawnProcessing
{
    public class EditorBeatmapObjectsInTimeRowProcessor
    {
        public EditorBeatmapObjectsInTimeRowProcessor()
        {
            Reset();
        }

        public void Reset()
        {
            _currentTimeSliceColorNotes = new EditorTimeSliceContainer<NoteEditorData>(8);
            _currentTimeSliceAllNotesAndSliders = new EditorTimeSliceContainer<BaseEditorData>(8);
            _currentTimeSliceNotesByColorType = new Dictionary<ColorType, EditorTimeSliceContainer<NoteEditorData>>();
            _unprocessedSliderTails = new List<BaseSliderEditorData>(4);

            int numberOfLines = 4;
            _numberOfLines = numberOfLines;
            _notesInColumnsReusableProcessingListOfLists = new List<NoteEditorData>[_numberOfLines];
            for (int i = 0; i < numberOfLines; i++)
            {
                _notesInColumnsReusableProcessingListOfLists[i] = new List<NoteEditorData>(3);
            }
            foreach (ColorType colorType in (ColorType[])Enum.GetValues(typeof(ColorType)))
            {
                EditorTimeSliceContainer<NoteEditorData> timeSliceContainer = new EditorTimeSliceContainer<NoteEditorData>(4);
                timeSliceContainer.didFinishTimeSliceEvent += HandlePerColorTypeTimeSliceContainerDidFinishTimeSlice;
                _currentTimeSliceNotesByColorType[colorType] = timeSliceContainer;
            }
            _currentTimeSliceAllNotesAndSliders.didFinishTimeSliceEvent += HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice;
            _currentTimeSliceAllNotesAndSliders.didStartNewTimeSliceEvent += HandleCurrentNewTimeSliceAllNotesAndSlidersDidStartNewTimeSlice;
            _currentTimeSliceColorNotes.didFinishTimeSliceEvent += HandleCurrentTimeSliceColorNotesDidFinishTimeSlice;
            _currentTimeSliceColorNotes.didAddItemEvent += HandleCurrentTimeSliceColorNotesDidAddItem;
        }

        [Inject]
        public void Construct()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            BeatmapLevelDataModel beatmapLevelDataModel = PopulateBeatmap._beatmapLevelDataModel;

            var objects = beatmapLevelDataModel.allBeatmapObjects.Cast<BaseEditorData>();
            foreach (var obj in objects)
            {
                if (obj is NoteEditorData noteData)
                {
                    //Plugin.Log.Info("Processing note");
                    ProcessNote(noteData);
                }
                else if (obj is BaseSliderEditorData sliderData)
                {
                    ProcessSlider(sliderData);
                }
            }

            stopwatch.Stop();
            Plugin.Log.Info($"bdf {stopwatch.ElapsedMilliseconds}");
        }

        public void ProcessNote(NoteEditorData noteData)
        {
            if (noteData.type != ColorType.None && noteData.cutDirection != NoteCutDirection.None)
            {
                _currentTimeSliceColorNotes.Add(noteData);
            }
            _currentTimeSliceNotesByColorType[noteData.type].Add(noteData);
            _currentTimeSliceAllNotesAndSliders.Add(noteData);
        }

        public void ProcessSlider(BaseSliderEditorData sliderData)
        {
            _currentTimeSliceAllNotesAndSliders.Add(sliderData);
            bool flag = false;
            for (int i = 0; i < _unprocessedSliderTails.Count; i++)
            {
                if (_unprocessedSliderTails[i].tailBeat > sliderData.tailBeat)
                {
                    _unprocessedSliderTails.Insert(i, sliderData);
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                _unprocessedSliderTails.Add(sliderData);
            }
        }

        public void ProcessAllRemainingData()
        {
            _currentTimeSliceColorNotes.FinishTimeSlice(float.MaxValue);
            _currentTimeSliceAllNotesAndSliders.FinishTimeSlice(float.MaxValue);
            foreach (KeyValuePair<ColorType, EditorTimeSliceContainer<NoteEditorData>> keyValuePair in _currentTimeSliceNotesByColorType)
            {
                keyValuePair.Value.FinishTimeSlice(float.MaxValue);
            }
            _unprocessedSliderTails.Clear();
        }

        private void HandleCurrentTimeSliceColorNotesDidAddItem(EditorTimeSliceContainer<NoteEditorData> timeSliceContainer, NoteEditorData noteData)
        {
            EditorSpawnDataRepository.GetSpawnData(noteData).timeToNextColorNote = float.MaxValue;
            EditorSpawnDataRepository.GetSpawnData(noteData).timeToPrevColorNote = noteData.beat - timeSliceContainer.previousTimeSliceTime;
        }

        private void HandleCurrentNewTimeSliceAllNotesAndSlidersDidStartNewTimeSlice(EditorTimeSliceContainer<BaseEditorData> allObjectsTimeSlice)
        {
            float time = allObjectsTimeSlice.time;
            while (_unprocessedSliderTails.Count > 0)
            {
                if (_unprocessedSliderTails[0].tailBeat >= time - 0.001f)
                {
                    break;
                }
                _unprocessedSliderTails.RemoveAt(0);
            }
            while (_unprocessedSliderTails.Count > 0 && Mathf.Abs(_unprocessedSliderTails[0].tailBeat - time) < 0.001f)
            {
                allObjectsTimeSlice.AddWithoutNotifications(new EditorSliderTailData(_unprocessedSliderTails[0]));
                _unprocessedSliderTails.RemoveAt(0);
            }
        }

        private void HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice(EditorTimeSliceContainer<BaseEditorData> allObjectsTimeSlice, float nextTimeSliceTime)
        {
            List<NoteEditorData>[] notesInColumnsReusableProcessingListOfLists = _notesInColumnsReusableProcessingListOfLists;
            for (int i = 0; i < notesInColumnsReusableProcessingListOfLists.Length; i++)
            {
                notesInColumnsReusableProcessingListOfLists[i].Clear();
            }
            IEnumerable<NoteEditorData> enumerable = allObjectsTimeSlice.items.OfType<NoteEditorData>();
            IEnumerable<BaseSliderEditorData> enumerable2 = allObjectsTimeSlice.items.OfType<BaseSliderEditorData>();
            IEnumerable<EditorSliderTailData> enumerable3 = allObjectsTimeSlice.items.OfType<EditorSliderTailData>();
            foreach (NoteEditorData noteData in enumerable)
            {
                List<NoteEditorData> list = _notesInColumnsReusableProcessingListOfLists[Mathf.Clamp(noteData.column, 0, 3)];
                bool flag = false;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].row > noteData.row)
                    {
                        list.Insert(j, noteData);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    list.Add(noteData);
                }
                //Plugin.Log.Info("hi2asdf! " + list.Count);
            }
            for (int k = 0; k < _notesInColumnsReusableProcessingListOfLists.Length; k++)
            {
                List<NoteEditorData> list2 = _notesInColumnsReusableProcessingListOfLists[k];
                for (int l = 0; l < list2.Count; l++)
                {
                    //Plugin.Log.Info("hi! " + list2.Count);
                    list2[l].SetBeforeJumpNoteLineLayer((NoteLineLayer)l);
                }
            }

            float offset = 4 / 2f;
            bool v2 = CustomDataRepository.GetCustomLivePreviewBeatmapData().version2_6_0AndEarlier;
            IReadOnlyList<BaseBeatmapObjectEditorData> containerItems = allObjectsTimeSlice.items.Where(x => x is BaseBeatmapObjectEditorData).Select(x => x as BaseBeatmapObjectEditorData).ToList();
            IEnumerable<NoteEditorData> notesInTimeRow = containerItems.OfType<NoteEditorData>().ToArray();
            Dictionary<float, List<NoteEditorData>> notesInColumns = new();
            foreach (NoteEditorData noteData in notesInTimeRow)
            {
                CustomData customData = noteData.GetCustomData();
                if (customData == null)
                {
                    Plugin.Log.Info("NERD");
                    continue;
                }
                IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                float lineIndex = position?.ElementAtOrDefault(0) + offset ?? noteData.column;
                float lineLayer = position?.ElementAtOrDefault(1) ?? (float)noteData.row;

                if (!notesInColumns.TryGetValue(lineIndex, out List<NoteEditorData> list))
                {
                    list = new List<NoteEditorData>(1);
                    notesInColumns.Add(lineIndex, list);
                }

                bool flag = false;
                for (int k = 0; k < list.Count; k++)
                {
                    IEnumerable<float?>? listPosition = list[k].GetCustomData().GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET);
                    float listLineLayer = listPosition?.ElementAtOrDefault(1) ?? (float)list[k].row;
                    if (!(listLineLayer > lineLayer))
                    {
                        continue;
                    }

                    list.Insert(k, noteData);
                    flag = true;
                    break;
                }

                if (!flag)
                {
                    list.Add(noteData);
                }

                // Flippy stuff
                IEnumerable<float?>? flip = customData.GetNullableFloats(v2 ? V2_FLIP : FLIP)?.ToList();
                float? flipX = flip?.ElementAtOrDefault(0);
                float? flipY = flip?.ElementAtOrDefault(1);
                if (flipX.HasValue || flipY.HasValue)
                {
                    if (flipX.HasValue)
                    {
                        customData[INTERNAL_FLIPLINEINDEX] = flipX.Value + offset;
                    }

                    if (flipY.HasValue)
                    {
                        customData[INTERNAL_FLIPYSIDE] = flipY.Value;
                    }
                }
                else if (!customData.ContainsKey(INTERNAL_FLIPYSIDE))
                {
                    customData[INTERNAL_FLIPLINEINDEX] = lineIndex;
                    customData[INTERNAL_FLIPYSIDE] = 0;
                }
            }

            foreach (KeyValuePair<float, List<NoteEditorData>> keyValue in notesInColumns)
            {
                List<NoteEditorData> list2 = keyValue.Value;
                for (int m = 0; m < list2.Count; m++)
                {
                    list2[m].GetCustomData()[INTERNAL_STARTNOTELINELAYER] = m;
                }
            }

            IEnumerable<BaseSliderEditorData> slidersInTimeRow = containerItems.OfType<BaseSliderEditorData>().ToArray();
            foreach (BaseSliderEditorData sliderData in slidersInTimeRow)
            {
                IEnumerable<float?>? headPosition = sliderData.GetCustomData().GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                float headX = headPosition?.ElementAtOrDefault(0) + offset ?? sliderData.column;
                float headY = headPosition?.ElementAtOrDefault(1) ?? (float)sliderData.row;
                IEnumerable<float?>? tailPosition = sliderData.GetCustomData().GetNullableFloats(TAIL_NOTE_OFFSET)?.ToList();
                float tailX = tailPosition?.ElementAtOrDefault(0) + offset ?? sliderData.tailColumn;
                float tailY = tailPosition?.ElementAtOrDefault(1) ?? (float)sliderData.tailRow;

                foreach (NoteEditorData noteData in notesInTimeRow)
                {
                    if (noteData.GetCustomData() == null)
                    {
                        Plugin.Log.Info("NERD");
                        continue;
                    }
                    IEnumerable<float?>? notePosition = noteData.GetCustomData().GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                    float noteX = notePosition?.ElementAtOrDefault(0) + offset ?? noteData.column;
                    float noteY = notePosition?.ElementAtOrDefault(1) ?? (float)noteData.row;

                    if (!Mathf.Approximately(headX, noteX) || !Mathf.Approximately(headY, noteY))
                    {
                        continue;
                    }

                    sliderData.SetHasHeadNote(true);
                    sliderData.GetCustomData()[INTERNAL_STARTNOTELINELAYER] = noteData.GetCustomData()[INTERNAL_STARTNOTELINELAYER];
                    if (sliderData is ChainEditorData)
                    {
                        noteData.ChangeToBurstSliderHead();
                        if (noteData.cutDirection != sliderData.cutDirection)
                        {
                            continue;
                        }

                        Vector2 line = SpawnDataManager.Get2DNoteOffset(noteX, 4, noteY) -
                                       SpawnDataManager.Get2DNoteOffset(tailX, 4, tailY);
                        float num = noteData.cutDirection.Direction().SignedAngleToLine(line);
                        if (!(Mathf.Abs(num) <= 40f))
                        {
                            continue;
                        }

                        noteData.SetCutDirectionAngleOffset(num);
                        sliderData.SetCutDirectionAngleOffset(num, num);
                    }
                }
            }

            foreach (EditorSliderTailData sliderTailData in containerItems.OfType<EditorSliderTailData>())
            {
                BaseSliderEditorData sliderData = sliderTailData.slider;
                IEnumerable<float?>? tailPosition = sliderData.GetCustomData().GetNullableFloats(TAIL_NOTE_OFFSET)?.ToList();
                float tailX = tailPosition?.ElementAtOrDefault(0) + offset ?? sliderData.tailColumn;
                float tailY = tailPosition?.ElementAtOrDefault(1) ?? (float)sliderData.tailRow;
                foreach (NoteEditorData noteData in notesInTimeRow)
                {
                    IEnumerable<float?>? notePosition = noteData.GetCustomData().GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                    float noteX = notePosition?.ElementAtOrDefault(0) + offset ?? noteData.column;
                    float noteY = notePosition?.ElementAtOrDefault(1) ?? (float)noteData.row;

                    if (!Mathf.Approximately(tailX, noteX) || !Mathf.Approximately(tailY, noteY))
                    {
                        continue;
                    }

                    sliderData.SetHasTailNote(true);
                    sliderData.GetCustomData()[INTERNAL_TAILSTARTNOTELINELAYER] = noteData.GetCustomData()[INTERNAL_STARTNOTELINELAYER];
                    sliderData.SetTailBeforeJumpLineLayer(EditorSpawnDataRepository.GetSpawnData(noteData).beforeJumpNoteLineLayer);
                }
            }

            return;
            foreach (BaseSliderEditorData sliderData in enumerable2)
            {
                foreach (NoteEditorData noteData2 in enumerable)
                {
                    if (SliderHeadPositionOverlapsWithNote(sliderData, noteData2))
                    {
                        sliderData.SetHasHeadNote(true);
                        sliderData.SetHeadBeforeJumpLineLayer(EditorSpawnDataRepository.GetSpawnData(noteData2).beforeJumpNoteLineLayer);
                        if (sliderData is ChainEditorData)
                        {
                            noteData2.ChangeToBurstSliderHead();
                        }
                    }
                }
            }
            foreach (BaseSliderEditorData sliderData2 in enumerable2)
            {
                foreach (BaseSliderEditorData sliderData3 in enumerable2)
                {
                    if (sliderData2 != sliderData3 && SliderHeadPositionOverlapsWithBurstTail(sliderData2, sliderData3))
                    {
                        sliderData2.SetHasHeadNote(true);
                        sliderData2.SetHeadBeforeJumpLineLayer(EditorSpawnDataRepository.GetSpawnData(sliderData3).tailBeforeJumpLineLayer);
                    }
                }
                foreach (EditorSliderTailData sliderTailData in enumerable3)
                {
                    if (SliderHeadPositionOverlapsWithBurstTail(sliderData2, sliderTailData.slider))
                    {
                        sliderData2.SetHasHeadNote(true);
                        sliderData2.SetHeadBeforeJumpLineLayer(EditorSpawnDataRepository.GetSpawnData(sliderTailData.slider).tailBeforeJumpLineLayer);
                    }
                }
            }
            foreach (EditorSliderTailData sliderTailData2 in enumerable3)
            {
                BaseSliderEditorData slider = sliderTailData2.slider;
                foreach (NoteEditorData noteData3 in enumerable)
                {
                    if (SliderTailPositionOverlapsWithNote(slider, noteData3))
                    {
                        slider.SetHasTailNote(true);
                        slider.SetTailBeforeJumpLineLayer(EditorSpawnDataRepository.GetSpawnData(noteData3).beforeJumpNoteLineLayer);
                    }
                }
            }
        }

        private void HandleCurrentTimeSliceColorNotesDidFinishTimeSlice(EditorTimeSliceContainer<NoteEditorData> currentTimeSlice, float nextTimeSliceTime)
        {
            float offset = 4 / 2f;
            IReadOnlyList<NoteEditorData> colorNotesData = currentTimeSlice.items;
            int customNoteCount = colorNotesData.Count;
            if (customNoteCount != 2)
            {
                return;
            }

            bool v2 = CustomDataRepository.GetCustomLivePreviewBeatmapData().version2_6_0AndEarlier;

            float[] lineIndexes = new float[2];
            float[] lineLayers = new float[2];
            for (int i = 0; i < 2; i++)
            {
                if (colorNotesData[i] is not NoteEditorData noteData)
                {
                    continue;
                }

                CustomData customData = noteData.GetCustomData();
                IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                lineIndexes[i] = position?.ElementAtOrDefault(0) + offset ?? colorNotesData[i].column;
                lineLayers[i] = position?.ElementAtOrDefault(1) ?? (float)colorNotesData[i].row;
            }

            if (colorNotesData[0].type == colorNotesData[1].type ||
                ((colorNotesData[0].type != ColorType.ColorA || !(lineIndexes[0] > lineIndexes[1])) &&
                 (colorNotesData[0].type != ColorType.ColorB || !(lineIndexes[0] < lineIndexes[1]))))
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                if (colorNotesData[i] is not NoteEditorData noteData)
                {
                    continue;
                }

                // apparently I can use customData to store my own variables in noteData, neat
                // ^ comment from a very young and naive aero
                CustomData customData = noteData.GetCustomData();
                customData[INTERNAL_FLIPLINEINDEX] = lineIndexes[1 - i];

                float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                if ((lineIndexes[i] > lineIndexes[1 - i] &&
                     lineLayers[i] < lineLayers[1 - i]) ||
                    (lineIndexes[i] < lineIndexes[1 - i] &&
                     lineLayers[i] > lineLayers[1 - i]))
                {
                    flipYSide *= -1f;
                }

                customData[INTERNAL_FLIPYSIDE] = flipYSide;
            }

            IReadOnlyList<NoteEditorData> items = currentTimeSlice.items;
            foreach (NoteEditorData noteData in items)
            {
                EditorSpawnDataRepository.GetSpawnData(noteData).timeToNextColorNote = nextTimeSliceTime - noteData.beat;
            }
            float currentTimeSliceTime = currentTimeSlice.time;
            if (items.Count != 2)
            {
                return;
            }
            bool flag;
            if (Math.Abs(_currentTimeSliceAllNotesAndSliders.time - currentTimeSliceTime) < 0.001f)
            {
                if (_currentTimeSliceAllNotesAndSliders.items.Any((BaseEditorData item) => item is BaseSliderEditorData || item is EditorSliderTailData))
                {
                    flag = true;
                    goto IL_C6;
                }
            }
            flag = _unprocessedSliderTails.Any((BaseSliderEditorData tail) => Math.Abs(tail.tailBeat - currentTimeSliceTime) < 0.001f);
        IL_C6:
            if (flag)
            {
                return;
            }
            NoteEditorData noteData2 = items[0];
            NoteEditorData noteData3 = items[1];
            if (noteData2.type != noteData3.type && ((noteData2.type == ColorType.ColorA && noteData2.column > noteData3.column) || (noteData2.type == ColorType.ColorB && noteData2.column < noteData3.column)))
            {
                //Plugin.Log.Info("Hello");
                noteData2.SetNoteFlipToNote(noteData3);
                noteData3.SetNoteFlipToNote(noteData2);
            }
        }

        private void HandlePerColorTypeTimeSliceContainerDidFinishTimeSlice(EditorTimeSliceContainer<NoteEditorData> timeSliceContainer, float nextTimeSliceTime)
        {
            IReadOnlyList<NoteEditorData> items = timeSliceContainer.items;
            if (items.Count != 2)
            {
                return;
            }
            NoteEditorData noteData = items[0];
            NoteEditorData noteData2 = items[1];
            if (noteData.cutDirection != noteData2.cutDirection && noteData.cutDirection != NoteCutDirection.Any && noteData2.cutDirection != NoteCutDirection.Any)
            {
                return;
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
            Vector2 vector = StaticBeatmapObjectSpawnMovementData.Get2DNoteOffset(noteData4.column, _numberOfLines, (NoteLineLayer)noteData4.row) - StaticBeatmapObjectSpawnMovementData.Get2DNoteOffset(noteData3.column, _numberOfLines, (NoteLineLayer)noteData3.row);
            float num = ((noteData3.cutDirection == NoteCutDirection.Any) ? new Vector2(0f, 1f) : noteData3.cutDirection.Direction()).SignedAngleToLine(vector);
            if (noteData4.cutDirection == NoteCutDirection.Any && noteData3.cutDirection == NoteCutDirection.Any)
            {
                noteData3.SetField("angle", (int)num);
                noteData4.SetField("angle", (int)num);
                return;
            }
            if (Mathf.Abs(num) > 40f)
            {
                return;
            }
            noteData3.SetField("angle", (int)num);
            if (noteData4.cutDirection == NoteCutDirection.Any && !noteData3.cutDirection.IsMainDirection())
            {
                noteData4.SetField("angle", (int)num + 45);
                return;
            }
            noteData4.SetField("angle", (int)num);
        }

        private static bool SliderHeadPositionOverlapsWithNote(BaseSliderEditorData slider, NoteEditorData note)
        {
            return slider.column == note.column && slider.row == note.row;
        }

        private static bool SliderTailPositionOverlapsWithNote(BaseSliderEditorData slider, NoteEditorData note)
        {
            return slider.tailColumn == note.column && slider.tailRow == note.row;
        }

        private static bool SliderHeadPositionOverlapsWithBurstTail(BaseSliderEditorData slider, BaseSliderEditorData sliderTail)
        {
            return slider.beatmapObjectType == BeatmapObjectType.Arc && sliderTail.beatmapObjectType == BeatmapObjectType.Chain && slider.column == sliderTail.tailColumn && slider.row == sliderTail.tailRow;
        }

        private EditorTimeSliceContainer<NoteEditorData> _currentTimeSliceColorNotes = new EditorTimeSliceContainer<NoteEditorData>(8);

        private EditorTimeSliceContainer<BaseEditorData> _currentTimeSliceAllNotesAndSliders = new EditorTimeSliceContainer<BaseEditorData>(8);

        private Dictionary<ColorType, EditorTimeSliceContainer<NoteEditorData>> _currentTimeSliceNotesByColorType = new Dictionary<ColorType, EditorTimeSliceContainer<NoteEditorData>>();

        private List<BaseSliderEditorData> _unprocessedSliderTails = new List<BaseSliderEditorData>(4);

        private List<NoteEditorData>[] _notesInColumnsReusableProcessingListOfLists;

        private int _numberOfLines;

        private const float kTimeRowEpsilon = 0.001f;

        private const float kMaxNotesAlignmentAngle = 40f;

        private class EditorTimeSliceContainer<T> where T : BaseEditorData
        {
            public float time { get; private set; } = -1f;

            public float previousTimeSliceTime { get; private set; } = -1f;

            public IReadOnlyList<T> items
            {
                get
                {
                    return _items;
                }
            }

            public event Action<EditorTimeSliceContainer<T>, float> didFinishTimeSliceEvent;

            public event Action<EditorTimeSliceContainer<T>> didStartNewTimeSliceEvent;

            public event Action<EditorTimeSliceContainer<T>, T> didAddItemEvent;

            public EditorTimeSliceContainer(int capacity)
            {
                _items = new List<T>(capacity);
            }

            public void Add(T item)
            {
                if (item.beat > time + 0.001f)
                {
                    FinishTimeSlice(item.beat);
                    StartNewTimeSlice(item.beat);
                }
                _items.Add(item);
                didAddItemEvent?.Invoke(this, item);
            }

            public void AddWithoutNotifications(T item)
            {
                _items.Add(item);
            }

            public void FinishTimeSlice(float nextTimeSliceTime)
            {
                if (_items.Count > 0)
                {
                    didFinishTimeSliceEvent?.Invoke(this, nextTimeSliceTime);
                    _items.Clear();
                }
            }

            private void StartNewTimeSlice(float newSliceTime)
            {
                previousTimeSliceTime = time;
                time = newSliceTime;
                didStartNewTimeSliceEvent?.Invoke(this);
            }

            private readonly List<T> _items;
        }

        public class EditorSliderTailData : BaseEditorData
        {
            public EditorSliderTailData(BaseSliderEditorData slider)
                : base(slider.id, slider.tailBeat)
            {
                this.slider = slider;
            }

            public readonly BaseSliderEditorData slider;
        }
    }

}
