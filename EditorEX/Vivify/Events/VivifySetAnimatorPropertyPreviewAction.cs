using System;
using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Events;
using Heck.Animation;
using UnityEngine;
using Vivify;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifySetAnimatorPropertyPreviewAction : IPreviewStateAction
    {
        private readonly string _prefabId;
        private readonly List<AnimatorProperty> _properties;
        private readonly EditorSetAnimatorProperty _setAnimatorProperty;
        private readonly float _fromBeat;
        private readonly float _durationBeats;
        private readonly Functions _easing;
        private readonly List<Action> _restore = [];
        private Animator[]? _captured;
        private bool _active;
        private bool _written;

        public VivifySetAnimatorPropertyPreviewAction(
            string prefabId,
            List<AnimatorProperty> properties,
            EditorSetAnimatorProperty setAnimatorProperty,
            float fromBeat,
            float durationBeats,
            Functions easing
        )
        {
            _prefabId = prefabId;
            _properties = properties;
            _setAnimatorProperty = setAnimatorProperty;
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

            for (int i = 0; i < _restore.Count; i++)
            {
                _restore[i]();
            }

            _restore.Clear();
            _captured = null;
            _written = false;
            _active = false;
        }

        public void Tick(float beat)
        {
            if (
                !_active
                || !_setAnimatorProperty.TryGetAnimators(_prefabId, out Animator[] animators)
            )
            {
                return;
            }

            if (NeedsRecapture(animators))
            {
                _restore.Clear();
                CaptureOriginals(animators);
                _captured = animators;
            }

            if (!VivifyPreviewOwnership.NeedsMaterialWrite(_durationBeats, _written))
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
            _setAnimatorProperty.ApplyAtProgress(animators, _properties, progress);
            _written = true;
        }

        private bool NeedsRecapture(Animator[] animators)
        {
            if (_restore.Count == 0 || _captured == null || _captured.Length != animators.Length)
            {
                return true;
            }

            for (int i = 0; i < animators.Length; i++)
            {
                if (
                    !VivifyPreviewOwnership.CanWriteAnimator(_captured[i])
                    || _captured[i] != animators[i]
                )
                {
                    return true;
                }
            }

            return false;
        }

        private void CaptureOriginals(Animator[] animators)
        {
            foreach (AnimatorProperty property in _properties)
            {
                string name = property.Name;
                AnimatorPropertyType type = property.Type;
                foreach (Animator animator in animators)
                {
                    if (!VivifyPreviewOwnership.CanWriteAnimator(animator))
                    {
                        continue;
                    }

                    Animator captured = animator;
                    switch (type)
                    {
                        case AnimatorPropertyType.Bool:
                            bool boolValue = captured.GetBool(name);
                            Remember(captured, () => captured.SetBool(name, boolValue));
                            break;
                        case AnimatorPropertyType.Float:
                            float floatValue = captured.GetFloat(name);
                            Remember(captured, () => captured.SetFloat(name, floatValue));
                            break;
                        case AnimatorPropertyType.Integer:
                            int intValue = captured.GetInteger(name);
                            Remember(captured, () => captured.SetInteger(name, intValue));
                            break;
                        case AnimatorPropertyType.Trigger:
                            bool trigger = (bool)property.Value;
                            Remember(
                                captured,
                                () =>
                                {
                                    if (trigger)
                                    {
                                        captured.ResetTrigger(name);
                                    }
                                }
                            );
                            break;
                    }
                }
            }
        }

        private void Remember(Animator captured, Action restore)
        {
            _restore.Add(() =>
            {
                if (VivifyPreviewOwnership.CanWriteAnimator(captured))
                {
                    restore();
                }
            });
        }
    }
}
