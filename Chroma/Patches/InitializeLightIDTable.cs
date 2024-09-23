using BeatmapEditor3D;
using Chroma.Lighting;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterEditor.Chroma.Patches
{
	[HarmonyPatch(typeof(BeatmapEditorLevelSceneTransitionSetupDataSO), nameof(BeatmapEditorLevelSceneTransitionSetupDataSO.Init))]
	public static class InitializeLightIDTable
	{
		[HarmonyPostfix]
		public static void Postfix(EnvironmentInfoSO environmentInfo)
		{
			LightIDTableManager.SetEnvironment(environmentInfo.serializedName);
		}
	}
}
