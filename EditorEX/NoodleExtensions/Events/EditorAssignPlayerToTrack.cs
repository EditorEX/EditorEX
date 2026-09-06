using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.Controller;
using Heck.Animation;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using UnityEngine;
using Zenject;

namespace EditorEX.NoodleExtensions.Events
{
    internal class EditorAssignPlayerToTrack
    {
        private readonly IInstantiator _container;
        private readonly PlayerTransforms _playerTransforms;
        private readonly Dictionary<PlayerObject, PlayerTrack> _playerTracks = new();
        private readonly BeatmapEditor360CameraController? _beatmapEditor360CameraController;

        private EditorAssignPlayerToTrack(
            IInstantiator container,
            PlayerTransforms playerTransforms
        )
        {
            _container = container;
            _playerTransforms = playerTransforms;
            _beatmapEditor360CameraController = Resources
                .FindObjectsOfTypeAll<BeatmapEditor360CameraController>()
                .FirstOrDefault();
        }

        internal void Assign(PlayerObject playerObject, Track track)
        {
            if (!_playerTracks.TryGetValue(playerObject, out PlayerTrack? playerTrack))
            {
                _playerTracks[playerObject] = playerTrack = Create(playerObject);
            }

            playerTrack.AssignTrack(track);
        }

        internal void Restore(PlayerObject playerObject, Track? previous)
        {
            if (!_playerTracks.TryGetValue(playerObject, out PlayerTrack? playerTrack))
            {
                return;
            }

            if (previous != null)
            {
                playerTrack.AssignTrack(previous);
                return;
            }

            Clear(playerTrack);
        }

        private static void Clear(PlayerTrack playerTrack)
        {
            if (playerTrack._track != null)
            {
                playerTrack._track.RemoveGameObject(playerTrack.gameObject);
                playerTrack._track = null;
            }

            if (playerTrack._transformController != null)
            {
                UnityEngine.Object.Destroy(playerTrack._transformController);
                playerTrack._transformController = null;
            }
        }

        private PlayerTrack Create(PlayerObject playerTrackObject)
        {
            GameObject noodleObject = new($"NoodlePlayerTrack{playerTrackObject}");
            Transform origin = noodleObject.transform;

            Transform target = playerTrackObject switch
            {
                PlayerObject.Root => _playerTransforms._originTransform.parent,
                PlayerObject.Head => _playerTransforms._headTransform,
                PlayerObject.LeftHand => _playerTransforms._leftHandTransform,
                PlayerObject.RightHand => _playerTransforms._rightHandTransform,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(playerTrackObject),
                    playerTrackObject,
                    null
                ),
            };

            if (playerTrackObject == PlayerObject.Root && _beatmapEditor360CameraController != null)
            {
                _beatmapEditor360CameraController.transform.SetParent(origin, true);
            }

            origin.SetParent(target.parent, false);
            target.SetParent(origin, true);

            return _container.InstantiateComponent<PlayerTrack>(
                noodleObject,
                new object[] { playerTrackObject }
            );
        }
    }
}
