using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.CustomEvents;
using CustomJSONData;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Heck.Deserialize;

namespace EditorEX.Heck.Deserialize
{
    public class EditorDataDeserializer
    {
        private readonly ConstructorInfo _constructor;

        private object? _instance;

        internal EditorDataDeserializer(string? id, Type type)
        {
            _constructor = AccessTools.FirstConstructor(type, _ => true);
            Id = id;
        }

        internal bool Enabled { get; set; }

        internal string? Id { get; }

        public override string ToString()
        {
            return Id ?? "NULL";
        }

        internal void Create(object[] inputs)
        {
            _instance = _constructor.Invoke(_constructor.ActualParameters(inputs));
            if (_instance is IEditorEarlyDeserializer earlyDeserializer)
            {
                earlyDeserializer.DeserializeEarly();
            }
        }

        internal EditorDeserializedData Deserialize()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("No instance found");
            }

            Dictionary<CustomEventEditorData, ICustomEventCustomData>? customEventDatas = null;
            if (_instance is IEditorCustomEventsDeserializer customEventsDeserializer)
            {
                customEventDatas = customEventsDeserializer.DeserializeCustomEvents();
            }

            Dictionary<BasicEventEditorData, IEventCustomData>? eventDatas = null;
            if (_instance is IEditorEventsDeserializer eventsDeserializer)
            {
                eventDatas = eventsDeserializer.DeserializeEvents();
            }

            Dictionary<BaseEditorData, IObjectCustomData>? objectDatas = null;
            if (_instance is IEditorObjectsDeserializer objectsDeserializer)
            {
                objectDatas = objectsDeserializer.DeserializeObjects();
            }

            customEventDatas ??= new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
            eventDatas ??= new Dictionary<BasicEventEditorData, IEventCustomData>();
            objectDatas ??= new Dictionary<BaseEditorData, IObjectCustomData>();

            return new EditorDeserializedData(customEventDatas, eventDatas, objectDatas);
        }
    }
}
