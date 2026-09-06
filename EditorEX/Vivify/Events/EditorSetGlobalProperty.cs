using System;
using System.Collections.Generic;
using System.Linq;
using EditorEX.Vivify.Managers;
using UnityEngine;
using Vivify;

namespace EditorEX.Vivify.Events
{
    internal class EditorSetGlobalProperty
    {
        private readonly EditorAssetBundleManager _assetBundleManager;

        private EditorSetGlobalProperty(EditorAssetBundleManager assetBundleManager)
        {
            _assetBundleManager = assetBundleManager;
        }

        internal void ApplyAtProgress(List<MaterialProperty> properties, float progress)
        {
            foreach (MaterialProperty property in properties)
            {
                global::Vivify.MaterialPropertyType type = property.Type;
                object value = property.Value;
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
                                    Shader.SetGlobalColor(
                                        propertyId,
                                        colorAnimated.PointDefinition.Interpolate(progress)
                                    );
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
                                    Shader.SetGlobalFloat(
                                        propertyId,
                                        floatAnimated.PointDefinition.Interpolate(progress)
                                    );
                                }
                                else
                                {
                                    Shader.SetGlobalFloat(propertyId, Convert.ToSingle(value));
                                }

                                continue;

                            case global::Vivify.MaterialPropertyType.Vector:
                                if (property is AnimatedMaterialProperty<Vector4> vectorAnimated)
                                {
                                    Shader.SetGlobalVector(
                                        propertyId,
                                        vectorAnimated.PointDefinition.Interpolate(progress)
                                    );
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
                                    SetGlobalKeyword(
                                        name,
                                        keywordAnimated.PointDefinition.Interpolate(progress) >= 1
                                    );
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

        internal static void SetGlobalKeyword(string keyword, bool value)
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
    }
}
