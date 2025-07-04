﻿using System.Collections.Generic;
using Chroma;
using EditorEX.Chroma.Colorizer;
using Heck.Animation;
using IPA.Utilities;
using UnityEngine;
using Zenject;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Chroma.Lighting
{
    internal class EditorChromaGradientController : ITickable
    {
        private readonly IAudioTimeSource _timeSource;
        private readonly EditorLightColorizerManager _manager;
        private readonly IBpmController _bpmController;

        private EditorChromaGradientController(
            IAudioTimeSource timeSource,
            EditorLightColorizerManager manager,
            IBpmController bpmController
        )
        {
            _timeSource = timeSource;
            _manager = manager;
            _bpmController = bpmController;
        }

        private Dictionary<BasicBeatmapEventType, ChromaGradientEvent> Gradients { get; } = new();

        public void Tick()
        {
            foreach (
                (BasicBeatmapEventType eventType, ChromaGradientEvent value) in new Dictionary<
                    BasicBeatmapEventType,
                    ChromaGradientEvent
                >(Gradients)
            )
            {
                Color color = value.Interpolate();
                _manager.Colorize(eventType, true, color, color, color, color);
            }
        }

        internal bool IsGradientActive(BasicBeatmapEventType eventType)
        {
            return Gradients.ContainsKey(eventType);
        }

        internal void CancelGradient(BasicBeatmapEventType eventType)
        {
            Gradients.Remove(eventType);
        }

        internal Color AddGradient(
            ChromaEventData.GradientObjectData gradientObject,
            BasicBeatmapEventType id,
            float time
        )
        {
            CancelGradient(id);

            float duration = gradientObject.Duration;
            Color initcolor = gradientObject.StartColor;
            Color endcolor = gradientObject.EndColor;
            Functions easing = gradientObject.Easing;

            ChromaGradientEvent gradientEvent = new(
                _timeSource,
                this,
                initcolor,
                endcolor,
                time,
                60 * duration / _bpmController.currentBpm,
                id,
                easing
            );
            Gradients[id] = gradientEvent;
            return gradientEvent.Interpolate();
        }

        internal class ChromaGradientEvent
        {
            private readonly IAudioTimeSource _timeSource;
            private readonly EditorChromaGradientController _gradientController;
            private readonly Color _initcolor;
            private readonly Color _endcolor;
            private readonly float _start;
            private readonly float _duration;
            private readonly BasicBeatmapEventType _event;
            private readonly Functions _easing;

            internal ChromaGradientEvent(
                IAudioTimeSource timeSource,
                EditorChromaGradientController gradientController,
                Color initcolor,
                Color endcolor,
                float start,
                float duration,
                BasicBeatmapEventType eventType,
                Functions easing = Functions.easeLinear
            )
            {
                _timeSource = timeSource;
                _gradientController = gradientController;
                _initcolor = initcolor;
                _endcolor = endcolor;
                _start = start;
                _duration = duration;
                _event = eventType;
                _easing = easing;
            }

            internal Color Interpolate()
            {
                float normalTime = _timeSource.songTime - _start;
                if (normalTime < 0)
                {
                    return _initcolor;
                }

                if (normalTime <= _duration)
                {
                    return Color.LerpUnclamped(
                        _initcolor,
                        _endcolor,
                        Easings.Interpolate(normalTime / _duration, _easing)
                    );
                }

                _gradientController.Gradients.Remove(_event);
                return _endcolor;
            }
        }
    }
}
