﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BetterEditor.Chroma.Events;
using BetterEditor.CustomJSONData;
using BetterEditor.Heck.Deserializer;
using Chroma;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using Zenject;

namespace BetterEditor.Chroma.Patches.Events
{
    internal class EditorRingStepChromafier : IAffinity, IDisposable
    {
        private static readonly FieldInfo _moveSpeedField = AccessTools.Field(typeof(TrackLaneRingsPositionStepEffectSpawner), "_moveSpeed");

        private readonly CodeInstruction _getPrecisionStep;
        private readonly CodeInstruction _getPrecisionSpeed;
        private readonly EditorDeserializedData _deserializedData;

        private EditorRingStepChromafier([Inject(Id = ChromaController.ID)] EditorDeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
            _getPrecisionStep = InstanceTranspilers.EmitInstanceDelegate<Func<float, BasicBeatmapEventData, float>>(GetPrecisionStep);
            _getPrecisionSpeed = InstanceTranspilers.EmitInstanceDelegate<Func<float, BasicBeatmapEventData, float>>(GetPrecisionSpeed);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_getPrecisionStep);
            InstanceTranspilers.DisposeDelegate(_getPrecisionSpeed);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(TrackLaneRingsPositionStepEffectSpawner), nameof(TrackLaneRingsPositionStepEffectSpawner.HandleBeatmapEvent))]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- float num = (basicBeatmapEventData.sameTypeIndex % 2 == 0) ? this._maxPositionStep : this._minPositionStep;
                 * ++ float num = GetPrecisionStep((basicBeatmapEventData.sameTypeIndex % 2 == 0) ? this._maxPositionStep : this._minPositionStep, basicBeatmapEventData);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
                .SetOpcodeAndAdvance(OpCodes.Ldarg_1)
                .InsertAndAdvance(
                    _getPrecisionStep,
                    new CodeInstruction(OpCodes.Stloc_0))

                /*
                 * -- rings[i].SetPosition(destPosZ, this._moveSpeed);
                 * ++ rings[i].SetPosition(destPosZ, GetPrecisionSpeed(this._moveSpeed, basicBeatmapEventData));
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _moveSpeedField))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    _getPrecisionSpeed)
                .InstructionEnumeration();
        }

        private float GetPrecisionStep(float @default, BasicBeatmapEventData beatmapEventData)
        {
            _deserializedData.Resolve(CustomDataRepository.GetBasicEventConversion(beatmapEventData), out EditorChromaEventData? chromaData);
            return chromaData is { Step: not null } ? chromaData.Step.Value : @default;
        }

        private float GetPrecisionSpeed(float @default, BasicBeatmapEventData beatmapEventData)
        {
            _deserializedData.Resolve(CustomDataRepository.GetBasicEventConversion(beatmapEventData), out EditorChromaEventData? chromaData);
            return chromaData is { Speed: not null } ? chromaData.Speed.Value : @default;
        }
    }
}