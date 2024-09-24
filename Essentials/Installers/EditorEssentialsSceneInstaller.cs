using BeatmapEditor3D;
using EditorEX.Essentials.Movement;
using EditorEX.Essentials.Movement.Arc.MovementProvider;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Movement.Note.MovementProvider;
using EditorEX.Essentials.Movement.Obstacle.MovementProvider;
using EditorEX.Essentials.Patches;
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
            // Clean this up!!!
            Container.Bind<VersionContext>().FromInstance(new(DeserializationPatch.beatmapVersion)).AsSingle();

            Container.Bind<ActiveViewMode>().AsSingle();

            //TODO: Improve this
            var objectsView = Resources.FindObjectsOfTypeAll<BeatmapObjectsView>().FirstOrDefault();
            Container.Bind<BeatmapObjectsView>().FromInstance(objectsView).AsSingle();

            Container.BindInterfacesAndSelfTo<ArcPreview>().AsSingle().NonLazy();

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Normal" }, typeof(EditorNoteBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Preview" }, typeof(EditorNoteGameMovement)));

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Normal" }, typeof(EditorObstacleBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Preview" }, typeof(EditorObstacleGameMovement)));

            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Normal" }, typeof(EditorArcBasicMovement)));
            Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Preview" }, typeof(EditorArcGameMovement)));

            Container.Bind<MovementTypeProvider>().AsSingle();

            Container.Bind<EditorBeatmapObjectsInTimeRowProcessor>().AsSingle();

            Container.BindInterfacesAndSelfTo<ProcessNewEditorData>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PreviewToggler>().AsSingle().NonLazy();

            Container.Bind<SideBarUI>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ViewModeSwappingUI>().AsSingle().NonLazy();

            Container.Bind<EditorBasicBeatmapObjectSpawnMovementData>().AsSingle().NonLazy();
        }
    }
}
