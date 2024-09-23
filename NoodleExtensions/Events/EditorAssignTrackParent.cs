using BetterEditor.CustomJSONData;
using BetterEditor.Heck.Deserializer;
using BetterEditor.Heck.EventData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Animation.Events;
using Heck.Animation.Transform;
using Heck.Event;
using JetBrains.Annotations;
using NoodleExtensions.Animation;
using NoodleExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BetterEditor.Heck.Events
{
	[CustomEvent(new string[] { "AssignTrackParent" })]
	internal class EditorAssignTrackParent : ICustomEvent
	{
		internal EditorAssignTrackParent(IReadonlyBeatmapData beatmapData, [Inject(Id = "NoodleExtensions")] EditorDeserializedData deserializedData, [Inject(Id = "leftHanded")] bool leftHanded, TransformControllerFactory transformControllerFactory)
		{
			_v2 = ((CustomBeatmapData)beatmapData).version2_6_0AndEarlier;
			_deserializedData = deserializedData;
			_leftHanded = leftHanded;
			_transformControllerFactory = transformControllerFactory;
		}

		public void Callback(CustomEventData customEventData)
		{
			NoodleParentTrackEventData noodleData;
			if (!_deserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out noodleData))
			{
				return;
			}
			GameObject parentGameObject = new GameObject("ParentObject");
			ParentObject instance = parentGameObject.AddComponent<ParentObject>();
			instance.Init(noodleData, _leftHanded, _parentObjects);
			if (_v2)
			{
				instance.ApplyV2Transform(noodleData);
				return;
			}
			instance.enabled = false;
			noodleData.TransformData.Apply(instance.transform, _leftHanded);
			_transformControllerFactory.Create(parentGameObject, noodleData.ParentTrack, false);
		}

		private readonly bool _v2;

		private readonly EditorDeserializedData _deserializedData;

		private readonly bool _leftHanded;

		private readonly TransformControllerFactory _transformControllerFactory;

		private readonly HashSet<ParentObject> _parentObjects = new HashSet<ParentObject>();
	}
}
