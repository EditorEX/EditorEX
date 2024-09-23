using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using BetterEditor.Essentials.Movement.Note;
using BetterEditor.Essentials.Movement.Obstacle;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterEditor.Essentials.Patches.Movement
{
	[HarmonyPatch]
	public static class InitMovement
	{
		private static readonly MethodInfo _init = AccessTools.Method(typeof(InitMovement), "Init", null, null);
		private static readonly MethodInfo _initObstacle = AccessTools.Method(typeof(InitMovement), "InitObstacle", null, null);
		private static readonly MethodInfo _initArc = AccessTools.Method(typeof(InitMovement), "InitArc", null, null);

		public static void Init(GameObject gameObject, BaseEditorData editorData)
		{
			if (gameObject.GetComponent<EditorNoteController>() == null) return;
			gameObject.GetComponent<EditorNoteController>().Init(editorData as NoteEditorData);
		}

		public static void InitObstacle(ObstacleView obstacleView, BaseEditorData editorData)
		{
			if (obstacleView.gameObject.GetComponent<EditorObstacleController>() == null) return;
			obstacleView.gameObject.GetComponent<EditorObstacleController>().Init(editorData as ObstacleEditorData);
		}

		public static void InitArc(ArcView arcView, BaseEditorData editorData)
		{
			if (arcView.gameObject.GetComponent<EditorObstacleController>() == null) return;
			arcView.gameObject.GetComponent<EditorObstacleController>().Init(editorData as ObstacleEditorData);
		}

		[HarmonyPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TranspilerNote(IEnumerable<CodeInstruction> instructions)
		{
			var result = new CodeMatcher(instructions, null)
				.End().Advance(-1) // before ret
				.Insert(new CodeInstruction[]
				{
					new CodeInstruction(OpCodes.Ldloc_S, 5),
					new CodeInstruction(OpCodes.Ldarg_1),
					new CodeInstruction(OpCodes.Call, _init)
				}).InstructionEnumeration();
			return result;
		}

		[HarmonyPatch(typeof(ObstacleBeatmapObjectView), nameof(ObstacleBeatmapObjectView.InsertObject))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TranspilerObstacle(IEnumerable<CodeInstruction> instructions)
		{
			var result = new CodeMatcher(instructions, null)
				.End().Advance(-1) // before ret
				.Insert(new CodeInstruction[]
				{
					new CodeInstruction(OpCodes.Ldloc_2),
					new CodeInstruction(OpCodes.Ldarg_1),
					new CodeInstruction(OpCodes.Call, _initObstacle)
				}).InstructionEnumeration();
			return result;
		}

		[HarmonyPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.InsertObject))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TranspilerArc(IEnumerable<CodeInstruction> instructions)
		{
			var result = new CodeMatcher(instructions, null)
				.End().Advance(-1) // before ret
				.Insert(new CodeInstruction[]
				{
					new CodeInstruction(OpCodes.Ldloc_0),
					new CodeInstruction(OpCodes.Ldarg_1),
					new CodeInstruction(OpCodes.Call, _initArc)
				}).InstructionEnumeration();
			return result;
		}
	}
}
