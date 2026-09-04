using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EditorEX.Tests.Harness
{
    public static class LoadedMapComparer
    {
        public const float Epsilon = 1e-4f;

        public static string? Diff(LoadedMapSnapshot expected, LoadedMapSnapshot actual)
        {
            var errors = new List<string>();
            if (
                expected.UseNormalEventsAsCompatibleEvents
                != actual.UseNormalEventsAsCompatibleEvents
            )
            {
                errors.Add(
                    $"UseNormalEventsAsCompatibleEvents expected {expected.UseNormalEventsAsCompatibleEvents} but was {actual.UseNormalEventsAsCompatibleEvents}"
                );
            }

            CompareList(errors, "notes", expected.Notes, actual.Notes, CompareNotes);
            CompareList(errors, "obstacles", expected.Obstacles, actual.Obstacles, CompareObstacles);
            CompareList(errors, "arcs", expected.Arcs, actual.Arcs, CompareArcs);
            CompareList(errors, "chains", expected.Chains, actual.Chains, CompareChains);
            CompareList(errors, "waypoints", expected.Waypoints, actual.Waypoints, CompareWaypoints);
            CompareList(errors, "events", expected.Events, actual.Events, CompareEvents);
            CompareList(
                errors,
                "customEvents",
                expected.CustomEvents,
                actual.CustomEvents,
                CompareCustomEvents
            );
            CompareList(errors, "bookmarks", expected.Bookmarks, actual.Bookmarks, CompareBookmarks);
            CompareList(errors, "bpmChanges", expected.BpmChanges, actual.BpmChanges, CompareBpm);
            CompareList(
                errors,
                "eventBoxGroups",
                expected.EventBoxGroups,
                actual.EventBoxGroups,
                CompareEventBoxGroups
            );
            CompareList(errors, "keywords", expected.Keywords, actual.Keywords, CompareKeywords);

            if (errors.Count == 0)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.AppendLine("Loaded map snapshots differ:");
            foreach (string error in errors)
            {
                builder.AppendLine("  - " + error);
            }

            return builder.ToString();
        }

        public static string Format(LoadedMapSnapshot snapshot)
        {
            return JsonConvert.SerializeObject(snapshot, Formatting.Indented);
        }

        private static void CompareList<T>(
            List<string> errors,
            string name,
            IReadOnlyList<T> expected,
            IReadOnlyList<T> actual,
            Action<List<string>, string, T, T> compareItem
        )
        {
            if (expected.Count != actual.Count)
            {
                errors.Add($"{name} count expected {expected.Count} but was {actual.Count}");
                int limit = Math.Min(expected.Count, actual.Count);
                for (int i = 0; i < limit; i++)
                {
                    compareItem(errors, $"{name}[{i}]", expected[i], actual[i]);
                }

                return;
            }

            for (int i = 0; i < expected.Count; i++)
            {
                compareItem(errors, $"{name}[{i}]", expected[i], actual[i]);
            }
        }

        private static void CompareNotes(
            List<string> errors,
            string path,
            LoadedMapSnapshot.NoteRecord expected,
            LoadedMapSnapshot.NoteRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".column", expected.Column, actual.Column);
            Compare(errors, path + ".row", expected.Row, actual.Row);
            Compare(errors, path + ".rotation", expected.Rotation, actual.Rotation);
            Compare(errors, path + ".noteType", expected.NoteType, actual.NoteType);
            Compare(errors, path + ".colorType", expected.ColorType, actual.ColorType);
            Compare(errors, path + ".cutDirection", expected.CutDirection, actual.CutDirection);
            Compare(errors, path + ".angle", expected.Angle, actual.Angle);
            CompareToken(errors, path + ".customData", expected.CustomData, actual.CustomData);
        }

        private static void CompareObstacles(
            List<string> errors,
            string path,
            LoadedMapSnapshot.ObstacleRecord expected,
            LoadedMapSnapshot.ObstacleRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".column", expected.Column, actual.Column);
            Compare(errors, path + ".row", expected.Row, actual.Row);
            Compare(errors, path + ".rotation", expected.Rotation, actual.Rotation);
            CompareFloat(errors, path + ".duration", expected.Duration, actual.Duration);
            Compare(errors, path + ".width", expected.Width, actual.Width);
            Compare(errors, path + ".height", expected.Height, actual.Height);
            CompareToken(errors, path + ".customData", expected.CustomData, actual.CustomData);
        }

        private static void CompareArcs(
            List<string> errors,
            string path,
            LoadedMapSnapshot.ArcRecord expected,
            LoadedMapSnapshot.ArcRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".column", expected.Column, actual.Column);
            Compare(errors, path + ".row", expected.Row, actual.Row);
            Compare(errors, path + ".rotation", expected.Rotation, actual.Rotation);
            CompareFloat(errors, path + ".tailBeat", expected.TailBeat, actual.TailBeat);
            Compare(errors, path + ".tailColumn", expected.TailColumn, actual.TailColumn);
            Compare(errors, path + ".tailRow", expected.TailRow, actual.TailRow);
            Compare(errors, path + ".tailRotation", expected.TailRotation, actual.TailRotation);
            Compare(errors, path + ".colorType", expected.ColorType, actual.ColorType);
            Compare(errors, path + ".cutDirection", expected.CutDirection, actual.CutDirection);
            Compare(
                errors,
                path + ".tailCutDirection",
                expected.TailCutDirection,
                actual.TailCutDirection
            );
            CompareFloat(errors, path + ".controlPoint", expected.ControlPoint, actual.ControlPoint);
            CompareFloat(
                errors,
                path + ".tailControlPoint",
                expected.TailControlPoint,
                actual.TailControlPoint
            );
            Compare(errors, path + ".midAnchorMode", expected.MidAnchorMode, actual.MidAnchorMode);
            CompareToken(errors, path + ".customData", expected.CustomData, actual.CustomData);
        }

        private static void CompareChains(
            List<string> errors,
            string path,
            LoadedMapSnapshot.ChainRecord expected,
            LoadedMapSnapshot.ChainRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".column", expected.Column, actual.Column);
            Compare(errors, path + ".row", expected.Row, actual.Row);
            Compare(errors, path + ".rotation", expected.Rotation, actual.Rotation);
            CompareFloat(errors, path + ".tailBeat", expected.TailBeat, actual.TailBeat);
            Compare(errors, path + ".tailColumn", expected.TailColumn, actual.TailColumn);
            Compare(errors, path + ".tailRow", expected.TailRow, actual.TailRow);
            Compare(errors, path + ".tailRotation", expected.TailRotation, actual.TailRotation);
            Compare(errors, path + ".colorType", expected.ColorType, actual.ColorType);
            Compare(errors, path + ".cutDirection", expected.CutDirection, actual.CutDirection);
            Compare(errors, path + ".sliceCount", expected.SliceCount, actual.SliceCount);
            CompareFloat(errors, path + ".squishAmount", expected.SquishAmount, actual.SquishAmount);
            CompareToken(errors, path + ".customData", expected.CustomData, actual.CustomData);
        }

        private static void CompareWaypoints(
            List<string> errors,
            string path,
            LoadedMapSnapshot.WaypointRecord expected,
            LoadedMapSnapshot.WaypointRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".column", expected.Column, actual.Column);
            Compare(errors, path + ".row", expected.Row, actual.Row);
            Compare(errors, path + ".rotation", expected.Rotation, actual.Rotation);
            Compare(
                errors,
                path + ".offsetDirection",
                expected.OffsetDirection,
                actual.OffsetDirection
            );
            CompareToken(errors, path + ".customData", expected.CustomData, actual.CustomData);
        }

        private static void CompareEvents(
            List<string> errors,
            string path,
            LoadedMapSnapshot.EventRecord expected,
            LoadedMapSnapshot.EventRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".type", expected.Type, actual.Type);
            Compare(errors, path + ".value", expected.Value, actual.Value);
            CompareFloat(errors, path + ".floatValue", expected.FloatValue, actual.FloatValue);
            CompareToken(errors, path + ".customData", expected.CustomData, actual.CustomData);
        }

        private static void CompareCustomEvents(
            List<string> errors,
            string path,
            LoadedMapSnapshot.CustomEventRecord expected,
            LoadedMapSnapshot.CustomEventRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".type", expected.Type, actual.Type);
            CompareToken(errors, path + ".data", expected.Data, actual.Data);
        }

        private static void CompareBookmarks(
            List<string> errors,
            string path,
            LoadedMapSnapshot.BookmarkRecord expected,
            LoadedMapSnapshot.BookmarkRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".name", expected.Name, actual.Name);
            Compare(errors, path + ".hasColor", expected.HasColor, actual.HasColor);
            if (expected.HasColor || actual.HasColor)
            {
                CompareFloat(errors, path + ".r", expected.R, actual.R);
                CompareFloat(errors, path + ".g", expected.G, actual.G);
                CompareFloat(errors, path + ".b", expected.B, actual.B);
            }
        }

        private static void CompareBpm(
            List<string> errors,
            string path,
            LoadedMapSnapshot.BpmRecord expected,
            LoadedMapSnapshot.BpmRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            CompareFloat(errors, path + ".bpm", expected.Bpm, actual.Bpm);
        }

        private static void CompareEventBoxGroups(
            List<string> errors,
            string path,
            LoadedMapSnapshot.EventBoxGroupRecord expected,
            LoadedMapSnapshot.EventBoxGroupRecord actual
        )
        {
            CompareFloat(errors, path + ".beat", expected.Beat, actual.Beat);
            Compare(errors, path + ".groupId", expected.GroupId, actual.GroupId);
            Compare(errors, path + ".type", expected.Type, actual.Type);
            CompareList(errors, path + ".boxes", expected.Boxes, actual.Boxes, CompareEventBoxes);
        }

        private static void CompareEventBoxes(
            List<string> errors,
            string path,
            LoadedMapSnapshot.EventBoxRecord expected,
            LoadedMapSnapshot.EventBoxRecord actual
        )
        {
            Compare(errors, path + ".kind", expected.Kind, actual.Kind);
            Compare(errors, path + ".filter", expected.Filter, actual.Filter);
            CompareFloat(
                errors,
                path + ".beatDistribution",
                expected.BeatDistribution,
                actual.BeatDistribution
            );
            Compare(
                errors,
                path + ".beatDistributionType",
                expected.BeatDistributionType,
                actual.BeatDistributionType
            );
            Compare(errors, path + ".extra", expected.Extra, actual.Extra);
            CompareList(
                errors,
                path + ".baseEvents",
                expected.BaseEvents,
                actual.BaseEvents,
                (list, itemPath, a, b) => Compare(list, itemPath, a, b)
            );
        }

        private static void CompareKeywords(
            List<string> errors,
            string path,
            LoadedMapSnapshot.KeywordRecord expected,
            LoadedMapSnapshot.KeywordRecord actual
        )
        {
            Compare(errors, path + ".keyword", expected.Keyword, actual.Keyword);
            CompareList(
                errors,
                path + ".eventTypes",
                expected.EventTypes,
                actual.EventTypes,
                (list, itemPath, a, b) => Compare(list, itemPath, a, b)
            );
        }

        private static void Compare<T>(List<string> errors, string path, T expected, T actual)
        {
            if (!Equals(expected, actual))
            {
                errors.Add($"{path} expected {expected} but was {actual}");
            }
        }

        private static void CompareFloat(List<string> errors, string path, float expected, float actual)
        {
            if (Math.Abs(expected - actual) > Epsilon)
            {
                errors.Add(
                    $"{path} expected {expected.ToString("G9", CultureInfo.InvariantCulture)} but was {actual.ToString("G9", CultureInfo.InvariantCulture)}"
                );
            }
        }

        private static void CompareToken(
            List<string> errors,
            string path,
            JToken expected,
            JToken actual
        )
        {
            if (!JToken.DeepEquals(expected, actual))
            {
                errors.Add($"{path} expected {expected} but was {actual}");
            }
        }
    }
}
