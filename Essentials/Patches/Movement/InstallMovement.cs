using BeatmapEditor3D;
using EditorEX.Essentials.Movement.Arc;
using EditorEX.Essentials.Movement.Arc.MovementProvider;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Movement.Note.MovementProvider;
using EditorEX.Essentials.Movement.Obstacle;
using EditorEX.Essentials.Movement.Obstacle.MovementProvider;
using EditorEX.Essentials.Visuals;
using HarmonyLib;
using SiraUtil.Affinity;

namespace EditorEX.Essentials.Patches.Movement
{
    [AffinityPatch]
    public class InstallMovement : IAffinity
    {
        [AffinityPatch(typeof(BeatmapLevelEditorInstaller), nameof(BeatmapLevelEditorInstaller.InstallBindings))]
        [AffinityPrefix]
        public void Prefix(BeatmapLevelEditorInstaller __instance)
        {
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicMovement>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteGameMovement>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteGameVisuals>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicVisuals>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteJump>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteFloorMovement>();
            __instance._normalNoteViewPrefab.gameObject.AddComponent<EditorNoteController>();

            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteGameMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorBombGameVisuals>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicVisuals>();
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
