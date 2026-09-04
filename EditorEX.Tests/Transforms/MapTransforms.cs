using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.LevelDataLoaders;
using EditorEX.Tests.Harness;

namespace EditorEX.Tests.Transforms
{
    public static class MapTransforms
    {
        public const string TestCustomDataKey = "editorexTest";

        public static void ShiftBeats(
            DifficultyLoadResult loaded,
            ICustomDataRepository repo,
            float delta
        )
        {
            loaded.Notes = loaded
                .Notes.Select(n =>
                    Rebind(
                        repo,
                        n,
                        NoteEditorData.CopyWithModifications(n, beat: n.beat + delta)
                    )
                )
                .ToList();
            loaded.Obstacles = loaded
                .Obstacles.Select(o =>
                    Rebind(
                        repo,
                        o,
                        ObstacleEditorData.CopyWithModifications(o, beat: o.beat + delta)
                    )
                )
                .ToList();
            loaded.Arcs = loaded
                .Arcs.Select(a =>
                    Rebind(
                        repo,
                        a,
                        ArcEditorData.CopyWithModifications(
                            a,
                            beat: a.beat + delta,
                            tailBeat: a.tailBeat + delta
                        )
                    )
                )
                .ToList();
            loaded.Chains = loaded
                .Chains.Select(c =>
                    Rebind(
                        repo,
                        c,
                        ChainEditorData.CopyWithModifications(
                            c,
                            beat: c.beat + delta,
                            tailBeat: c.tailBeat + delta
                        )
                    )
                )
                .ToList();
            loaded.Waypoints = loaded
                .Waypoints.Select(w =>
                    Rebind(
                        repo,
                        w,
                        WaypointEditorData.CopyWithModifications(w, beat: w.beat + delta)
                    )
                )
                .ToList();
            loaded.BasicEvents = loaded
                .BasicEvents.Select(e =>
                    Rebind(
                        repo,
                        e,
                        BasicEventEditorData.CreateNew(e.type, e.beat + delta, e.value, e.floatValue)
                    )
                )
                .ToList();
            loaded.BpmChanges = loaded
                .BpmChanges.Select(b => new BpmChangeEventData(b.beat + delta, b.bpm))
                .ToList();

            var shiftedEvents = new List<CustomEventEditorData>();
            foreach (CustomEventEditorData evt in repo.GetCustomEvents() ?? new())
            {
                var copy = CustomEventEditorData.CreateNew(
                    evt.beat + delta,
                    evt.eventType,
                    evt.customData,
                    evt.version2_6_0AndEarlier
                );
                Rebind(repo, evt, copy);
                shiftedEvents.Add(copy);
            }

            repo.SetCustomEvents(shiftedEvents);

            CustomData? beatmapCustom =
                repo.GetBeatmapData()?.customData ?? repo.GetCustomBeatmapSaveData()?.customData;
            if (beatmapCustom != null)
            {
                List<CustomDataBookmark> bookmarks = CustomDataBookmarkCodec.Read(
                    beatmapCustom,
                    v3: true
                );
                foreach (CustomDataBookmark bookmark in bookmarks)
                {
                    bookmark.Beat += delta;
                }

                CustomDataBookmarkCodec.Write(beatmapCustom, bookmarks, v3: true);
            }
        }

        public static NoteEditorData AddColorNote(
            DifficultyLoadResult loaded,
            ICustomDataRepository repo
        )
        {
            float beat = loaded.Notes.Count == 0 ? 32f : loaded.Notes.Max(n => n.beat) + 4f;
            NoteEditorData note = NoteEditorData.CreateNew(
                beat,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
            repo.AddCustomData(note, new CustomData());
            loaded.Notes.Add(note);
            return note;
        }

        public static NoteEditorData? RemoveOneNote(DifficultyLoadResult loaded)
        {
            NoteEditorData? note =
                loaded.Notes.FirstOrDefault(n => n.noteType == NoteType.Note)
                ?? loaded.Notes.FirstOrDefault();
            if (note == null)
            {
                return null;
            }

            loaded.Notes.Remove(note);
            return note;
        }

        public static void AddAndStripCustomData(
            DifficultyLoadResult loaded,
            ICustomDataRepository repo
        )
        {
            NoteEditorData? note = loaded.Notes.FirstOrDefault(n => n.noteType == NoteType.Note);
            if (note == null)
            {
                return;
            }

            CustomData existing = repo.GetCustomData(note) ?? new CustomData();
            var next = new CustomData(existing);
            string? removable = next.Keys.FirstOrDefault(k => k != TestCustomDataKey);
            if (removable != null)
            {
                next.TryRemove(removable, out _);
            }

            next[TestCustomDataKey] = true;
            repo.AddCustomData(note, next);
        }

        private static T Rebind<T>(ICustomDataRepository repo, BaseEditorData original, T copy)
            where T : BaseEditorData
        {
            CustomData custom = repo.GetCustomData(original);
            if (custom != null)
            {
                repo.AddCustomData(copy, custom);
            }

            return copy;
        }
    }
}
