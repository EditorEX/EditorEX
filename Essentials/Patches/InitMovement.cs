using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using BetterEditor.Essentials.Movement.Note;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterEditor.Essentials.Patches
{
	[HarmonyPatch]
	public static class InitMovement
	{
		private static readonly MethodInfo _init = AccessTools.Method(typeof(InitMovement), "Init", null, null);

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

		public static void Init(GameObject gameObject, NoteEditorData editorData)
		{
			if (gameObject.GetComponent<EditorNoteController>() == null) return;
			gameObject.GetComponent<EditorNoteController>().Init(editorData);
		}
	}
}
