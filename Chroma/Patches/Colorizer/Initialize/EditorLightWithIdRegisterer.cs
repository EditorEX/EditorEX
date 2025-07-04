﻿using System.Collections.Generic;
using Chroma.Lighting;
using EditorEX.Chroma.Colorizer;
using SiraUtil.Affinity;
using UnityEngine;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Chroma.HarmonyPatches.Colorizer.Initialize
{
    internal class EditorLightWithIdRegisterer : IAffinity
    {
        private readonly Dictionary<ILightWithId, int> _requestedIds = new();
        private readonly HashSet<ILightWithId> _needToRegister = new();
        private readonly EditorLightColorizerManager _colorizerManager;
        private readonly LightIDTableManager _tableManager;
        private readonly LightWithIdManager _lightWithIdManager;

        private EditorLightWithIdRegisterer(
            EditorLightColorizerManager colorizerManager,
            LightWithIdManager lightWithIdManager,
            LightIDTableManager tableManager
        )
        {
            _colorizerManager = colorizerManager;
            _lightWithIdManager = lightWithIdManager;
            _tableManager = tableManager;
        }

        internal void SetRequestedId(ILightWithId lightWithId, int id)
        {
            _requestedIds[lightWithId] = id;
        }

        internal void MarkForTableRegister(ILightWithId lightWithId)
        {
            _needToRegister.Add(lightWithId);
        }

        internal void ForceUnregister(ILightWithId lightWithId)
        {
            int lightId = lightWithId.lightId;
            List<ILightWithId> lights = _lightWithIdManager._lights[lightId];
            int index = lights.FindIndex(n => n == lightWithId);
            lights[index] = null!; // TODO: handle null
            _tableManager.UnregisterIndex(lightId, index);
            _colorizerManager.CreateLightColorizerContractByLightID(
                lightId,
                n => n.ChromaLightSwitchEventEffect.UnregisterLight(lightWithId)
            );
            lightWithId.__SetIsUnRegistered();
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.RegisterLight))]
        private bool Prefix(
            LightWithIdManager __instance,
            ILightWithId lightWithId,
            List<ILightWithId>?[] ____lights,
            List<ILightWithId> ____lightsToUnregister
        )
        {
            // TODO: figure this shit out
            // for some reason, despite being an affinity patch bound to player, this still runs in the menu scene
            // so quick and dirty fix
            if (__instance.gameObject.scene.name.Contains("Menu"))
            {
                return true;
            }

            if (lightWithId.isRegistered)
            {
                return false;
            }

            int lightId = lightWithId.lightId;
            if (lightId == -1)
            {
                return false;
            }

            List<ILightWithId>? lights = ____lights[lightId];
            if (lights == null)
            {
                lights = new List<ILightWithId>(10);
                ____lights[lightId] = lights;
            }

            lightWithId.__SetIsRegistered();

            if (lights.Contains(lightWithId))
            {
                return false;
            }

            // TODO: find a better way to register "new" lights to table
            int index = lights.Count;
            if (_needToRegister.Remove(lightWithId))
            {
                int? tableId = _requestedIds.TryGetValue(lightWithId, out int value) ? value : null;
                _tableManager.RegisterIndex(lightId, index, tableId);
            }

            // this also colors the light
            _colorizerManager.CreateLightColorizerContractByLightID(
                lightId,
                n => n.ChromaLightSwitchEventEffect.RegisterLight(lightWithId, index)
            );

            lights.Add(lightWithId);

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.UnregisterLight))]
        private bool DontClearList(ILightWithId lightWithId)
        {
            lightWithId.__SetIsUnRegistered();
            return false;
        }

        // too lazy to make a transpiler
        [AffinityPrefix]
        [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.SetColorForId))]
        private bool AllowNull(
            int lightId,
            Color color,
            List<ILightWithId?>?[] ____lights,
            Color?[] ____colors,
            bool ____didChangeSomeColorsThisFrame
        )
        {
            ____colors[lightId] = color;
            ____didChangeSomeColorsThisFrame = true;
            ____lights[lightId]
                ?.ForEach(n =>
                {
                    if (n is { isRegistered: true })
                    {
                        n.ColorWasSet(color);
                    }
                });
            return false;
        }
    }
}
