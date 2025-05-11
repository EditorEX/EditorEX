using BeatmapEditor3D;
using EditorEX.Essentials.Movement.Arc;
using EditorEX.Essentials.Movement.Arc.MovementProvider;
using EditorEX.Essentials.Movement.ChainHead;
using EditorEX.Essentials.Movement.ChainHead.MovementProvider;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Movement.Note.MovementProvider;
using EditorEX.Essentials.Movement.Obstacle;
using EditorEX.Essentials.Movement.Obstacle.MovementProvider;
using EditorEX.Essentials.VariableMovement;
using EditorEX.Essentials.Visuals.Note;
using EditorEX.Essentials.Visuals.Obstacle;
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
            __instance._normalNoteViewPrefab.gameObject.AddComponent<VariableMovementHolder>();

            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteGameMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorBombGameVisuals>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicVisuals>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteJump>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteFloorMovement>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<EditorNoteController>();
            __instance._bombNoteViewPrefab.gameObject.AddComponent<VariableMovementHolder>();

            __instance._chainNoteViewPrefab.gameObject.AddComponent<EditorChainHeadBasicMovement>();
            __instance._chainNoteViewPrefab.gameObject.AddComponent<EditorChainHeadGameMovement>();
            //__instance._chainNoteViewPrefab.gameObject.AddComponent<EditorNoteGameVisuals>();
            //__instance._chainNoteViewPrefab.gameObject.AddComponent<EditorNoteBasicVisuals>();
            //__instance._chainNoteViewPrefab.gameObject.AddComponent<EditorNoteJump>();
            //__instance._chainNoteViewPrefab.gameObject.AddComponent<EditorNoteFloorMovement>();
            __instance._chainNoteViewPrefab.gameObject.AddComponent<EditorChainHeadController>();
            __instance._chainNoteViewPrefab.gameObject.AddComponent<VariableMovementHolder>();

            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleBasicMovement>();
            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleGameMovement>();
            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleBasicVisuals>();
            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleGameVisuals>();
            __instance._obstacleViewPrefab.gameObject.AddComponent<EditorObstacleController>();
            __instance._obstacleViewPrefab.gameObject.AddComponent<VariableMovementHolder>();

            __instance._arcViewPrefab.gameObject.AddComponent<EditorArcBasicMovement>();
            __instance._arcViewPrefab.gameObject.AddComponent<EditorArcGameMovement>();
            __instance._arcViewPrefab.gameObject.AddComponent<EditorArcController>();
            __instance._arcViewPrefab.gameObject.AddComponent<EditorSliderIntensityEffect>();
            __instance._arcViewPrefab.gameObject.AddComponent<VariableMovementHolder>();
        }
    }
}
