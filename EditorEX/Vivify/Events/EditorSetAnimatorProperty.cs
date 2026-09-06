using System;
using System.Collections.Generic;
using UnityEngine;
using Vivify;
using Vivify.Managers;

namespace EditorEX.Vivify.Events
{
    internal class EditorSetAnimatorProperty
    {
        private readonly PrefabManager _prefabManager;

        private EditorSetAnimatorProperty(PrefabManager prefabManager)
        {
            _prefabManager = prefabManager;
        }

        internal bool TryGetAnimators(string id, out Animator[] animators)
        {
            if (
                _prefabManager.TryGetPrefab(id, out InstantiatedPrefab? instantiatedPrefab)
                && instantiatedPrefab != null
            )
            {
                animators = instantiatedPrefab.Animators;
                return animators.Length > 0;
            }

            animators = Array.Empty<Animator>();
            return false;
        }

        internal void ApplyAtProgress(
            Animator[] animators,
            List<AnimatorProperty> properties,
            float progress
        )
        {
            foreach (AnimatorProperty property in properties)
            {
                string name = property.Name;
                AnimatorPropertyType type = property.Type;
                object value = property.Value;
                AnimatedAnimatorProperty? animated = property as AnimatedAnimatorProperty;
                switch (type)
                {
                    case AnimatorPropertyType.Bool:
                        bool boolValue =
                            animated != null
                                ? animated.PointDefinition.Interpolate(progress) >= 1
                                : (bool)value;
                        foreach (Animator animator in animators)
                        {
                            animator.SetBool(name, boolValue);
                        }

                        break;

                    case AnimatorPropertyType.Float:
                        float floatValue =
                            animated != null
                                ? animated.PointDefinition.Interpolate(progress)
                                : Convert.ToSingle(value);
                        foreach (Animator animator in animators)
                        {
                            animator.SetFloat(name, floatValue);
                        }

                        break;

                    case AnimatorPropertyType.Integer:
                        int intValue =
                            animated != null
                                ? (int)animated.PointDefinition.Interpolate(progress)
                                : Convert.ToInt32(value);
                        foreach (Animator animator in animators)
                        {
                            animator.SetInteger(name, intValue);
                        }

                        break;

                    case AnimatorPropertyType.Trigger:
                        bool trigger = (bool)value;
                        foreach (Animator animator in animators)
                        {
                            if (trigger)
                            {
                                animator.SetTrigger(name);
                            }
                            else
                            {
                                animator.ResetTrigger(name);
                            }
                        }

                        break;
                }
            }
        }
    }
}
