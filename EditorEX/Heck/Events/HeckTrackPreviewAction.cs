using System;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.EventData;
using Heck.Animation;
using UnityEngine;

namespace EditorEX.Heck.Events
{
    internal sealed class HeckTrackPreviewAction : IPreviewStateAction
    {
        private readonly EditorCoroutineEventData.CoroutineInfo _info;
        private readonly IPointDefinition? _previousPointDefinition;
        private readonly float _fromBeat;
        private readonly float _durationBeats;
        private readonly int _repeat;
        private readonly Functions _easing;
        private readonly bool _path;
        private bool _active;
        private bool _settled;
        private float _latchBeat;

        public HeckTrackPreviewAction(
            EditorCoroutineEventData.CoroutineInfo info,
            float fromBeat,
            float durationBeats,
            int repeat,
            Functions easing,
            bool path,
            IPointDefinition? previousPointDefinition = null
        )
        {
            _info = info;
            _fromBeat = fromBeat;
            _durationBeats = durationBeats;
            _repeat = repeat;
            _easing = easing;
            _path = path;
            _previousPointDefinition = previousPointDefinition;
        }

        public void Execute()
        {
            if (_active)
            {
                return;
            }

            _active = true;
            if (_path)
            {
                HeckTrackPreviewPathInit.Apply(
                    ((BasePathProperty)_info.Property).IInterpolation,
                    _previousPointDefinition,
                    _info.PointDefinition
                );
                if (_info.PointDefinition == null)
                {
                    _info.Track.UpdatedThisFrame = true;
                    PreviewOriginalTransform.RestoreUnanimated(_info.Track);
                }

                return;
            }

            if (_info.PointDefinition == null)
            {
                _info.Track.UpdatedThisFrame = true;
                _info.Property.Null();
                PreviewOriginalTransform.RestoreUnanimated(_info.Track);
            }
        }

        public void Reverse()
        {
            if (!_active)
            {
                return;
            }

            _info.Track.UpdatedThisFrame = true;
            _info.Property.Null();
            PreviewOriginalTransform.RestoreUnanimated(_info.Track);

            _settled = false;
            _active = false;
        }

        public void Tick(float beat)
        {
            if (!_active)
            {
                return;
            }

            IPointDefinition? points = _info.PointDefinition;
            if (points == null)
            {
                return;
            }

            bool hasBaseProvider = points.HasBaseProvider;
            if (HeckTrackPreviewSampler.CanSkipSample(_settled, hasBaseProvider, beat, _latchBeat))
            {
                return;
            }

            _settled = false;

            int repeat = _path ? 0 : _repeat;
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat,
                _fromBeat,
                _durationBeats,
                repeat,
                _easing,
                out bool complete
            );

            if (_path)
            {
                // Do not Finish(): that drops the previous point definition and makes
                // scrubbing back through this same interval unable to blend.
                ((BasePathProperty)_info.Property)
                    .IInterpolation
                    .Time = progress;
                if (!hasBaseProvider && complete)
                {
                    Latch(beat);
                }

                return;
            }

            SetPropertyValue(points, _info.Property, _info.Track, progress, out bool onLast);
            if (
                !hasBaseProvider
                && onLast
                && HeckTrackPreviewSampler.OnLastRepeat(
                    beat,
                    _fromBeat,
                    _durationBeats,
                    repeat,
                    complete
                )
            )
            {
                Latch(beat);
            }
        }

        private void Latch(float beat)
        {
            _settled = true;
            _latchBeat = beat;
        }

        private static void SetPropertyValue(
            IPointDefinition points,
            BaseProperty property,
            Track track,
            float time,
            out bool onLast
        )
        {
            switch (points)
            {
                case PointDefinition<float> values:
                    SetPropertyValue(values, Cast<float>(property), track, time, out onLast);
                    break;

                case PointDefinition<Vector3> values:
                    SetPropertyValue(values, Cast<Vector3>(property), track, time, out onLast);
                    break;

                case PointDefinition<Vector4> values:
                    SetPropertyValue(values, Cast<Vector4>(property), track, time, out onLast);
                    break;

                case PointDefinition<Quaternion> values:
                    SetPropertyValue(values, Cast<Quaternion>(property), track, time, out onLast);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(points));
            }

            return;

            Property<T> Cast<T>(BaseProperty toCast)
                where T : struct
            {
                return toCast as Property<T> ?? throw new InvalidOperationException();
            }
        }

        private static void SetPropertyValue(
            PointDefinition<float> points,
            Property<float> property,
            Track track,
            float time,
            out bool onLast
        )
        {
            float value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue && property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }

        private static void SetPropertyValue(
            PointDefinition<Vector3> points,
            Property<Vector3> property,
            Track track,
            float time,
            out bool onLast
        )
        {
            Vector3 value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue && property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }

        private static void SetPropertyValue(
            PointDefinition<Vector4> points,
            Property<Vector4> property,
            Track track,
            float time,
            out bool onLast
        )
        {
            Vector4 value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue && property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }

        private static void SetPropertyValue(
            PointDefinition<Quaternion> points,
            Property<Quaternion> property,
            Track track,
            float time,
            out bool onLast
        )
        {
            Quaternion value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue && property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }
    }
}
