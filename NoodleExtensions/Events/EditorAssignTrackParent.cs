using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.Animation;
using Heck;
using Heck.Animation.Transform;
using Heck.Event;
using NoodleExtensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.Events
{
    [CustomEvent("AssignTrackParent")]
    internal class EditorAssignTrackParent : ICustomEvent
    {
        internal EditorAssignTrackParent(IReadonlyBeatmapData beatmapData, [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData deserializedData, [Inject(Id = "leftHanded")] bool leftHanded, TransformControllerFactory transformControllerFactory)
        {
            _version = ((CustomBeatmapData)beatmapData).version;
            _editorDeserializedData = deserializedData;
            _leftHanded = leftHanded;
            _transformControllerFactory = transformControllerFactory;
        }

        public void Callback(CustomEventData customEventData)
        {
            NoodleParentTrackEventData noodleData;
            if (!(_editorDeserializedData?.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out noodleData) ?? false))
            {
                return;
            }
            GameObject parentGameObject = new GameObject($"ParentObject {customEventData.customData.Get<string>("_parentTrack")}");
            EditorParentObject instance = parentGameObject.AddComponent<EditorParentObject>();
            instance.Init(noodleData, _leftHanded, _parentObjects);
            if (_version.Major == 2)
            {
                instance.ApplyV2Transform(noodleData);
                return;
            }
            instance.enabled = false;
            noodleData.TransformData.Apply(instance.transform, _leftHanded);
            _transformControllerFactory.Create(parentGameObject, noodleData.ParentTrack);
        }

        private readonly Version _version;

        private readonly EditorDeserializedData _editorDeserializedData;

        private readonly bool _leftHanded;

        private readonly TransformControllerFactory _transformControllerFactory;

        private readonly HashSet<EditorParentObject> _parentObjects = new();
    }
}
