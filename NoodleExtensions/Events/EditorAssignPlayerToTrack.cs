﻿using CustomJSONData.CustomBeatmap;
using Heck.Deserialize;
using Heck.Event;
using Heck;
using NoodleExtensions.Animation;
using NoodleExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using EditorEX.Heck.Deserialize;
using EditorEX.CustomJSONData;

namespace EditorEX.NoodleExtensions.Events
{
    [CustomEvent(NoodleController.ASSIGN_PLAYER_TO_TRACK)]
    internal class EditorAssignPlayerToTrack : ICustomEvent
    {
        private readonly IInstantiator _container;
        private readonly PlayerTransforms _playerTransforms;
        private readonly EditorDeserializedData _editorDeserializedData;
        private readonly Dictionary<PlayerTrackObject, PlayerTrack> _playerTracks = new();

        private EditorAssignPlayerToTrack(
            IInstantiator container,
            PlayerTransforms playerTransforms,
            [Inject(Id = NoodleController.ID)] EditorDeserializedData editorDeserializedData)
        {
            _container = container;
            _playerTransforms = playerTransforms;
            _editorDeserializedData = editorDeserializedData;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_editorDeserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out NoodlePlayerTrackEventData? noodlePlayerData))
            {
                return;
            }

            PlayerTrackObject resultPlayerTrackObject = noodlePlayerData.PlayerTrackObject;
            if (!_playerTracks.TryGetValue(resultPlayerTrackObject, out PlayerTrack? playerTrack))
            {
                _playerTracks[resultPlayerTrackObject] = playerTrack = Create(resultPlayerTrackObject);
            }

            playerTrack.AssignTrack(noodlePlayerData.Track);
        }

        private PlayerTrack Create(PlayerTrackObject playerTrackObject)
        {
            GameObject noodleObject = new($"NoodlePlayerTrack{playerTrackObject}");
            Transform origin = noodleObject.transform;

            Transform target = playerTrackObject switch
            {
                PlayerTrackObject.Root => _playerTransforms._originTransform.parent,
                PlayerTrackObject.Head => _playerTransforms._headTransform,
                PlayerTrackObject.LeftHand => _playerTransforms._leftHandTransform,
                PlayerTrackObject.RightHand => _playerTransforms._rightHandTransform,
                _ => throw new ArgumentOutOfRangeException(nameof(playerTrackObject), playerTrackObject, null)
            };

            origin.SetParent(target.parent, false);
            target.SetParent(origin, true);

            return _container.InstantiateComponent<PlayerTrack>(noodleObject, new object[] { playerTrackObject });
        }
    }
}