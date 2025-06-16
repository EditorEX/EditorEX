using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.CustomEvents;
using Heck.Deserialize;

namespace EditorEX.Heck.Deserialize
{
    public interface IEditorEarlyDeserializer
    {
        public void DeserializeEarly();
    }

    public interface IEditorCustomEventsDeserializer
    {
        public Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents();
    }

    public interface IEditorEventsDeserializer
    {
        public Dictionary<BasicEventEditorData, IEventCustomData> DeserializeEvents();
    }

    public interface IEditorObjectsDeserializer
    {
        public Dictionary<BaseEditorData?, IObjectCustomData> DeserializeObjects();
    }
}
