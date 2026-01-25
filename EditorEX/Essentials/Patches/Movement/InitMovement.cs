using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Movement.Arc;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Movement.Obstacle;
using HarmonyLib;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.Essentials.Patches.Movement
{
    [AffinityPatch]
    public class InitMovement : IAffinity
    {
        private static readonly MethodInfo _init = AccessTools.Method(typeof(InitMovement), "Init");
        private static readonly MethodInfo _initObstacle = AccessTools.Method(
            typeof(InitMovement),
            "InitObstacle"
        );
        private static readonly MethodInfo _initArc = AccessTools.Method(
            typeof(InitMovement),
            "InitArc"
        );

        public static void Init(GameObject gameObject, BaseEditorData editorData)
        {
            if (gameObject.GetComponent<EditorNoteController>() == null)
                return;
            gameObject.GetComponent<EditorNoteController>().Init(editorData as NoteEditorData);
        }

        public static void InitObstacle(ObstacleView obstacleView, BaseEditorData editorData)
        {
            if (obstacleView.gameObject.GetComponent<EditorObstacleController>() == null)
                return;
            obstacleView
                .gameObject.GetComponent<EditorObstacleController>()
                .Init(editorData as ObstacleEditorData);
        }

        public static void InitArc(ArcView arcView, BaseEditorData editorData)
        {
            if (arcView.gameObject.GetComponent<EditorArcController>() == null)
                return;
            arcView
                .gameObject.GetComponent<EditorArcController>()
                .Init(editorData as ArcEditorData);
        }

        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerNote(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var result = new CodeMatcher(instructions)
                .End()
                .Advance(-1) // before ret
                .Insert(new(OpCodes.Ldloc_S, 5), new(OpCodes.Ldarg_1), new(OpCodes.Call, _init))
                .InstructionEnumeration();
            return result;
        }

        [AffinityPatch(
            typeof(ObstacleBeatmapObjectView),
            nameof(ObstacleBeatmapObjectView.InsertObject)
        )]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerObstacle(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var result = new CodeMatcher(instructions)
                .End()
                .Advance(-1) // before ret
                .Insert(
                    new(OpCodes.Ldloc_2),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Call, _initObstacle)
                )
                .InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerArc(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var result = new CodeMatcher(instructions)
                .End()
                .Advance(-1) // before ret
                .Insert(new(OpCodes.Ldloc_0), new(OpCodes.Ldarg_1), new(OpCodes.Call, _initArc))
                .InstructionEnumeration();
            return result;
        }
    }
}
