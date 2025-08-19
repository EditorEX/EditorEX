﻿using System;
using System.Collections;
using System.Collections.Generic;
using BeatmapEditor3D;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.Patches;
using EditorEX.Heck.Deserialize;
using Heck;
using Heck.Animation;
using Heck.Event;
using SiraUtil.Logging;
using UnityEngine;
using Vivify;
using Vivify.Managers;
using Zenject;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    [CustomEvent(SET_ANIMATOR_PROPERTY)]
    internal class EditorSetAnimatorProperty : ICustomEvent
    {
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly IBpmController _bpmController;
        private readonly CoroutineDummy _coroutineDummy;
        private readonly EditorDeserializedData _deserializedData;
        private readonly SiraLog _log;
        private readonly PrefabManager _prefabManager;
        private readonly AudioDataModel _audioDataModel;

        private EditorSetAnimatorProperty(
            SiraLog log,
            PrefabManager prefabManager,
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            IAudioTimeSource audioTimeSource,
            IBpmController bpmController,
            CoroutineDummy coroutineDummy,
            PopulateBeatmap populateBeatmap
        )
        {
            _log = log;
            _prefabManager = prefabManager;
            _deserializedData = deserializedData;
            _audioTimeSource = audioTimeSource;
            _bpmController = bpmController;
            _coroutineDummy = coroutineDummy;
            _audioDataModel = populateBeatmap._audioDataModel;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (
                !_deserializedData.Resolve(
                    CustomDataRepository.GetCustomEventConversion(customEventData),
                    out SetAnimatorPropertyData? data
                )
            )
            {
                return;
            }

            float duration = data.Duration;
            duration = (60f * duration) / _bpmController.currentBpm; // Convert to real time;

            if (!_prefabManager.TryGetPrefab(data.Id, out InstantiatedPrefab? instantiatedPrefab))
            {
                return;
            }

            List<AnimatorProperty> properties = data.Properties;
            SetAnimatorProperties(
                instantiatedPrefab.Animators,
                properties,
                duration,
                data.Easing,
                _audioDataModel.bpmData.BeatToSeconds(customEventData.time)
            );
        }

        internal void SetAnimatorProperties(
            Animator[] animators,
            List<AnimatorProperty> properties,
            float duration,
            Functions easing,
            float startTime
        )
        {
            foreach (AnimatorProperty property in properties)
            {
                string name = property.Name;
                AnimatorPropertyType type = property.Type;
                object value = property.Value;
                bool noDuration = duration == 0 || startTime + duration < _audioTimeSource.songTime;
                AnimatedAnimatorProperty? animated = property as AnimatedAnimatorProperty;
                Debug.Log(
                    $"Setting animator property: {name} ({type}) with value: {value} (noDuration: {noDuration})"
                );
                switch (type)
                {
                    case AnimatorPropertyType.Bool:
                        if (animated != null)
                        {
                            if (noDuration)
                            {
                                foreach (Animator animator in animators)
                                {
                                    animator.SetBool(
                                        name,
                                        animated.PointDefinition.Interpolate(1) >= 1
                                    );
                                }
                            }
                            else
                            {
                                StartCoroutine(
                                    animated.PointDefinition,
                                    animators,
                                    name,
                                    AnimatorPropertyType.Bool,
                                    duration,
                                    startTime,
                                    easing
                                );
                            }
                        }
                        else
                        {
                            foreach (Animator animator in animators)
                            {
                                animator.SetBool(name, (bool)value);
                            }
                        }

                        break;

                    case AnimatorPropertyType.Float:
                        if (animated != null)
                        {
                            if (noDuration)
                            {
                                foreach (Animator animator in animators)
                                {
                                    animator.SetFloat(
                                        name,
                                        animated.PointDefinition.Interpolate(1)
                                    );
                                }
                            }
                            else
                            {
                                StartCoroutine(
                                    animated.PointDefinition,
                                    animators,
                                    name,
                                    AnimatorPropertyType.Float,
                                    duration,
                                    startTime,
                                    easing
                                );
                            }
                        }
                        else
                        {
                            foreach (Animator animator in animators)
                            {
                                animator.SetFloat(name, Convert.ToSingle(value));
                            }
                        }

                        break;

                    case AnimatorPropertyType.Integer:
                        if (animated != null)
                        {
                            if (noDuration)
                            {
                                foreach (Animator animator in animators)
                                {
                                    animator.SetFloat(
                                        name,
                                        animated.PointDefinition.Interpolate(1)
                                    );
                                }
                            }
                            else
                            {
                                StartCoroutine(
                                    animated.PointDefinition,
                                    animators,
                                    name,
                                    AnimatorPropertyType.Float,
                                    duration,
                                    startTime,
                                    easing
                                );
                            }
                        }
                        else
                        {
                            foreach (Animator animator in animators)
                            {
                                animator.SetFloat(name, Convert.ToSingle(value));
                            }
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

                    default:
                        _log.Error($"[{type}] invalid");
                        break;
                }
            }
        }

        private IEnumerator AnimatePropertyCoroutine(
            PointDefinition<float> points,
            Animator[] animators,
            string name,
            AnimatorPropertyType type,
            float duration,
            float startTime,
            Functions easing
        )
        {
            while (true)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;

                if (elapsedTime < duration)
                {
                    float time = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);
                    switch (type)
                    {
                        case AnimatorPropertyType.Bool:
                            {
                                bool value = points.Interpolate(time) >= 1;
                                foreach (Animator animator in animators)
                                {
                                    animator.SetBool(name, value);
                                }

                                break;
                            }

                        case AnimatorPropertyType.Float:
                            {
                                float value = points.Interpolate(time);
                                foreach (Animator animator in animators)
                                {
                                    animator.SetFloat(name, value);
                                }

                                break;
                            }

                        case AnimatorPropertyType.Integer:
                            {
                                float value = points.Interpolate(time);
                                foreach (Animator animator in animators)
                                {
                                    animator.SetInteger(name, (int)value);
                                }

                                break;
                            }

                        default:
                            yield break;
                    }

                    yield return null;
                }
                else
                {
                    break;
                }
            }
        }

        private void StartCoroutine(
            PointDefinition<float> points,
            Animator[] animators,
            string name,
            AnimatorPropertyType type,
            float duration,
            float startTime,
            Functions easing
        )
        {
            _coroutineDummy.StartCoroutine(
                AnimatePropertyCoroutine(points, animators, name, type, duration, startTime, easing)
            );
        }
    }
}
