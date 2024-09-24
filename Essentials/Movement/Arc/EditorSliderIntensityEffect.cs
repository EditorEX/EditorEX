using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetterEditor.Essentials.Movement.Arc
{
    public class EditorSliderIntensityEffect : MonoBehaviour
    {
        private float _longSliderHeadIntensity = 0.8f;
        private float _shortSliderHeadIntensity = 1.2f;
        private float _tailIntensity = 1.5f;
        private float _fadeOutDuration = 0.1f;
        private float _stayOffDuration = 1f;
        private float _flashBoost = 2f;
        private float _flashInDuration = 0.3f;
        private float _flashOutDuration = 0.7f;
        private readonly IAudioTimeSource _audioTimeSyncController;
        private float _coreIntensity;
        private float _effectIntensity;
        private float _halfJumpDuration;
        private float _sliderDuration;
        private float headIntensity;

        private SliderIntensityEffect.FadeElement[] _dipEffectFadeElements;
        private SliderIntensityEffect.FadeElement[] _flashEffectFadeElements;
        private SliderIntensityEffect.FadeElement[] _fadeInEffectFadeElements;

        public event Action fadeInDidStartEvent;

        public float colorIntensity
        {
            get
            {
                return _coreIntensity * _effectIntensity;
            }
        }
        protected void Awake()
        {
            _dipEffectFadeElements = new SliderIntensityEffect.FadeElement[]
            {
                new SliderIntensityEffect.FadeElement(EaseType.OutQuad, 1f, 0f, null),
                new SliderIntensityEffect.FadeElement(EaseType.Linear, 0f, 0f, null),
                new SliderIntensityEffect.FadeElement(EaseType.Linear, 0f, 1f, delegate
                {
                    Action action = this.fadeInDidStartEvent;
                    if (action == null)
                    {
                        return;
                    }
                    action();
                })
            };

            _flashEffectFadeElements = new SliderIntensityEffect.FadeElement[]
            {
                new SliderIntensityEffect.FadeElement(EaseType.OutQuad, 1f, _flashBoost, null),
                new SliderIntensityEffect.FadeElement(EaseType.OutQuart, _flashBoost, 1f, null)
            };

            _fadeInEffectFadeElements = new SliderIntensityEffect.FadeElement[]
            {
                new SliderIntensityEffect.FadeElement(EaseType.Linear, 0f, 1f, delegate
                {
                    Action action2 = this.fadeInDidStartEvent;
                    if (action2 == null)
                    {
                        return;
                    }
                    action2();
                })
            };
        }

        public void Init(float sliderDuration, float halfJumpDuration, bool startVisible)
        {
            _halfJumpDuration = halfJumpDuration;
            _sliderDuration = sliderDuration;
            headIntensity = ((sliderDuration < halfJumpDuration) ? _shortSliderHeadIntensity : _longSliderHeadIntensity);
            _coreIntensity = headIntensity;
            _effectIntensity = (startVisible ? 1f : 0f);
            float num = Mathf.Max(sliderDuration - _fadeOutDuration - _stayOffDuration, 0.1f);
            _dipEffectFadeElements[0].duration = _fadeOutDuration;
            _dipEffectFadeElements[1].duration = _stayOffDuration;
            _dipEffectFadeElements[2].duration = num;
            _flashEffectFadeElements[0].duration = _flashInDuration;
            _flashEffectFadeElements[1].duration = _flashOutDuration;
            _fadeInEffectFadeElements[0].duration = num;
        }

        public void ManualUpdate(float timeSinceHeadNoteJump)
        {
            _coreIntensity = Mathf.Lerp(headIntensity, _tailIntensity, (timeSinceHeadNoteJump - _halfJumpDuration) / _sliderDuration);
        }

        private IEnumerator ProcessEffectCoroutine(IEnumerable<SliderIntensityEffect.FadeElement> fadeElements)
        {
            foreach (SliderIntensityEffect.FadeElement fadeElement in fadeElements)
            {
                Action startCallback = fadeElement.startCallback;
                if (startCallback != null)
                {
                    startCallback();
                }
                float startTime = _audioTimeSyncController.songTime;
                float num;
                while ((num = _audioTimeSyncController.songTime - startTime) < fadeElement.duration)
                {
                    float num2 = Interpolation.Interpolate(num / fadeElement.duration, fadeElement.easeType);
                    _effectIntensity = Mathf.LerpUnclamped(fadeElement.startIntensity, fadeElement.endIntensity, num2);
                    yield return null;
                }
            }
        }

        public void StartIntensityDipEffect()
        {
            StopAllCoroutines();
            StartCoroutine(ProcessEffectCoroutine(_dipEffectFadeElements));
        }

        public void StartIntensityFadeInEffect()
        {
            StopAllCoroutines();
            StartCoroutine(ProcessEffectCoroutine(_fadeInEffectFadeElements));
        }

        public void StartFlashEffect()
        {
            StopAllCoroutines();
            StartCoroutine(ProcessEffectCoroutine(_flashEffectFadeElements));
        }
    }
}
