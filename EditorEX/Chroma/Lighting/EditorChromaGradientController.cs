using System.Collections.Generic;
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

        // Reused across frames so an idle/active Tick allocates nothing. Interpolate() can remove
        // from Gradients mid-iteration, so we snapshot into this buffer first instead of iterating
        // (or copying) the live dictionary every frame.
        private readonly List<
            KeyValuePair<BasicBeatmapEventType, ChromaGradientEvent>
        > _tickBuffer = new();

        public void Tick()
        {
            if (Gradients.Count == 0)
            {
                return;
            }

            _tickBuffer.Clear();
            foreach (KeyValuePair<BasicBeatmapEventType, ChromaGradientEvent> pair in Gradients)
            {
                _tickBuffer.Add(pair);
            }

            for (int i = 0; i < _tickBuffer.Count; i++)
            {
                Color color = _tickBuffer[i].Value.Interpolate();
                _manager.Colorize(_tickBuffer[i].Key, true, color, color, color, color);
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
