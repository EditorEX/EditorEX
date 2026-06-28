using System.Collections.Generic;
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

        [AffinityPrefix]
        [AffinityPatch(
            typeof(EventMarkerObject),
            nameof(EventMarkerObject.Init),
            AffinityMethodType.Normal,
            null,
            typeof(BasicEventEditorData),
            typeof(Color)
        )]
        private void PrefixMarker(BasicEventEditorData? basicEventData, ref Color color)
        {
            if (basicEventData == null)
                return;
            if (TryGetMarkerColor(basicEventData, out var chromaColor))
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

        // The game only spawns a duration bar for events with an end time. Gradients
        // aren't tied to end time, so we spawn our own bar for gradient events that
        // have none, spanning the gradient's beat duration. The bar is added to the
        // spawner's own list/pool so its despawn lifecycle is handled by the game.
        [AffinityPostfix]
        [AffinityPatch(typeof(LightEventMarkerSpawner), nameof(LightEventMarkerSpawner.SpawnAt))]
        private void SpawnAtPostfix(
            LightEventMarkerSpawner __instance,
            BasicEventEditorData data,
            float xPos,
            float currentBeat,
            List<DurationEventMarkerObject> ____durationEventMarkerObjects,
            DurationEventMarkerObject.Pool ____durationEventMarkerObjectPool
        )
        {
            // The game already spawned (and we already colored) a bar in this case.
            if (data.hasEndTime)
            {
                return;
            }

            if (
                !TryGetChromaData(data, out EditorChromaEventData chromaData)
                || chromaData.GradientObject == null
            )
            {
                return;
            }

            float startPos = __instance._beatmapObjectPlacementHelper.BeatToPosition(
                data.beat,
                currentBeat
            );
            float endPos = __instance._beatmapObjectPlacementHelper.BeatToPosition(
                data.beat + chromaData.GradientObject.Duration,
                currentBeat
            );

            DurationEventMarkerObject bar = ____durationEventMarkerObjectPool.Spawn();
            ____durationEventMarkerObjects.Add(bar);
            bar.Init(
                data,
                chromaData.GradientObject.StartColor,
                chromaData.GradientObject.EndColor
            );
            bar.SetScaleZ(endPos - startPos);
            bar.transform.localPosition = new Vector3(xPos, -0.125f, startPos);
        }

        // The game's UpdateDurationEvents rescales every bar using basicEventData.endBeat,
        // which is meaningless for our gradient bars (the event has no end time). Recompute
        // their scale from the gradient duration after the game has run.
        [AffinityPostfix]
        [AffinityPatch(
            typeof(LightEventMarkerSpawner),
            nameof(LightEventMarkerSpawner.UpdateDurationEvents)
        )]
        private void UpdateDurationEventsPostfix(
            LightEventMarkerSpawner __instance,
            List<DurationEventMarkerObject> ____durationEventMarkerObjects
        )
        {
            foreach (DurationEventMarkerObject bar in ____durationEventMarkerObjects)
            {
                BasicEventEditorData data = bar.basicEventData;
                if (
                    data == null
                    || data.hasEndTime
                    || !TryGetChromaData(data, out EditorChromaEventData chromaData)
                    || chromaData.GradientObject == null
                )
                {
                    continue;
                }

                float startPos = __instance._beatmapObjectPlacementHelper.BeatToPosition(data.beat);
                float endPos = __instance._beatmapObjectPlacementHelper.BeatToPosition(
                    data.beat + chromaData.GradientObject.Duration
                );
                bar.SetScaleZ(endPos - startPos);
            }
        }
    }
}
