using BeatmapEditor3D;
using BetterEditor.Essentials.Movement.Arc;
using BetterEditor.Essentials.Movement.Arc.MovementProvider;
using BetterEditor.Essentials.Movement.Note;
using BetterEditor.Essentials.Movement.Note.MovementProvider;
using BetterEditor.Essentials.Movement.Obstacle;
using BetterEditor.Essentials.Movement.Obstacle.MovementProvider;
using HarmonyLib;

namespace BetterEditor.Essentials.Patches.Movement
{
    [HarmonyPatch(typeof(BeatmapLevelEditorInstaller), nameof(BeatmapLevelEditorInstaller.InstallBindings))]
    public static class InstallMovement
    {
        [HarmonyPrefix]
        public static void Prefix(BeatmapLevelEditorInstaller __instance)
        {
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicMovement>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteGameMovement>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteJump>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteFloorMovement>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteController>();

            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteGameMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteJump>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteFloorMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteController>();

            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleBasicMovement>();
            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleGameMovement>();
            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleController>();

            __instance._arcViewPrefab.gameObject.AddComponent<EditorArcBasicMovement>();
            __instance._arcViewPrefab.gameObject.AddComponent<EditorArcGameMovement>();
            __instance._arcViewPrefab.gameObject.AddComponent<EditorArcController>();
            __instance._arcViewPrefab.gameObject.AddComponent<EditorSliderIntensityEffect>();
        }
    }
}
