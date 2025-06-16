using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.Patches;
using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.Managers;
using Heck;
using Heck.Animation;
using Heck.Deserialize;
using Heck.Event;
using UnityEngine;
using Vivify;
using Zenject;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    [CustomEvent(SET_GLOBAL_PROPERTY)]
    internal class EditorSetGlobalProperty : ICustomEvent, IInitializable, IDisposable
    {
        private readonly EditorAssetBundleManager _assetBundleManager;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly IBpmController _bpmController;
        private readonly CoroutineDummy _coroutineDummy;
        private readonly EditorDeserializedData _deserializedData;
        private readonly AudioDataModel _audioDataModel;

        private readonly List<ResettableProperty> _resettableProperties = [];

        private EditorSetGlobalProperty(
            EditorAssetBundleManager assetBundleManager,
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            IAudioTimeSource audioTimeSource,
            IBpmController bpmController,
            CoroutineDummy coroutineDummy,
            PopulateBeatmap populateBeatmap
        )
        {
            _assetBundleManager = assetBundleManager;
            _deserializedData = deserializedData;
            _audioTimeSource = audioTimeSource;
            _bpmController = bpmController;
            _coroutineDummy = coroutineDummy;
            _audioDataModel = populateBeatmap._audioDataModel;
        }

        public void Initialize()
        {
            foreach (
                ICustomEventCustomData customEventCustomData in _deserializedData
                    .CustomEventCustomDatas
                    .Values
            )
            {
                if (customEventCustomData is not SetGlobalPropertyData setGlobalPropertyData)
                {
                    continue;
                }

                foreach (MaterialProperty materialProperty in setGlobalPropertyData.Properties)
                {
                    object original = materialProperty.Id switch
                    {
                        int propertyId => materialProperty.Type switch
                        {
                            global::Vivify.MaterialPropertyType.Texture => Shader.GetGlobalTexture(
                                propertyId
                            ),
                            global::Vivify.MaterialPropertyType.Color => Shader.GetGlobalColor(
                                propertyId
                            ),
                            global::Vivify.MaterialPropertyType.Float => Shader.GetGlobalFloat(
                                propertyId
                            ),
                            global::Vivify.MaterialPropertyType.Vector => Shader.GetGlobalVector(
                                propertyId
                            ),
                            _ => new ArgumentOutOfRangeException(),
                        },
                        string name => materialProperty.Type switch
                        {
                            global::Vivify.MaterialPropertyType.Keyword => Shader.IsKeywordEnabled(
                                name
                            ),
                            _ => new ArgumentOutOfRangeException(),
                        },
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    _resettableProperties.Add(
                        new ResettableProperty(materialProperty.Id, materialProperty.Type, original)
                    );
                }
            }
        }

        public void Dispose()
        {
            foreach (ResettableProperty resettableProperty in _resettableProperties)
            {
                object original = resettableProperty.Value;
                switch (resettableProperty.Id)
                {
                    case int propertyId:
                        switch (resettableProperty.Type)
                        {
                            case global::Vivify.MaterialPropertyType.Texture:
                                Shader.SetGlobalTexture(propertyId, (Texture)original);
                                break;

                            case global::Vivify.MaterialPropertyType.Color:
                                Shader.SetGlobalColor(propertyId, (Color)original);
                                break;

                            case global::Vivify.MaterialPropertyType.Float:
                                Shader.SetGlobalFloat(propertyId, (float)original);
                                break;

                            case global::Vivify.MaterialPropertyType.Vector:
                                Shader.SetGlobalVector(propertyId, (Vector4)original);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;

                    case string name:
                        switch (resettableProperty.Type)
                        {
                            case global::Vivify.MaterialPropertyType.Keyword:
                                SetGlobalKeyword(name, (bool)original);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Callback(CustomEventData customEventData)
        {
            if (
                !_deserializedData.Resolve(
                    CustomDataRepository.GetCustomEventConversion(customEventData),
                    out SetGlobalPropertyData? data
                )
            )
            {
                return;
            }

            float duration = data.Duration;
            duration = (60f * duration) / _bpmController.currentBpm; // Convert to real time;

            List<MaterialProperty> properties = data.Properties;
            Functions easing = data.Easing;
            float startTime = _audioDataModel.bpmData.BeatToSeconds(customEventData.time);

            foreach (MaterialProperty property in properties)
            {
                var type = property.Type;
                object value = property.Value;
                bool noDuration = duration == 0 || startTime + duration < _audioTimeSource.songTime;
                switch (property.Id)
                {
                    case int propertyId:
                        switch (type)
                        {
                            case global::Vivify.MaterialPropertyType.Texture:
                                string texValue = Convert.ToString(value);
                                if (_assetBundleManager.TryGetAsset(texValue, out Texture? texture))
                                {
                                    Shader.SetGlobalTexture(propertyId, texture);
                                }

                                continue;

                            case global::Vivify.MaterialPropertyType.Color:
                                if (property is AnimatedMaterialProperty<Vector4> colorAnimated)
                                {
                                    if (noDuration)
                                    {
                                        Shader.SetGlobalColor(
                                            propertyId,
                                            colorAnimated.PointDefinition.Interpolate(1)
                                        );
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            colorAnimated.PointDefinition,
                                            propertyId,
                                            global::Vivify.MaterialPropertyType.Color,
                                            duration,
                                            startTime,
                                            easing
                                        );
                                    }
                                }
                                else
                                {
                                    List<float> color = ((List<object>)value)
                                        .Select(Convert.ToSingle)
                                        .ToList();
                                    Shader.SetGlobalColor(
                                        propertyId,
                                        new Color(
                                            color[0],
                                            color[1],
                                            color[2],
                                            color.Count > 3 ? color[3] : 1
                                        )
                                    );
                                }

                                continue;

                            case global::Vivify.MaterialPropertyType.Float:
                                if (property is AnimatedMaterialProperty<float> floatAnimated)
                                {
                                    if (noDuration)
                                    {
                                        Shader.SetGlobalFloat(
                                            propertyId,
                                            floatAnimated.PointDefinition.Interpolate(1)
                                        );
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            floatAnimated.PointDefinition,
                                            propertyId,
                                            global::Vivify.MaterialPropertyType.Float,
                                            duration,
                                            startTime,
                                            easing
                                        );
                                    }
                                }
                                else
                                {
                                    Shader.SetGlobalFloat(propertyId, Convert.ToSingle(value));
                                }

                                continue;

                            case global::Vivify.MaterialPropertyType.Vector:
                                if (property is AnimatedMaterialProperty<Vector4> vectorAnimated)
                                {
                                    if (noDuration)
                                    {
                                        Shader.SetGlobalVector(
                                            propertyId,
                                            vectorAnimated.PointDefinition.Interpolate(1)
                                        );
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            vectorAnimated.PointDefinition,
                                            propertyId,
                                            global::Vivify.MaterialPropertyType.Vector,
                                            duration,
                                            startTime,
                                            easing
                                        );
                                    }
                                }
                                else
                                {
                                    List<float> vector = ((List<object>)value)
                                        .Select(Convert.ToSingle)
                                        .ToList();
                                    Shader.SetGlobalVector(
                                        propertyId,
                                        new Vector4(vector[0], vector[1], vector[2], vector[3])
                                    );
                                }

                                continue;
                        }

                        break;

                    case string name:
                        switch (type)
                        {
                            case global::Vivify.MaterialPropertyType.Keyword:
                                if (property is AnimatedMaterialProperty<float> keywordAnimated)
                                {
                                    if (noDuration)
                                    {
                                        SetGlobalKeyword(
                                            name,
                                            keywordAnimated.PointDefinition.Interpolate(1) >= 1
                                        );
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            keywordAnimated.PointDefinition,
                                            name,
                                            global::Vivify.MaterialPropertyType.Float,
                                            duration,
                                            startTime,
                                            easing
                                        );
                                    }
                                }
                                else
                                {
                                    SetGlobalKeyword(name, (bool)value);
                                }

                                continue;
                        }

                        break;
                }

                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Type not currently supported."
                );
            }
        }

        private static void SetGlobalKeyword(string keyword, bool value)
        {
            if (value)
            {
                Shader.EnableKeyword(keyword);
            }
            else
            {
                Shader.DisableKeyword(keyword);
            }
        }

        private IEnumerator AnimateGlobalPropertyCoroutine<T>(
            PointDefinition<T> points,
            object id,
            global::Vivify.MaterialPropertyType type,
            float duration,
            float startTime,
            Functions easing
        )
            where T : struct
        {
            while (true)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;

                if (elapsedTime < duration)
                {
                    float time = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);
                    switch (id)
                    {
                        case int propertyId:
                            switch (type)
                            {
                                case global::Vivify.MaterialPropertyType.Color:
                                    Shader.SetGlobalColor(
                                        propertyId,
                                        (points as PointDefinition<Vector4>)!.Interpolate(time)
                                    );
                                    break;

                                case global::Vivify.MaterialPropertyType.Float:
                                    Shader.SetGlobalFloat(
                                        propertyId,
                                        (points as PointDefinition<float>)!.Interpolate(time)
                                    );
                                    break;

                                case global::Vivify.MaterialPropertyType.Vector:
                                    Shader.SetGlobalVector(
                                        propertyId,
                                        (points as PointDefinition<Vector4>)!.Interpolate(time)
                                    );
                                    break;
                            }

                            break;

                        case string name:
                            switch (type)
                            {
                                case global::Vivify.MaterialPropertyType.Keyword:
                                    SetGlobalKeyword(
                                        name,
                                        (points as PointDefinition<float>)!.Interpolate(time) >= 1
                                    );
                                    break;
                            }

                            break;
                    }

                    yield return null;
                }
                else
                {
                    break;
                }
            }
        }

        private void StartCoroutine<T>(
            PointDefinition<T> points,
            object id,
            global::Vivify.MaterialPropertyType type,
            float duration,
            float startTime,
            Functions easing
        )
            where T : struct
        {
            _coroutineDummy.StartCoroutine(
                AnimateGlobalPropertyCoroutine(points, id, type, duration, startTime, easing)
            );
        }

        private readonly struct ResettableProperty
        {
            internal ResettableProperty(
                object id,
                global::Vivify.MaterialPropertyType type,
                object value
            )
            {
                Id = id;
                Type = type;
                Value = value;
            }

            internal object Id { get; }

            internal global::Vivify.MaterialPropertyType Type { get; }

            internal object Value { get; }
        }
    }
}
