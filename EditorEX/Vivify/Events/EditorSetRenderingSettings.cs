using System;
using System.Collections.Generic;
using EditorEX.Vivify.Managers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Vivify;
using Vivify.Events;
using Vivify.Extras;
using Vivify.Managers;
using Zenject;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    internal class EditorSetRenderingSettings : IInitializable, IDisposable
    {
        private readonly Dictionary<string, ISettingHandler> _settings = new()
        {
            {
                nameof(RenderSettings.ambientEquatorColor),
                new StructSettingHandler<RenderSettings, Vector4, Color>(
                    new RenderColorCapturedSetting(nameof(RenderSettings.ambientEquatorColor))
                )
            },
            {
                nameof(RenderSettings.ambientGroundColor),
                new StructSettingHandler<RenderSettings, Vector4, Color>(
                    new RenderColorCapturedSetting(nameof(RenderSettings.ambientGroundColor))
                )
            },
            {
                nameof(RenderSettings.ambientIntensity),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.ambientIntensity))
                )
            },
            {
                nameof(RenderSettings.ambientLight),
                new StructSettingHandler<RenderSettings, Vector4, Color>(
                    new RenderColorCapturedSetting(nameof(RenderSettings.ambientLight))
                )
            },
            {
                nameof(RenderSettings.ambientMode),
                new StructSettingHandler<RenderSettings, float, AmbientMode>(
                    new RenderEnumCapturedSetting<AmbientMode>(nameof(RenderSettings.ambientMode))
                )
            },
            {
                nameof(RenderSettings.ambientSkyColor),
                new StructSettingHandler<RenderSettings, Vector4, Color>(
                    new RenderColorCapturedSetting(nameof(RenderSettings.ambientSkyColor))
                )
            },
            {
                nameof(RenderSettings.defaultReflectionMode),
                new StructSettingHandler<RenderSettings, float, DefaultReflectionMode>(
                    new RenderEnumCapturedSetting<DefaultReflectionMode>(
                        nameof(RenderSettings.defaultReflectionMode)
                    )
                )
            },
            {
                nameof(RenderSettings.defaultReflectionResolution),
                new StructSettingHandler<RenderSettings, float, int>(
                    new RenderIntCapturedSetting(nameof(RenderSettings.defaultReflectionResolution))
                )
            },
            {
                nameof(RenderSettings.flareFadeSpeed),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.flareFadeSpeed))
                )
            },
            {
                nameof(RenderSettings.flareStrength),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.flareStrength))
                )
            },
            {
                nameof(RenderSettings.fog),
                new StructSettingHandler<RenderSettings, float, bool>(
                    new RenderBoolCapturedSetting(nameof(RenderSettings.fog))
                )
            },
            {
                nameof(RenderSettings.fogColor),
                new StructSettingHandler<RenderSettings, Vector4, Color>(
                    new RenderColorCapturedSetting(nameof(RenderSettings.fogColor))
                )
            },
            {
                nameof(RenderSettings.fogDensity),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.fogDensity))
                )
            },
            {
                nameof(RenderSettings.fogEndDistance),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.fogEndDistance))
                )
            },
            {
                nameof(RenderSettings.fogMode),
                new StructSettingHandler<RenderSettings, float, FogMode>(
                    new RenderEnumCapturedSetting<FogMode>(nameof(RenderSettings.fogMode))
                )
            },
            {
                nameof(RenderSettings.fogStartDistance),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.fogStartDistance))
                )
            },
            {
                nameof(RenderSettings.haloStrength),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.haloStrength))
                )
            },
            {
                nameof(RenderSettings.reflectionBounces),
                new StructSettingHandler<RenderSettings, float, int>(
                    new RenderIntCapturedSetting(nameof(RenderSettings.reflectionBounces))
                )
            },
            {
                nameof(RenderSettings.reflectionIntensity),
                new StructSettingHandler<RenderSettings, float, float>(
                    new RenderFloatCapturedSetting(nameof(RenderSettings.reflectionIntensity))
                )
            },
            {
                nameof(RenderSettings.subtractiveShadowColor),
                new StructSettingHandler<RenderSettings, Vector4, Color>(
                    new RenderColorCapturedSetting(nameof(RenderSettings.subtractiveShadowColor))
                )
            },
            {
                nameof(QualitySettings.anisotropicFiltering),
                new StructSettingHandler<QualitySettings, float, AnisotropicFiltering>(
                    new QualityEnumCapturedSetting<AnisotropicFiltering>(
                        nameof(QualitySettings.anisotropicFiltering)
                    )
                )
            },
            {
                nameof(QualitySettings.antiAliasing),
                new StructSettingHandler<QualitySettings, float, int>(
                    new QualityIntCapturedSetting(nameof(QualitySettings.antiAliasing))
                )
            },
            {
                nameof(QualitySettings.pixelLightCount),
                new StructSettingHandler<QualitySettings, float, int>(
                    new QualityIntCapturedSetting(nameof(QualitySettings.pixelLightCount))
                )
            },
            {
                nameof(QualitySettings.realtimeReflectionProbes),
                new StructSettingHandler<QualitySettings, float, bool>(
                    new QualityBoolCapturedSetting(nameof(QualitySettings.realtimeReflectionProbes))
                )
            },
            {
                nameof(QualitySettings.shadowCascades),
                new StructSettingHandler<QualitySettings, float, int>(
                    new QualityIntCapturedSetting(nameof(QualitySettings.shadowCascades))
                )
            },
            {
                nameof(QualitySettings.shadowDistance),
                new StructSettingHandler<QualitySettings, float, float>(
                    new QualityFloatCapturedSetting(nameof(QualitySettings.shadowDistance))
                )
            },
            {
                nameof(QualitySettings.shadowmaskMode),
                new StructSettingHandler<QualitySettings, float, ShadowmaskMode>(
                    new QualityEnumCapturedSetting<ShadowmaskMode>(
                        nameof(QualitySettings.shadowmaskMode)
                    )
                )
            },
            {
                nameof(QualitySettings.shadowNearPlaneOffset),
                new StructSettingHandler<QualitySettings, float, float>(
                    new QualityFloatCapturedSetting(nameof(QualitySettings.shadowNearPlaneOffset))
                )
            },
            {
                nameof(QualitySettings.shadowProjection),
                new StructSettingHandler<QualitySettings, float, ShadowProjection>(
                    new QualityEnumCapturedSetting<ShadowProjection>(
                        nameof(QualitySettings.shadowProjection)
                    )
                )
            },
            {
                nameof(QualitySettings.shadowResolution),
                new StructSettingHandler<QualitySettings, float, ShadowResolution>(
                    new QualityEnumCapturedSetting<ShadowResolution>(
                        nameof(QualitySettings.shadowResolution)
                    )
                )
            },
            {
                nameof(QualitySettings.shadows),
                new StructSettingHandler<QualitySettings, float, ShadowQuality>(
                    new QualityEnumCapturedSetting<ShadowQuality>(nameof(QualitySettings.shadows))
                )
            },
            {
                nameof(QualitySettings.softParticles),
                new StructSettingHandler<QualitySettings, float, bool>(
                    new QualityBoolCapturedSetting(nameof(QualitySettings.softParticles))
                )
            },
            {
                nameof(XRSettings.useOcclusionMesh),
                new StructSettingHandler<XRSettingsSetter, float, bool>(
                    new BoolCapturedSetting<XRSettingsSetter>(nameof(XRSettings.useOcclusionMesh))
                )
            },
        };

        private EditorSetRenderingSettings(
            EditorAssetBundleManager assetBundleManager,
            PrefabManager prefabManager
        )
        {
            _settings.Add(
                "skybox",
                new ClassSettingHandler<string, Material>(
                    new EditorRenderMaterialCapturedSetting(
                        nameof(RenderSettings.skybox),
                        assetBundleManager
                    )
                )
            );
            _settings.Add(
                "sun",
                new ClassSettingHandler<string, Light>(
                    new RenderLightCapturedSetting(nameof(RenderSettings.sun), prefabManager)
                )
            );
        }

        public class EditorRenderMaterialCapturedSetting
            : CapturedSettings<RenderSettings, Material>
        {
            internal EditorRenderMaterialCapturedSetting(
                string property,
                EditorAssetBundleManager assetBundleManager
            )
                : base(
                    property,
                    obj =>
                        assetBundleManager.TryGetAsset((string)obj, out Material? material)
                            ? material
                            : null
                ) { }
        }

        private interface ISettingHandler
        {
            public void Capture();

            public void ApplyAtProgress(RenderingSettingsProperty property, float progress);

            public void Reset();
        }

        public void Dispose()
        {
            foreach (ISettingHandler settingHandler in _settings.Values)
            {
                settingHandler.Reset();
            }
        }

        public void Initialize()
        {
            foreach (ISettingHandler settingHandler in _settings.Values)
            {
                settingHandler.Capture();
            }
        }

        internal void ApplyAtProgress(RenderingSettingsProperty property, float progress)
        {
            if (_settings.TryGetValue(property.Name, out ISettingHandler settingHandler))
            {
                settingHandler.ApplyAtProgress(property, progress);
            }
        }

        internal void Reset(string name)
        {
            if (_settings.TryGetValue(name, out ISettingHandler settingHandler))
            {
                settingHandler.Reset();
            }
        }

        private class StructSettingHandler<TSettings, THandled, TProperty> : ISettingHandler
            where TSettings : class
            where THandled : struct
        {
            private readonly CapturedSettings<TSettings, TProperty> _capturedSetting;

            internal StructSettingHandler(CapturedSettings<TSettings, TProperty> capturedSetting)
            {
                _capturedSetting = capturedSetting;
            }

            public void Capture()
            {
                _capturedSetting.Capture();
            }

            public void ApplyAtProgress(RenderingSettingsProperty property, float progress)
            {
                switch (property)
                {
                    case AnimatedRenderingSettingsProperty<THandled> animated:
                        _capturedSetting.Set(animated.PointDefinition.Interpolate(progress));
                        break;
                    case RenderingSettingsProperty<THandled> value:
                        _capturedSetting.Set(value.Value);
                        DynamicGI.UpdateEnvironment();
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Could not handle type [{property.GetType().FullName}]."
                        );
                }
            }

            public void Reset()
            {
                _capturedSetting.Reset();
            }
        }

        private class ClassSettingHandler<THandled, TProperty> : ISettingHandler
            where THandled : class
        {
            private readonly CapturedSettings<RenderSettings, TProperty> _capturedSetting;

            internal ClassSettingHandler(
                CapturedSettings<RenderSettings, TProperty> capturedSetting
            )
            {
                _capturedSetting = capturedSetting;
            }

            public void Capture()
            {
                _capturedSetting.Capture();
            }

            public void ApplyAtProgress(RenderingSettingsProperty property, float progress)
            {
                switch (property)
                {
                    case RenderingSettingsProperty<THandled> value:
                        _capturedSetting.Set(value.Value);
                        DynamicGI.UpdateEnvironment();
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Could not handle type [{property.GetType().FullName}]."
                        );
                }
            }

            public void Reset()
            {
                _capturedSetting.Reset();
            }
        }
    }
}
