using System;
using System.Collections.Generic;
using BeatmapEditor3D;
using CustomJSONData.CustomBeatmap;
using UnityEngine;

namespace EditorEX.MapData.Bookmarks
{
    public static class CustomDataBookmarkCodec
    {
        public const string V2Key = "_bookmarks";
        public const string V3Key = "bookmarks";

        public static List<CustomDataBookmark> Read(CustomData? customData, bool v3)
        {
            var result = new List<CustomDataBookmark>();
            if (customData == null)
            {
                return result;
            }

            var entries =
                customData.Get<List<object>>(v3 ? V3Key : V2Key)
                ?? customData.Get<List<object>>(v3 ? V2Key : V3Key);
            if (entries == null)
            {
                return result;
            }

            foreach (var raw in entries)
            {
                if (raw is not CustomData entry)
                {
                    continue;
                }

                float? beat = ReadFloat(entry, v3 ? "b" : "_time", v3 ? "_time" : "b", "time");
                if (beat == null)
                {
                    continue;
                }

                string name =
                    ReadString(entry, v3 ? "n" : "_name", v3 ? "_name" : "n", "name") ?? "";
                bool hasColor = TryReadColor(entry, out Color color);
                result.Add(
                    new CustomDataBookmark
                    {
                        Beat = beat.Value,
                        Name = name,
                        Color = color,
                        HasColor = hasColor,
                    }
                );
            }

            return result;
        }

        public static void Write(CustomData customData, BookmarksDataModel model, bool v3)
        {
            Write(customData, Flatten(model), v3);
        }

        public static void Write(
            CustomData customData,
            IReadOnlyList<CustomDataBookmark> bookmarks,
            bool v3
        )
        {
            string key = v3 ? V3Key : V2Key;
            string otherKey = v3 ? V2Key : V3Key;
            customData.TryRemove(otherKey, out _);

            if (bookmarks.Count == 0)
            {
                customData.TryRemove(key, out _);
                return;
            }

            var list = new List<object>(bookmarks.Count);
            foreach (var bookmark in bookmarks)
            {
                var entry = new CustomData();
                if (v3)
                {
                    entry["b"] = bookmark.Beat;
                    entry["n"] = bookmark.Name ?? "";
                    entry["c"] = ColorToList(bookmark.Color);
                }
                else
                {
                    entry["_time"] = bookmark.Beat;
                    entry["_name"] = bookmark.Name ?? "";
                    entry["_color"] = ColorToList(bookmark.Color);
                }

                list.Add(entry);
            }

            customData[key] = list;
        }

        public static List<CustomDataBookmark> Flatten(BookmarksDataModel model)
        {
            var result = new List<CustomDataBookmark>();
            foreach (var set in model.bookmarkSetById.Values)
            {
                if (!model.bookmarksListBySetId.TryGetValue(set.id, out var bookmarks))
                {
                    continue;
                }

                foreach (var bookmark in bookmarks)
                {
                    string name = !string.IsNullOrEmpty(bookmark.label)
                        ? bookmark.label
                        : bookmark.text ?? "";
                    result.Add(
                        new CustomDataBookmark
                        {
                            Beat = bookmark.beat,
                            Name = name,
                            Color = set.color,
                            HasColor = true,
                        }
                    );
                }
            }

            result.Sort((a, b) => a.Beat.CompareTo(b.Beat));
            return result;
        }

        private static List<object> ColorToList(Color color)
        {
            return new List<object> { color.r, color.g, color.b };
        }

        private static float? ReadFloat(CustomData data, params string[] keys)
        {
            foreach (string key in keys)
            {
                object? value = data.Get<object>(key);
                if (value == null)
                {
                    continue;
                }

                try
                {
                    return Convert.ToSingle(value);
                }
                catch (Exception)
                {
                    // Try the next key.
                }
            }

            return null;
        }

        private static string? ReadString(CustomData data, params string[] keys)
        {
            foreach (string key in keys)
            {
                string? value = data.Get<string>(key);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        private static bool TryReadColor(CustomData entry, out Color color)
        {
            var list = entry.Get<List<object>>("c") ?? entry.Get<List<object>>("_color");
            if (list != null && list.Count >= 3)
            {
                try
                {
                    color = new Color(
                        Convert.ToSingle(list[0]),
                        Convert.ToSingle(list[1]),
                        Convert.ToSingle(list[2]),
                        list.Count > 3 ? Convert.ToSingle(list[3]) : 1f
                    );
                    return true;
                }
                catch (Exception)
                {
                    // Fall through to hex.
                }
            }

            string? hex = entry.Get<string>("c") ?? entry.Get<string>("_color");
            if (!string.IsNullOrEmpty(hex))
            {
                if (hex![0] != '#')
                {
                    hex = "#" + hex;
                }

                if (ColorUtility.TryParseHtmlString(hex, out color))
                {
                    return true;
                }
            }

            color = default;
            return false;
        }
    }
}
