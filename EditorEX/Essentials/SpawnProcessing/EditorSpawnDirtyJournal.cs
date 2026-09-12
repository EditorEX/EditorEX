using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.LevelEditor;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.Patches;
using Heck;
using NoodleExtensions;
using SiraUtil.Affinity;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal sealed class EditorSpawnDirtyJournal : IAffinity
    {
        private readonly EditorSpawnDirtyTracker _tracker;

        [Inject]
        internal EditorSpawnDirtyJournal(EditorSpawnDirtyTracker tracker)
        {
            _tracker = tracker;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.AddNote))]
        private void AddNote(NoteEditorData note)
        {
            _tracker.NoteAdded(note);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.RemoveNote))]
        private void RemoveNote(NoteEditorData note)
        {
            _tracker.NoteRemoved(note);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.AddNotes))]
        private void AddNotes(IEnumerable<NoteEditorData> notes)
        {
            foreach (NoteEditorData note in notes)
            {
                _tracker.NoteAdded(note);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatmapObjectsDataModel),
            nameof(BeatmapObjectsDataModel.RemoveNotes)
        )]
        private void RemoveNotes(IEnumerable<NoteEditorData> notes)
        {
            foreach (NoteEditorData note in notes)
            {
                _tracker.NoteRemoved(note);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.AddArc))]
        private void AddArc(ArcEditorData arc)
        {
            _tracker.SliderAdded(arc);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.RemoveArc))]
        private void RemoveArc(ArcEditorData arc)
        {
            _tracker.SliderRemoved(arc);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.AddArcs))]
        private void AddArcs(IEnumerable<ArcEditorData> arcs)
        {
            foreach (ArcEditorData arc in arcs)
            {
                _tracker.SliderAdded(arc);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.RemoveArcs))]
        private void RemoveArcs(IEnumerable<ArcEditorData> arcs)
        {
            foreach (ArcEditorData arc in arcs)
            {
                _tracker.SliderRemoved(arc);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.AddChain))]
        private void AddChain(ChainEditorData chain)
        {
            _tracker.SliderAdded(chain);
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatmapObjectsDataModel),
            nameof(BeatmapObjectsDataModel.RemoveChain)
        )]
        private void RemoveChain(ChainEditorData chain)
        {
            _tracker.SliderRemoved(chain);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsDataModel), nameof(BeatmapObjectsDataModel.AddChains))]
        private void AddChains(IEnumerable<ChainEditorData> chains)
        {
            foreach (ChainEditorData chain in chains)
            {
                _tracker.SliderAdded(chain);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatmapObjectsDataModel),
            nameof(BeatmapObjectsDataModel.RemoveChains)
        )]
        private void RemoveChains(IEnumerable<ChainEditorData> chains)
        {
            foreach (ChainEditorData chain in chains)
            {
                _tracker.SliderRemoved(chain);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(CustomDataRepository), nameof(CustomDataRepository.AddCustomData))]
        private void AddCustomData(BaseEditorData? data, CustomData customData)
        {
            if (!_tracker.IsReady || data == null || customData == null)
            {
                return;
            }

            if (
                !customData.ContainsKey(NOTE_OFFSET)
                && !customData.ContainsKey(HeckController.V2_POSITION)
                && !customData.ContainsKey(FLIP)
                && !customData.ContainsKey(V2_FLIP)
                && !customData.ContainsKey(TAIL_NOTE_OFFSET)
            )
            {
                return;
            }

            _tracker.MarkExistingDirty(data);
            //BindRebuildEditorSpawnDataCommandPatch.CommandSignalBus?.Fire<BeatmapLevelUpdatedSignal>();
        }
    }
}
