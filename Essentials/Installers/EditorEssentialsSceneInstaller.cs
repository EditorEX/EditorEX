using BeatmapEditor3D;
using EditorEX.Essentials.Movement;
using EditorEX.Essentials.Movement.Arc.MovementProvider;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Movement.Note.MovementProvider;
using EditorEX.Essentials.Movement.Obstacle.MovementProvider;
using EditorEX.Essentials.Patches;
using EditorEX.Essentials.Patches.Movement;
using EditorEX.Essentials.Patches.Preview;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.Essentials.ViewMode;
using EditorEX.Essentials.Visuals;
using EditorEX.Essentials.Visuals.Note;
using EditorEX.Essentials.Visuals.Obstacle;
using EditorEX.Essentials.Visuals.Universal;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using EditorEX.Essentials.VariableMovement;
using UnityEngine;
using Zenject;
using EditorEX.Essentials.Features.HideUI;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsSceneInstaller : Installer
    {
        [Inject]
        private PopulateBeatmap populateBeatmap;

        public override void InstallBindings()
        {
            Container.BindInstance(populateBeatmap).AsSingle().NonLazy();

            Container.Bind<ActiveViewMode>().AsSingle();

            //TODO: Improve this
            var objectsView = Resources.FindObjectsOfTypeAll<BeatmapObjectsView>().FirstOrDefault();
            Container.Bind<BeatmapObjectsView>().FromInstance(objectsView).AsSingle();

            Container.BindInterfacesAndSelfTo<InstallMovement>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InitMovement>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DisableMovement>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ArcPreview>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PreviewSelectionPatches>().AsSingle().NonLazy();

            // MOVEMENT

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((["normal"], typeof(EditorNoteBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((["preview", "preview-lock-cam"], typeof(EditorNoteGameMovement)));

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((["normal"], typeof(EditorObstacleBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((["preview", "preview-lock-cam"], typeof(EditorObstacleGameMovement)));

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((["normal"], typeof(EditorArcBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((["preview", "preview-lock-cam"], typeof(EditorArcGameMovement)));

            Container.Bind<MovementTypeProvider>().AsSingle();

            // VARIABLE MOVEMENT

            Container.Bind<ValueTuple<string, Type>>().WithId("VariableMovement").FromInstance(("Variable", typeof(VariableMovementDataProvider)));
            Container.Bind<ValueTuple<string, Type>>().WithId("VariableMovement").FromInstance(("Noodle", typeof(EditorNoodleMovementDataProvider)));

            Container.Bind<VariableMovementTypeProvider>().AsSingle();

            Container
                .BindMemoryPool<EditorNoodleMovementDataProvider, EditorNoodleMovementDataProvider.Pool>()
                .WithInitialSize(40);

            // VISUALS

            Container.Bind<VisualsTypeProvider>().AsSingle().NonLazy();
            Container.Bind<VisualAssetProvider>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<ValueTuple<string[], Type>>().WithId("Visuals").FromInstance((["normal"], typeof(EditorNoteBasicVisuals)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Visuals").FromInstance((["preview", "preview-lock-cam"], typeof(EditorNoteGameVisuals)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Visuals").FromInstance((["preview", "preview-lock-cam"], typeof(EditorBombGameVisuals)));

            Container.Bind<ValueTuple<string[], Type>>().WithId("Visuals").FromInstance((["normal"], typeof(EditorObstacleBasicVisuals)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Visuals").FromInstance((["preview", "preview-lock-cam"], typeof(EditorObstacleGameVisuals)));



            Container.Bind<EditorBeatmapObjectsInTimeRowProcessor>().AsSingle();

            Container.BindInterfacesAndSelfTo<FixAudioTimeSource>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProcessNewEditorData>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PreviewToggler>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CameraLock>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ViewModeSwappingUI>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ViewModeSwapper>().AsSingle().NonLazy();

            Container.Bind<EditorBasicBeatmapObjectSpawnMovementData>().AsSingle().NonLazy();
            Container.Bind<VariableMovementInitializer>().AsSingle().NonLazy();
        }
    }
}
