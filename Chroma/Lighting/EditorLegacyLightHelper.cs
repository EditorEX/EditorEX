using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using UnityEngine;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Chroma.Lighting
{
    internal class EditorLegacyLightHelper
    {
        internal const int RGB_INT_OFFSET = 2000000000;

        internal EditorLegacyLightHelper(IEnumerable<BasicEventEditorData> eventData)
        {
            foreach (BasicEventEditorData d in eventData)
            {
                if (d.value < RGB_INT_OFFSET)
                {
                    continue;
                }

                if (
                    !LegacyColorEvents.TryGetValue(
                        d.type,
                        out List<Tuple<float, Color>> dictionaryID
                    )
                )
                {
                    dictionaryID = new List<Tuple<float, Color>>();
                    LegacyColorEvents.Add(d.type, dictionaryID);
                }

                dictionaryID.Add(new Tuple<float, Color>(d.beat, ColorFromInt(d.value)));
            }
        }

        internal Dictionary<
            BasicBeatmapEventType,
            List<Tuple<float, Color>>
        > LegacyColorEvents
        { get; } = new();

        internal Color? GetLegacyColor(BasicEventEditorData beatmapEventData)
        {
            if (
                !LegacyColorEvents.TryGetValue(
                    beatmapEventData.type,
                    out List<Tuple<float, Color>> dictionaryID
                )
            )
            {
                return null;
            }

            List<Tuple<float, Color>> colors = dictionaryID
                .Where(n => n.Item1 <= beatmapEventData.beat)
                .ToList();
            if (colors.Count > 0)
            {
                return colors.Last().Item2;
            }

            return null;
        }

        private static Color ColorFromInt(int rgb)
        {
            rgb -= RGB_INT_OFFSET;
            int red = (rgb >> 16) & 0x0ff;
            int green = (rgb >> 8) & 0x0ff;
            int blue = rgb & 0x0ff;
            return new Color(red / 255f, green / 255f, blue / 255f);
        }
    }
}
