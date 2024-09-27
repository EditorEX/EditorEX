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
using EditorEX.Heck.Patches;
using EditorEX.UI.SideBar;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<ActiveViewMode>().AsSingle();

            //TODO: Improve this
            var objectsView = Resources.FindObjectsOfTypeAll<BeatmapObjectsView>().FirstOrDefault();
            Container.Bind<BeatmapObjectsView>().FromInstance(objectsView).AsSingle();

            Container.BindInterfacesAndSelfTo<InstallMovement>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InitMovement>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DisableMovement>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PopulateBeatmap>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ArcPreview>().AsSingle().NonLazy();

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "normal" }, typeof(EditorNoteBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "preview", "preview-lock-cam" }, typeof(EditorNoteGameMovement)));

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "normal" }, typeof(EditorObstacleBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "preview", "preview-lock-cam" }, typeof(EditorObstacleGameMovement)));

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "normal" }, typeof(EditorArcBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "preview", "preview-lock-cam" }, typeof(EditorArcGameMovement)));

            Container.Bind<MovementTypeProvider>().AsSingle();

            Container.Bind<EditorBeatmapObjectsInTimeRowProcessor>().AsSingle();

            Container.BindInterfacesAndSelfTo<ProcessNewEditorData>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PreviewToggler>().AsSingle().NonLazy();

            Container.Bind<SideBarUI>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ViewModeSwappingUI>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ViewModeSwapper>().AsSingle().NonLazy();

            Container.Bind<EditorBasicBeatmapObjectSpawnMovementData>().AsSingle().NonLazy();
        }
    }
}
