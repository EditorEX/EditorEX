using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using HarmonyLib;
using System.Collections.Generic;

namespace BetterEditor.Essentials.Patches
{
	[HarmonyPatch]
	public static class DisableMovement
	{
		[HarmonyPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.UpdateObjects))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TranspilerNote(IEnumerable<CodeInstruction> instructions)
		{
			var result = new CodeMatcher(instructions, null).Advance(17).RemoveInstructions(20).Advance(40).RemoveInstructions(20).InstructionEnumeration();
			return result;
		}

		[HarmonyPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TranspilerNoteInsert(IEnumerable<CodeInstruction> instructions)
		{
			var result = new CodeMatcher(instructions, null).Advance(153).RemoveInstructions(20).InstructionEnumeration();
			return result;
		}

		/*[HarmonyPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.UpdateObjects))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TranspilerArc(IEnumerable<CodeInstruction> instructions)
		{
			var result = new CodeMatcher(instructions, null).Advance(11).RemoveInstructions(17).InstructionEnumeration();
			return result;
		}*/

		[HarmonyPatch(typeof(ObstacleBeatmapObjectView), nameof(ObstacleBeatmapObjectView.UpdateObjects))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TranspilerObstacle(IEnumerable<CodeInstruction> instructions)
		{
			var result = new CodeMatcher(instructions, null).Advance(9).RemoveInstructions(19).InstructionEnumeration();
			foreach (var instruction in result)
			{
				Plugin.Log.Info(instruction.ToString());
			}
			return result;
		}
	}
}
