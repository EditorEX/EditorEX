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
using Vivify.Managers;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    [CustomEvent(SET_MATERIAL_PROPERTY)]
    internal class EditorSetMaterialProperty : ICustomEvent
    {
        private readonly EditorAssetBundleManager _assetBundleManager;
        private readonly EditorDeserializedData _deserializedData;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly IBpmController _bpmController;
        private readonly CoroutineDummy _coroutineDummy;
        private readonly AudioDataModel _audioDataModel;

        private EditorSetMaterialProperty(
            EditorAssetBundleManager assetBundleManager,
            [Inject(Id = ID)] EditorDeserializedData deserializedData,
            IAudioTimeSource audioTimeSource,
            IBpmController bpmController,
            CoroutineDummy coroutineDummy,
            PopulateBeatmap populateBeatmap)
        {
            _assetBundleManager = assetBundleManager;
            _deserializedData = deserializedData;
            _audioTimeSource = audioTimeSource;
            _bpmController = bpmController;
            _coroutineDummy = coroutineDummy;
            _audioDataModel = populateBeatmap._audioDataModel;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out SetMaterialPropertyData? data))
            {
                return;
            }

            float duration = data.Duration;
            duration = (60f * duration) / _bpmController.currentBpm; // Convert to real time;

            if (!_assetBundleManager.TryGetAsset(data.Asset, out Material? material))
            {
                return;
            }

            List<MaterialProperty> properties = data.Properties;
            SetMaterialProperties(material, properties, duration, data.Easing, _audioDataModel.bpmData.BeatToSeconds(customEventData.time));
        }

        internal void SetMaterialProperties(
            Material material,
            List<MaterialProperty> properties,
            float duration,
            Functions easing,
            float startTime)
        {
            Debug.Log("static " + material.name);
            foreach (MaterialProperty property in properties)
            {
                global::Vivify.MaterialPropertyType type = property.Type;
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
                                    material.SetTexture(propertyId, texture);
                                }

                                continue;

                            case global::Vivify.MaterialPropertyType.Color:
                                if (property is AnimatedMaterialProperty<Vector4> colorAnimated)
                                {
                                    if (noDuration)
                                    {
                                        material.SetColor(propertyId, colorAnimated.PointDefinition.Interpolate(1));
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            colorAnimated.PointDefinition,
                                            material,
                                            propertyId,
                                            global::Vivify.MaterialPropertyType.Color,
                                            duration,
                                            startTime,
                                            easing);
                                    }
                                }
                                else
                                {
                                    List<float> color = ((List<object>)value).Select(Convert.ToSingle).ToList();
                                    material.SetColor(
                                        propertyId,
                                        new Color(color[0], color[1], color[2], color.Count > 3 ? color[3] : 1));
                                }

                                continue;

                            case global::Vivify.MaterialPropertyType.Float:
                                if (property is AnimatedMaterialProperty<float> floatAnimated)
                                {
                                    if (noDuration)
                                    {
                                        material.SetFloat(propertyId, floatAnimated.PointDefinition.Interpolate(1));
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            floatAnimated.PointDefinition,
                                            material,
                                            propertyId,
                                            global::Vivify.MaterialPropertyType.Float,
                                            duration,
                                            startTime,
                                            easing);
                                    }
                                }
                                else
                                {
                                    material.SetFloat(propertyId, Convert.ToSingle(value));
                                }

                                continue;

                            case global::Vivify.MaterialPropertyType.Vector:
                                if (property is AnimatedMaterialProperty<Vector4> vectorAnimated)
                                {
                                    if (noDuration)
                                    {
                                        material.SetVector(propertyId, vectorAnimated.PointDefinition.Interpolate(1));
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            vectorAnimated.PointDefinition,
                                            material,
                                            propertyId,
                                            global::Vivify.MaterialPropertyType.Vector,
                                            duration,
                                            startTime,
                                            easing);
                                    }
                                }
                                else
                                {
                                    List<float> vector = ((List<object>)value).Select(Convert.ToSingle).ToList();
                                    material.SetVector(propertyId,
                                        new Vector4(vector[0], vector[1], vector[2], vector[3]));
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
                                        SetKeyword(material, name, keywordAnimated.PointDefinition.Interpolate(1) >= 1);
                                    }
                                    else
                                    {
                                        StartCoroutine(
                                            keywordAnimated.PointDefinition,
                                            material,
                                            name,
                                            global::Vivify.MaterialPropertyType.Float,
                                            duration,
                                            startTime,
                                            easing);
                                    }
                                }
                                else
                                {
                                    SetKeyword(material, name, (bool)value);
                                }

                                continue;
                        }

                        break;
                }

                // im lazy, shoot me
                throw new ArgumentOutOfRangeException(nameof(type), type, "Type not currently supported.");
            }
        }

        private static void SetKeyword(Material material, string keyword, bool value)
        {
            if (value)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }

        private IEnumerator AnimatePropertyCoroutine<T>(
            PointDefinition<T> points,
            Material material,
            object id,
            global::Vivify.MaterialPropertyType type,
            float duration,
            float startTime,
            Functions easing)
            where T : struct
        {
            Debug.Log("animate " + material.name);
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
                                    material.SetColor(propertyId,
                                        (points as PointDefinition<Vector4>)!.Interpolate(time));
                                    break;

                                case global::Vivify.MaterialPropertyType.Float:
                                    material.SetFloat(propertyId,
                                        (points as PointDefinition<float>)!.Interpolate(time));
                                    break;

                                case global::Vivify.MaterialPropertyType.Vector:
                                    material.SetVector(propertyId,
                                        (points as PointDefinition<Vector4>)!.Interpolate(time));
                                    break;
                            }

                            break;

                        case string name:
                            switch (type)
                            {
                                case global::Vivify.MaterialPropertyType.Keyword:
                                    SetKeyword(material, name,
                                        (points as PointDefinition<float>)!.Interpolate(time) >= 1);
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
            Material material,
            object id,
            global::Vivify.MaterialPropertyType type,
            float duration,
            float startTime,
            Functions easing)
            where T : struct
        {
            _coroutineDummy.StartCoroutine(
                AnimatePropertyCoroutine(points, material, id, type, duration, startTime, easing));
        }
    }
}