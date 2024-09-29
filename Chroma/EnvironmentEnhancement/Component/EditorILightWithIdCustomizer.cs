using Chroma.HarmonyPatches.Colorizer.Initialize;
using CustomJSONData.CustomBeatmap;
using EditorEX.Chroma.Colorizer;
using JetBrains.Annotations;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace EditorEx.Chroma.EnvironmentEnhancement.Component
{
    internal class EditorILightWithIdCustomizer
    {
        private readonly SiraLog _log;
        private readonly EditorLightColorizerManager _lightColorizerManager;
        private readonly EditorLightWithIdRegisterer _lightWithIdRegisterer;
        private readonly LightWithIdManager _lightWithIdManager;

        [UsedImplicitly]
        private EditorILightWithIdCustomizer(
            SiraLog log,
            EditorLightColorizerManager lightColorizerManager,
            EditorLightWithIdRegisterer lightWithIdRegisterer,
            LightWithIdManager lightWithIdManager)
        {
            _log = log;
            _lightColorizerManager = lightColorizerManager;
            _lightWithIdRegisterer = lightWithIdRegisterer;
            _lightWithIdManager = lightWithIdManager;
        }

        internal void ILightWithIdInit(List<UnityEngine.Component> allComponents, CustomData customData)
        {
            ILightWithId[] lightWithIds = allComponents
                .OfType<LightWithIds>()
                .SelectMany(n => n._lightWithIds)
                .Cast<ILightWithId>()
                .Concat(allComponents.OfType<LightWithIdMonoBehaviour>())
                .ToArray();
            if (lightWithIds.Length == 0)
            {
                _log.Error($"No [{LIGHT_WITH_ID}] component found");
                return;
            }

            int? lightID = customData.Get<int?>(LIGHT_ID);
            int? type = customData.Get<int?>(LIGHT_TYPE);
            if (!type.HasValue && !lightID.HasValue)
            {
                return;
            }

            foreach (ILightWithId lightWithId in lightWithIds)
            {
                if (lightWithId.isRegistered)
                {
                    _lightWithIdRegisterer.ForceUnregister(lightWithId);
                    _lightWithIdRegisterer.MarkForTableRegister(lightWithId);
                    SetType();
                    SetLightID();
                    _lightWithIdManager.RegisterLight(lightWithId);
                }
                else
                {
                    SetType();
                    SetLightID();
                }

                continue;

                void SetLightID()
                {
                    if (lightID.HasValue)
                    {
                        _lightWithIdRegisterer.SetRequestedId(lightWithId, lightID.Value);
                    }
                }

                void SetType()
                {
                    if (!type.HasValue)
                    {
                        return;
                    }

                    int lightId = _lightColorizerManager.GetColorizer((BasicBeatmapEventType)type.Value).ChromaLightSwitchEventEffect.LightsID;

                    switch (lightWithId)
                    {
                        case LightWithIds.LightWithId lightWithIdsLightWithId:
                            lightWithIdsLightWithId._lightId = lightId;
                            break;

                        case LightWithIdMonoBehaviour lightWithIdMonoBehaviour:
                            lightWithIdMonoBehaviour._ID = lightId;
                            break;
                    }
                }
            }
        }
    }
}
