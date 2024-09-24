using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.CustomEvents;
using CustomJSONData;
using HarmonyLib;
using Heck;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EditorEX.Heck.Deserializer
{
    public class EditorDataDeserializer
    {
        internal EditorDataDeserializer(object id, IReflect type)
        {
            Id = id;
            MethodInfo[] methods = type.GetMethods(AccessTools.allDeclared);
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                AccessAttribute<EarlyDeserializer>(ref _earlyMethod, ref method);
                AccessAttribute<CustomEventsDeserializer>(ref _customEventMethod, ref method);
                AccessAttribute<EventsDeserializer>(ref _beatmapEventMethod, ref method);
                AccessAttribute<ObjectsDeserializer>(ref _beatmapObjectMethod, ref method);
            }
        }

        public bool Enabled { get; set; }

        public object Id { get; }

        public override string ToString()
        {
            object id = Id;
            return (id?.ToString()) ?? "NULL";
        }

        internal void InjectedInvokeEarly(object[] inputs)
        {
            MethodInfo earlyMethod = _earlyMethod;
            if (earlyMethod == null)
            {
                return;
            }
            earlyMethod.Invoke(null, _earlyMethod.ActualParameters(inputs));
        }

        internal Dictionary<CustomEventEditorData, ICustomEventCustomData> InjectedInvokeCustomEvent(object[] inputs)
        {
            return InjectedInvoke<Dictionary<CustomEventEditorData, ICustomEventCustomData>>(_customEventMethod, inputs);
        }

        internal Dictionary<BasicEventEditorData, IEventCustomData> InjectedInvokeEvent(object[] inputs)
        {
            return InjectedInvoke<Dictionary<BasicEventEditorData, IEventCustomData>>(_beatmapEventMethod, inputs);
        }

        internal Dictionary<BaseEditorData, IObjectCustomData> InjectedInvokeObject(object[] inputs)
        {
            return InjectedInvoke<Dictionary<BaseEditorData, IObjectCustomData>>(_beatmapObjectMethod, inputs);
        }

        private static T InjectedInvoke<T>(MethodInfo method, object[] inputs) where T : new()
        {
            T t = (T)((object)((method != null) ? method.Invoke(null, method.ActualParameters(inputs)) : null));
            if (t == null)
            {
                return new T();
            }
            return t;
        }

        internal static void AccessAttribute<TAttribute>(ref MethodInfo savedMethod, ref MethodInfo method) where TAttribute : Attribute
        {
            if (method.GetCustomAttribute<TAttribute>() == null)
            {
                return;
            }
            savedMethod = method;
        }

        private readonly MethodInfo _customEventMethod;

        private readonly MethodInfo _beatmapEventMethod;

        private readonly MethodInfo _beatmapObjectMethod;

        private readonly MethodInfo _earlyMethod;
    }
}
