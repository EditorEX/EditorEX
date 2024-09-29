using BeatmapEditor3D.DataModels;
using Chroma;
using Chroma.Lighting;
using CustomJSONData.CustomBeatmap;
using EditorEX.Chroma.Lighting;
using EditorEX.Util;
using Heck.Animation;
using Heck.Deserialize;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Chroma.ChromaEventData;
using static EditorEX.Chroma.Constants;
using static EditorEX.Heck.Constants;

namespace EditorEX.Chroma.Events
{
    internal class EditorChromaEventData : IEventCustomData
    {
        internal EditorChromaEventData(
            BasicEventEditorData beatmapEventData,
            EditorLegacyLightHelper legacyLightHelper,
            bool v2)
        {
            CustomData customData = beatmapEventData.GetCustomData();

            Color? color = global::Chroma.CustomDataDeserializer.GetColorFromData(customData, v2);
            if (legacyLightHelper != null)
            {
                color = color ?? legacyLightHelper.GetLegacyColor(beatmapEventData);
            }

            PropID = v2 ? customData.Get<object>(V2_PROPAGATION_ID) : null;
            ColorData = color;
            Easing = customData.GetStringToEnum<Functions?>(v2 ? V2_EASING : EASING);
            LerpType = customData.GetStringToEnum<LerpType?>(v2 ? V2_LERP_TYPE : LERP_TYPE);
            LockPosition = customData.Get<bool?>(v2 ? V2_LOCK_POSITION : LOCK_POSITION).GetValueOrDefault(false);
            NameFilter = customData.Get<string>(v2 ? V2_NAME_FILTER : NAME_FILTER);
            Direction = customData.Get<int?>(v2 ? V2_DIRECTION : DIRECTION);
            CounterSpin = v2 ? customData.Get<bool?>(V2_COUNTER_SPIN) : null;
            Reset = v2 ? customData.Get<bool?>(V2_RESET) : null;
            Step = customData.Get<float?>(v2 ? V2_STEP : STEP);
            Prop = customData.Get<float?>(v2 ? V2_PROP : PROP);
            Speed = customData.Get<float?>(v2 ? V2_SPEED : SPEED) ?? (v2 ? customData.Get<float?>(V2_PRECISE_SPEED) : null);
            Rotation = customData.Get<float?>(v2 ? V2_ROTATION : RING_ROTATION);
            StepMult = v2 ? customData.Get<float?>(V2_STEP_MULT).GetValueOrDefault(1f) : 1;
            PropMult = v2 ? customData.Get<float?>(V2_PROP_MULT).GetValueOrDefault(1f) : 1;
            SpeedMult = v2 ? customData.Get<float?>(V2_SPEED_MULT).GetValueOrDefault(1f) : 1;

            if (v2)
            {
                CustomData gradientObject = customData.Get<CustomData>(V2_LIGHT_GRADIENT);
                if (gradientObject != null)
                {
                    GradientObject = new GradientObjectData(
                        gradientObject.Get<float>(V2_DURATION),
                        CustomDataDeserializer.GetColorFromData(gradientObject, V2_START_COLOR) ?? Color.white,
                        CustomDataDeserializer.GetColorFromData(gradientObject, V2_END_COLOR) ?? Color.white,
                        gradientObject.GetStringToEnum<Functions?>(V2_EASING) ?? Functions.easeLinear);
                }
            }

            object lightID = customData.Get<object>(v2 ? V2_LIGHT_ID : ChromaController.LIGHT_ID);
            if (lightID != null)
            {
                switch (lightID)
                {

                }
                LightID = lightID switch
                {
                    List<object> lightIDobjects => lightIDobjects.Select(Convert.ToInt32),
                    long lightIDint => new[] { (int)lightIDint },
                    _ => null
                };
            }
        }

        internal IEnumerable<int> LightID { get; }

        internal object PropID { get; }

        internal Color? ColorData { get; }

        internal GradientObjectData GradientObject { get; }

        internal Functions? Easing { get; }

        internal LerpType? LerpType { get; }

        internal bool LockPosition { get; }

        internal string NameFilter { get; }

        internal int? Direction { get; }

        internal bool? CounterSpin { get; }

        internal bool? Reset { get; }

        internal float? Step { get; }

        internal float? Prop { get; }

        internal float? Speed { get; }

        internal float? Rotation { get; }

        internal float StepMult { get; }

        internal float PropMult { get; }

        internal float SpeedMult { get; }

        internal Dictionary<int, BasicEventEditorData> NextSameTypeEvent { get; set; }
    }
}
