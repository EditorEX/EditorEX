﻿using System;
using System.Collections.Generic;
using System.Linq;
using Heck;
using Heck.Animation;
using NoodleExtensions;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.Animation
{
    internal class EditorParentObject : MonoBehaviour
    {
        private Track _track = null!;
        private bool _worldPositionStays;
        private bool _leftHanded;
        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startRot = Quaternion.identity;
        private Quaternion _startLocalRot = Quaternion.identity;
        private Vector3 _startScale = Vector3.one;

        internal HashSet<Track> ChildrenTracks { get; } = new();

        internal void Init(
            NoodleParentTrackEventData noodleData,
            bool leftHanded,
            HashSet<EditorParentObject> parentObjects
        )
        {
            IReadOnlyList<Track> tracks = noodleData.ChildrenTracks;
            Track parentTrack = noodleData.ParentTrack;
            if (tracks.Contains(parentTrack))
            {
                throw new InvalidOperationException("How could a track contain itself?");
            }

            _track = parentTrack;
            _worldPositionStays = noodleData.WorldPositionStays;
            _leftHanded = leftHanded;

            parentTrack.AddGameObject(gameObject);

            foreach (Track track in tracks)
            {
                foreach (EditorParentObject parentObject in parentObjects)
                {
                    track.GameObjectAdded -= parentObject.OnTrackGameObjectAdded;
                    track.GameObjectRemoved -= OnTrackGameObjectRemoved;
                    parentObject.ChildrenTracks.Remove(track);
                }

                foreach (GameObject go in track.GameObjects)
                {
                    if (go == null)
                        continue;
                    ParentToObject(go.transform);
                }

                ChildrenTracks.Add(track);

                track.GameObjectAdded += OnTrackGameObjectAdded;
                track.GameObjectRemoved += OnTrackGameObjectRemoved;
            }

            parentObjects.Add(this);
        }

        internal void ApplyV2Transform(NoodleParentTrackEventData noodleData)
        {
            Vector3? startPos = noodleData.OffsetPosition;
            Quaternion? startRot = noodleData.WorldRotation;
            Quaternion? startLocalRot = noodleData.TransformData.LocalRotation;
            Vector3? startScale = noodleData.TransformData.Scale;

            Transform transform1 = transform;
            if (startPos.HasValue)
            {
                _startPos = startPos.Value;
                transform1.localPosition =
                    _startPos * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
            }

            if (startRot.HasValue)
            {
                _startRot = startRot.Value;
                _startLocalRot = _startRot;
                transform1.localPosition = _startRot * transform1.localPosition;
                transform1.localRotation = _startRot;
            }

            if (startLocalRot.HasValue)
            {
                _startLocalRot = _startRot * startLocalRot.Value;
                transform1.localRotation *= _startLocalRot;
            }

            // ReSharper disable once InvertIf
            if (startScale.HasValue)
            {
                _startScale = startScale.Value;
                transform1.localScale = _startScale;
            }
        }

        private static void OnTrackGameObjectRemoved(GameObject trackGameObject)
        {
            trackGameObject.transform.SetParent(null, false);
        }

        private void OnTrackGameObjectAdded(GameObject trackGameObject)
        {
            ParentToObject(trackGameObject.transform);
        }

        private void ParentToObject(Transform childTransform)
        {
            childTransform.SetParent(transform, _worldPositionStays);
        }

        private void OnDestroy()
        {
            foreach (Track track in ChildrenTracks)
            {
                track.GameObjectAdded -= OnTrackGameObjectAdded;
                track.GameObjectRemoved -= OnTrackGameObjectRemoved;
            }
        }

        private void Update()
        {
            Quaternion? rotation = _track
                .GetProperty<Quaternion>(OFFSET_ROTATION)
                ?.Mirror(_leftHanded);
            Vector3? position = _track.GetProperty<Vector3>(OFFSET_POSITION)?.Mirror(_leftHanded);

            Quaternion worldRotationQuatnerion = _startRot;
            Vector3 positionVector =
                worldRotationQuatnerion
                * (_startPos * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance);
            if (rotation.HasValue || position.HasValue)
            {
                Quaternion rotationOffset = rotation ?? Quaternion.identity;
                worldRotationQuatnerion *= rotationOffset;
                Vector3 positionOffset = position ?? Vector3.zero;
                positionVector =
                    worldRotationQuatnerion
                    * (
                        (positionOffset + _startPos)
                        * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance
                    );
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = _track
                .GetProperty<Quaternion>(LOCAL_ROTATION)
                ?.Mirror(_leftHanded);
            if (localRotation.HasValue)
            {
                worldRotationQuatnerion *= localRotation.Value;
            }

            Vector3 scaleVector = _startScale;
            Vector3? scale = _track.GetProperty<Vector3>(SCALE);
            if (scale.HasValue)
            {
                scaleVector = Vector3.Scale(_startScale, scale.Value);
            }

            Transform transform1 = transform;
            transform1.localRotation = worldRotationQuatnerion;
            transform1.localPosition = positionVector;
            transform1.localScale = scaleVector;
        }
    }
}
