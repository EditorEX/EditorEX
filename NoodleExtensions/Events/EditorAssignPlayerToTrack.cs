using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.Controller;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using Heck;
using Heck.Event;
using NoodleExtensions;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using UnityEngine;
using Zenject;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.Events
{
    [CustomEvent(NoodleController.ASSIGN_PLAYER_TO_TRACK)]
    internal class EditorAssignPlayerToTrack : ICustomEvent
    {
        private readonly IInstantiator _container;
        private readonly PlayerTransforms _playerTransforms;
        private readonly EditorDeserializedData _editorDeserializedData;
        private readonly Dictionary<PlayerObject, PlayerTrack> _playerTracks = new();
        private readonly BeatmapEditor360CameraController _beatmapEditor360CameraController;

        private EditorAssignPlayerToTrack(
            IInstantiator container,
            PlayerTransforms playerTransforms,
            [InjectOptional(Id = NoodleController.ID)] EditorDeserializedData editorDeserializedData
        )
        {
            _container = container;
            _playerTransforms = playerTransforms;
            _editorDeserializedData = editorDeserializedData;
            _beatmapEditor360CameraController = Resources
                .FindObjectsOfTypeAll<BeatmapEditor360CameraController>()
                .FirstOrDefault();
        }

        public void Callback(CustomEventData customEventData)
        {
            if (
                !(
                    _editorDeserializedData?.Resolve(
                        CustomDataRepository.GetCustomEventConversion(customEventData),
                        out NoodlePlayerTrackEventData? noodlePlayerData
                    ) ?? false
                )
            )
            {
                return;
            }

            PlayerObject resultPlayerTrackObject = noodlePlayerData.PlayerObject;
            if (!_playerTracks.TryGetValue(resultPlayerTrackObject, out PlayerTrack? playerTrack))
            {
                _playerTracks[resultPlayerTrackObject] = playerTrack = Create(
                    resultPlayerTrackObject
                );
            }

            playerTrack.AssignTrack(noodlePlayerData.Track);
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

            if (playerTrackObject == PlayerObject.Root)
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
