using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using Chroma;
using EditorEX.Chroma.Events;
using EditorEX.Heck.Deserialize;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace EditorEX.Chroma.Patches.Events
{
    // Colors the timeline event markers (the dot markers and the duration bars)
    // using Chroma custom color data read from the deserialized event data.
    // The marker objects carry the BasicEventEditorData directly, so we can
    // resolve the Chroma color without going through the live preview conversion.
    internal class EditorEventMarkerColorizer : IAffinity
    {
        private readonly EditorDeserializedData _editorDeserializedData;

        private EditorEventMarkerColorizer(
            [InjectOptional(Id = ChromaController.ID)] EditorDeserializedData deserializedData
        )
        {
            _editorDeserializedData = deserializedData;
        }

        private bool TryGetChromaData(
            BasicEventEditorData basicEventData,
            out EditorChromaEventData chromaData
        )
        {
            chromaData = null;
            return _editorDeserializedData != null
                && _editorDeserializedData.Resolve(basicEventData, out chromaData)
                && chromaData != null;
        }

        // The static color shown on the dot marker. A solid color takes priority;
        // otherwise a gradient is represented by the color at the event's own beat
        // (the gradient's start color).
        private bool TryGetMarkerColor(BasicEventEditorData basicEventData, out Color color)
        {
            color = default;
            if (!TryGetChromaData(basicEventData, out EditorChromaEventData chromaData))
            {
                return false;
            }

            if (chromaData.ColorData.HasValue)
            {
                color = chromaData.ColorData.Value;
                return true;
            }

            if (chromaData.GradientObject != null)
            {
                color = chromaData.GradientObject.StartColor;
                return true;
            }

            return false;
        }

        // The dot marker (LightEventMarkerObject and friends go through this virtual base).
        [AffinityPrefix]
        [AffinityPatch(
            typeof(EventMarkerObject),
            nameof(EventMarkerObject.Init),
            AffinityMethodType.Normal,
            null,
            typeof(BasicEventEditorData),
            typeof(Color)
        )]
        private void PrefixMarker(BasicEventEditorData basicEventData, ref Color color)
        {
            if (TryGetMarkerColor(basicEventData, out Color chromaColor))
            {
                color = chromaColor;
            }
        }

        // The duration/fade bar. The shader lerps from colorA to colorB across the
        // bar, so a gradient is represented start -> end. A solid Chroma color
        // overrides only the primary color (colorB keeps the game's end-value logic).
        [AffinityPrefix]
        [AffinityPatch(
            typeof(DurationEventMarkerObject),
            nameof(DurationEventMarkerObject.Init),
            AffinityMethodType.Normal,
            null,
            typeof(BasicEventEditorData),
            typeof(Color),
            typeof(Color)
        )]
        private void PrefixDuration(
            BasicEventEditorData basicEventData,
            ref Color colorA,
            ref Color colorB
        )
        {
            if (!TryGetChromaData(basicEventData, out EditorChromaEventData chromaData))
            {
                return;
            }

            if (chromaData.GradientObject != null)
            {
                colorA = chromaData.GradientObject.StartColor;
                colorB = chromaData.GradientObject.EndColor;
            }
            else if (chromaData.ColorData.HasValue)
            {
                colorA = chromaData.ColorData.Value;
            }
        }
    }
}
