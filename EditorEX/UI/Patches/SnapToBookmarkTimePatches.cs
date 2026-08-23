using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using SiraUtil.Affinity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.UI.Patches
{
    internal class SnapToBookmarkTimePatches : IAffinity
    {
        private const float MaxClickDistancePx = 16f;
        private const float FreeDragSlopPx = 5f;

        [AffinityPatch(
            typeof(AudioWaveformViewGenericInputController),
            nameof(AudioWaveformViewGenericInputController.OnPointerClick)
        )]
        [AffinityPrefix]
        private bool PrefixOnPointerClick(
            AudioWaveformViewGenericInputController __instance,
            PointerEventData eventData
        )
        {
            return !TrySeekToNearbyBookmark(__instance, eventData);
        }

        [AffinityPatch(
            typeof(AudioWaveformViewGenericInputController),
            nameof(AudioWaveformViewGenericInputController.UpdatePlayHead)
        )]
        [AffinityPrefix]
        private bool PrefixUpdatePlayHead(
            AudioWaveformViewGenericInputController __instance,
            PointerEventData eventData,
            UpdatePlayHeadSignal.SnapType snapType
        )
        {
            if (IsFreeDrag(eventData, snapType))
            {
                return true;
            }

            return !TrySeekToNearbyBookmark(__instance, eventData);
        }

        private static bool IsFreeDrag(
            PointerEventData eventData,
            UpdatePlayHeadSignal.SnapType snapType
        )
        {
            if (snapType != UpdatePlayHeadSignal.SnapType.None)
            {
                return false;
            }

            Vector2 delta = eventData.position - eventData.pressPosition;
            return delta.sqrMagnitude > FreeDragSlopPx * FreeDragSlopPx;
        }

        private static bool TrySeekToNearbyBookmark(
            AudioWaveformViewGenericInputController controller,
            PointerEventData eventData
        )
        {
            var bookmarksView = controller.GetComponentInChildren<AudioWaveformBookmarksView>(true);
            if (bookmarksView == null)
            {
                return false;
            }

            float clickX = eventData.position.x;
            float bestDistance = MaxClickDistancePx;
            float? bestBeat = null;

            foreach (var pair in bookmarksView._bookmarkMarkers)
            {
                var marker = pair.Value;
                if (marker == null || !marker.isActiveAndEnabled)
                {
                    continue;
                }

                float markerX = RectTransformUtility
                    .WorldToScreenPoint(null, marker.rectTransform.position)
                    .x;
                float distance = clickX - markerX;
                if (distance >= bestDistance || distance < 0)
                {
                    continue;
                }

                if (!bookmarksView._bookmarkMarkerData.TryGetValue(pair.Key, out var data))
                {
                    continue;
                }

                bestDistance = distance;
                bestBeat = data.beat;
            }

            if (bestBeat == null)
            {
                return false;
            }

            controller._signalBus.Fire(
                new UpdatePlayHeadSignal(bestBeat.Value, UpdatePlayHeadSignal.SnapType.None, true)
            );
            return true;
        }
    }
}
