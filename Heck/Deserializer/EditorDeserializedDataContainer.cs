﻿using Heck.Animation;
using System.Collections.Generic;

namespace BetterEditor.Heck.Deserializer
{
	// Used until we can install it in EditorSceneInstaller
	internal static class EditorDeserializedDataContainer
	{
		public static bool Ready { get; internal set; }

		public static Dictionary<string, Track> Tracks;

		public static Dictionary<object, EditorDeserializedData> DeserializeDatas;

		public static EditorDeserializedData GetDeserializedData(object key)
		{
			if (DeserializeDatas.TryGetValue(key, out EditorDeserializedData data))
			{
				return data;
			}
			return null;
		}
	}
}