using System;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.EventData;
using Heck.Animation;
using UnityEngine;

namespace EditorEX.Heck.Events
{
    internal sealed class HeckTrackPreviewAction : IPreviewStateAction, IPreviewStateSampling
    {
        private readonly EditorCoroutineEventData.CoroutineInfo _info;
        private readonly IPointDefinition? _points;
        private readonly IPointDefinition? _previousPointDefinition;
        private readonly BasePathProperty? _pathProperty;
        private readonly PointDefinition<float>? _floatPoints;
        private readonly Property<float>? _floatProperty;
        private readonly PointDefinition<Vector3>? _vector3Points;
        private readonly Property<Vector3>? _vector3Property;
        private readonly PointDefinition<Vector4>? _vector4Points;
        private readonly Property<Vector4>? _vector4Property;
        private readonly PointDefinition<Quaternion>? _quaternionPoints;
        private readonly Property<Quaternion>? _quaternionProperty;
        private readonly float _fromBeat;
        private readonly float _durationBeats;
        private readonly int _repeat;
        private readonly Functions _easing;
        private readonly bool _path;
        private readonly bool _hasBaseProvider;
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
            _points = info.PointDefinition;
            _fromBeat = fromBeat;
            _durationBeats = durationBeats;
            _repeat = repeat;
            _easing = easing;
            _path = path;
            _previousPointDefinition = previousPointDefinition;
            _hasBaseProvider = _points?.HasBaseProvider ?? false;
            if (path)
            {
                _pathProperty = (BasePathProperty)info.Property;
                return;
            }

            switch (_points)
            {
                case PointDefinition<float> values:
                    _floatPoints = values;
                    _floatProperty = (Property<float>)info.Property;
                    break;
                case PointDefinition<Vector3> values:
                    _vector3Points = values;
                    _vector3Property = (Property<Vector3>)info.Property;
                    break;
                case PointDefinition<Vector4> values:
                    _vector4Points = values;
                    _vector4Property = (Property<Vector4>)info.Property;
                    break;
                case PointDefinition<Quaternion> values:
                    _quaternionPoints = values;
                    _quaternionProperty = (Property<Quaternion>)info.Property;
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info));
            }
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
                    _pathProperty!.IInterpolation,
                    _previousPointDefinition,
                    _points
                );
                if (_points == null)
                {
                    _info.Track.UpdatedThisFrame = true;
                    PreviewOriginalTransform.RestoreUnanimated(_info.Track);
                }

                return;
            }

            if (_points == null)
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

        public bool WantsTick(float beat)
        {
            return _active
                && _points != null
                && !HeckTrackPreviewSampler.CanSkipSample(
                    _settled,
                    _hasBaseProvider,
                    beat,
                    _latchBeat
                );
        }

        public void Tick(float beat)
        {
            if (!WantsTick(beat))
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
                _pathProperty!.IInterpolation.Time = progress;
                if (!_hasBaseProvider && complete)
                {
                    Latch(beat);
                }

                return;
            }

            SetPropertyValue(progress, out bool onLast);
            if (
                !_hasBaseProvider
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

        private void SetPropertyValue(float time, out bool onLast)
        {
            if (_floatPoints != null)
            {
                SetPropertyValue(_floatPoints, _floatProperty!, _info.Track, time, out onLast);
                return;
            }

            if (_vector3Points != null)
            {
                SetPropertyValue(_vector3Points, _vector3Property!, _info.Track, time, out onLast);
                return;
            }

            if (_vector4Points != null)
            {
                SetPropertyValue(_vector4Points, _vector4Property!, _info.Track, time, out onLast);
                return;
            }

            if (_quaternionPoints != null)
            {
                SetPropertyValue(
                    _quaternionPoints,
                    _quaternionProperty!,
                    _info.Track,
                    time,
                    out onLast
                );
                return;
            }

            throw new InvalidOperationException();
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
