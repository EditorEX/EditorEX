using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using EditorEX.NoodleExtensions.Animation;
using Heck.Animation.Transform;
using NoodleExtensions;
using UnityEngine;

namespace EditorEX.NoodleExtensions.Events
{
    internal sealed class AssignTrackParentPreviewAction : IPreviewStateAction
    {
        private readonly NoodleParentTrackEventData _data;
        private readonly bool _leftHanded;
        private readonly bool _v2;
        private readonly TransformControllerFactory _transformControllerFactory;
        private readonly HashSet<EditorParentObject> _parentObjects;

        private EditorParentObject? _instance;

        public AssignTrackParentPreviewAction(
            NoodleParentTrackEventData data,
            bool leftHanded,
            bool v2,
            TransformControllerFactory transformControllerFactory,
            HashSet<EditorParentObject> parentObjects
        )
        {
            _data = data;
            _leftHanded = leftHanded;
            _v2 = v2;
            _transformControllerFactory = transformControllerFactory;
            _parentObjects = parentObjects;
        }

        public void Execute()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject parentGameObject = new($"ParentObject {_data.ParentTrack}");
            EditorParentObject instance = parentGameObject.AddComponent<EditorParentObject>();
            instance.Init(_data, _leftHanded, _parentObjects);
            if (_v2)
            {
                instance.ApplyV2Transform(_data);
                _instance = instance;
                return;
            }

            instance.enabled = false;
            _data.TransformData.Apply(instance.transform, _leftHanded);
            PreviewOriginalTransform restorer =
                parentGameObject.GetComponent<PreviewOriginalTransform>()
                ?? parentGameObject.AddComponent<PreviewOriginalTransform>();
            restorer.Capture(_data.TransformData, _leftHanded, v2: false);
            _transformControllerFactory.Create(parentGameObject, _data.ParentTrack);
            _instance = instance;
        }

        public void Reverse()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.Teardown(_parentObjects);
            _instance = null;
        }

        public void Tick(float beat) { }
    }
}
