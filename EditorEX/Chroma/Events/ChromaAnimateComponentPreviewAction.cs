using System;
using System.Collections.Generic;
using Chroma.EnvironmentEnhancement.Component;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Events;
using Heck.Animation;
using UnityEngine;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace EditorEX.Chroma.Events
{
    internal sealed class ChromaAnimateComponentPreviewAction : IPreviewStateAction
    {
        private readonly Track _track;
        private readonly string _componentName;
        private readonly string _property;
        private readonly PointDefinition<float> _points;
        private readonly float _fromBeat;
        private readonly float _durationBeats;
        private readonly Functions _easing;
        private readonly Dictionary<object, float> _originals = new();
        private bool _active;

        public ChromaAnimateComponentPreviewAction(
            Track track,
            string componentName,
            string property,
            PointDefinition<float> points,
            float fromBeat,
            float durationBeats,
            Functions easing
        )
        {
            _track = track;
            _componentName = componentName;
            _property = property;
            _points = points;
            _fromBeat = fromBeat;
            _durationBeats = durationBeats;
            _easing = easing;
        }

        public void Execute()
        {
            if (_active)
            {
                return;
            }

            _active = true;
        }

        public void Reverse()
        {
            if (!_active)
            {
                return;
            }

            foreach (KeyValuePair<object, float> pair in _originals)
            {
                if (pair.Key == null)
                {
                    continue;
                }

                Write(pair.Key, pair.Value);
            }

            _active = false;
        }

        public void Tick(float beat)
        {
            if (!_active)
            {
                return;
            }

            object[] components = GetComponents();
            if (components.Length == 0)
            {
                return;
            }

            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat,
                _fromBeat,
                _durationBeats,
                repeat: 0,
                _easing,
                out _
            );
            float value = _points.Interpolate(progress, out _);

            foreach (object component in components)
            {
                if (component == null)
                {
                    continue;
                }

                if (!_originals.ContainsKey(component))
                {
                    _originals[component] = Read(component);
                }

                Write(component, value);
            }
        }

        private object[] GetComponents()
        {
            return _componentName switch
            {
                BLOOM_FOG_ENVIRONMENT => BloomFogCustomizer.GetComponents(_track),
                TUBE_BLOOM_PRE_PASS_LIGHT => TubeBloomLightCustomizer.GetComponents(_track),
                _ => Array.Empty<object>(),
            };
        }

        private float Read(object component)
        {
            switch (component)
            {
                case BloomFogEnvironmentParams fog:
                    return _property switch
                    {
                        ATTENUATION => fog.attenuation,
                        OFFSET => fog.offset,
                        HEIGHT_FOG_HEIGHT => fog.heightFogHeight,
                        HEIGHT_FOG_STARTY => fog.heightFogStartY,
                        _ => 0f,
                    };
                case TubeBloomPrePassLight tube:
                    return _property switch
                    {
                        COLOR_ALPHA_MULTIPLIER => tube._colorAlphaMultiplier,
                        BLOOM_FOG_INTENSITY_MULTIPLIER => tube.bloomFogIntensityMultiplier,
                        _ => 0f,
                    };
                default:
                    return 0f;
            }
        }

        private void Write(object component, float value)
        {
            switch (component)
            {
                case BloomFogEnvironmentParams fog:
                    switch (_property)
                    {
                        case ATTENUATION:
                            fog.attenuation = value;
                            break;
                        case OFFSET:
                            fog.offset = value;
                            break;
                        case HEIGHT_FOG_HEIGHT:
                            fog.heightFogHeight = value;
                            break;
                        case HEIGHT_FOG_STARTY:
                            fog.heightFogStartY = value;
                            break;
                    }

                    break;
                case TubeBloomPrePassLight tube:
                    switch (_property)
                    {
                        case COLOR_ALPHA_MULTIPLIER:
                            TubeBloomLightCustomizer.SetColorAlphaMultiplier(tube, value);
                            break;
                        case BLOOM_FOG_INTENSITY_MULTIPLIER:
                            tube.bloomFogIntensityMultiplier = value;
                            break;
                    }

                    break;
            }
        }
    }
}
