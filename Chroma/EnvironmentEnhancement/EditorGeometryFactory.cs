﻿using System;
using System.Linq;
using Chroma.EnvironmentEnhancement;
using Chroma.Extras;
using Chroma.HarmonyPatches.EnvironmentComponent;
using CustomJSONData.CustomBeatmap;
using EditorEX.Chroma.HarmonyPatches.Colorizer.Initialize;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using static Chroma.ChromaController;
using Object = UnityEngine.Object;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Chroma.EnvironmentEnhancement
{
    internal class EditorGeometryFactory
    {
        private readonly IInstantiator _instantiator;
        private readonly EditorMaterialsManager _materialsManager;
        private readonly EditorLightWithIdRegisterer _lightWithIdRegisterer;
        private readonly ParametricBoxControllerTransformOverride _parametricBoxControllerTransformOverride;
        private TubeBloomPrePassLight? _originalTubeBloomPrePassLight = Resources
            .FindObjectsOfTypeAll<TubeBloomPrePassLight>()
            .FirstOrDefault();

        private EditorGeometryFactory(
            IInstantiator instantiator,
            EditorMaterialsManager materialsManager,
            EditorLightWithIdRegisterer lightWithIdRegisterer,
            ParametricBoxControllerTransformOverride parametricBoxControllerTransformOverride
        )
        {
            _instantiator = instantiator;
            _materialsManager = materialsManager;
            _lightWithIdRegisterer = lightWithIdRegisterer;
            _parametricBoxControllerTransformOverride = parametricBoxControllerTransformOverride;
        }

        internal static bool IsLightType(ShaderType shaderType)
        {
            return shaderType
                is ShaderType.OpaqueLight
                    or ShaderType.TransparentLight
                    or ShaderType.BillieWater;
        }

        internal GameObject Create(CustomData customData, bool v2)
        {
            GeometryType geometryType = customData.GetStringToEnumRequired<GeometryType>(
                v2 ? V2_GEOMETRY_TYPE : GEOMETRY_TYPE
            );
            bool collision = customData.Get<bool?>(v2 ? V2_COLLISION : COLLISION) ?? false;

            object materialData = customData.GetRequired<object>(v2 ? V2_MATERIAL : MATERIAL);
            MaterialInfo materialInfo = materialData switch
            {
                string name => _materialsManager.GetMaterialInfo(name),
                CustomData data => _materialsManager.CreateMaterialInfo(data),
                _ => throw new InvalidOperationException($"Could not read [{MATERIAL}]."),
            };
            ShaderType shaderType = materialInfo.ShaderType;

            PrimitiveType primitiveType = geometryType switch
            {
                GeometryType.Sphere => PrimitiveType.Sphere,
                GeometryType.Capsule => PrimitiveType.Capsule,
                GeometryType.Cylinder => PrimitiveType.Cylinder,
                GeometryType.Cube => PrimitiveType.Cube,
                GeometryType.Plane => PrimitiveType.Plane,
                GeometryType.Quad => PrimitiveType.Quad,
                GeometryType.Triangle => PrimitiveType.Quad,
                _ => throw new ArgumentOutOfRangeException(
                    $"Geometry type {geometryType} does not match a primitive!",
                    nameof(geometryType)
                ),
            };

            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = $"{geometryType}{shaderType}";
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            gameObject.layer = 14;

            // Disable expensive shadows
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            // Shared material is usually better performance as far as I know
            meshRenderer.sharedMaterial = materialInfo.Material;

            if (geometryType == GeometryType.Triangle)
            {
                Mesh mesh = ChromaUtils.CreateTriangleMesh();
                gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                if (collision)
                {
                    MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        meshCollider.sharedMesh = mesh;
                    }
                }
            }

            if (!collision)
            {
                // destroy colliders
                Object.Destroy(gameObject.GetComponent<Collider>());
            }

            // Handle light preset
            if (!IsLightType(shaderType))
            {
                return gameObject;
            }

            // Stop TubeBloomPrePassLight from running OnEnable before I can set the fields
            gameObject.SetActive(false);

            TubeBloomPrePassLight tubeBloomPrePassLight =
                gameObject.AddComponent<TubeBloomPrePassLight>();
            ParametricBoxController parametricBoxController =
                gameObject.AddComponent<ParametricBoxController>();
            _parametricBoxControllerTransformOverride.UpdatePosition(parametricBoxController);
            _parametricBoxControllerTransformOverride.UpdateScale(parametricBoxController);
            parametricBoxController._meshRenderer = meshRenderer;

            if (_originalTubeBloomPrePassLight != null)
            {
                tubeBloomPrePassLight._mainEffectPostProcessEnabled =
                    _originalTubeBloomPrePassLight._mainEffectPostProcessEnabled;

                tubeBloomPrePassLight._lightType = _originalTubeBloomPrePassLight._lightType;
                tubeBloomPrePassLight._registeredWithLightType =
                    _originalTubeBloomPrePassLight._registeredWithLightType;
            }
            else
            {
                throw new InvalidOperationException(
                    $"[{nameof(_originalTubeBloomPrePassLight)}] was null."
                );
            }

            tubeBloomPrePassLight._parametricBoxController = parametricBoxController;
            TubeBloomPrePassLightWithId lightWithId =
                _instantiator.InstantiateComponent<TubeBloomPrePassLightWithId>(gameObject);
            lightWithId._tubeBloomPrePassLight = tubeBloomPrePassLight;
            _lightWithIdRegisterer.MarkForTableRegister(lightWithId);

            gameObject.SetActive(true);

            return gameObject;
        }
    }
}
