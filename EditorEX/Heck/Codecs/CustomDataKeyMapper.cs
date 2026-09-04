using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;

namespace EditorEX.Heck.Codecs
{
    internal static class CustomDataKeyMapper
    {
        public static void MoveKey(CustomData json, string from, string to)
        {
            if (from == to)
            {
                return;
            }

            if (!json.TryGetValue(from, out object? value))
            {
                return;
            }

            json[to] = value;
            json.TryRemove(from, out _);
        }

        public static Dictionary<string, string> InvertMap(
            IReadOnlyDictionary<string, string> fromTo
        )
        {
            var result = new Dictionary<string, string>(fromTo.Count);
            foreach (var pair in fromTo)
            {
                result[pair.Value] = pair.Key;
            }

            return result;
        }

        public static void RemapKeys(CustomData json, IReadOnlyDictionary<string, string> fromTo)
        {
            foreach (var pair in fromTo)
            {
                MoveKey(json, pair.Key, pair.Value);
            }
        }

        public static void RemapNested(
            CustomData json,
            string nestedKey,
            IReadOnlyDictionary<string, string> fromTo
        )
        {
            CustomData? nested = json.Get<CustomData>(nestedKey);
            if (nested == null)
            {
                return;
            }

            RemapKeys(nested, fromTo);
        }

        public static void InvertBoolean(CustomData json, string from, string to, bool invert)
        {
            if (!json.TryGetValue(from, out object? raw) || raw is not bool value)
            {
                return;
            }

            json[to] = invert ? !value : value;
            if (from != to)
            {
                json.TryRemove(from, out _);
            }
        }
    }
}
