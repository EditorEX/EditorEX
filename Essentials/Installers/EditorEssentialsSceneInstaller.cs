using BeatmapEditor3D;
using BetterEditor.Essentials.Movement;
using BetterEditor.Essentials.Movement.Data;
using BetterEditor.Essentials.Movement.Note.MovementProvider;
using BetterEditor.Essentials.Movement.Obstacle.MovementProvider;
using BetterEditor.Essentials.Patches;
using BetterEditor.Essentials.SpawnProcessing;
using BetterEditor.Essentials.ViewMode;
using BetterEditor.Heck.Patches;
using BetterEditor.UI.SideBar;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BetterEditor.Essentials.Installers
{
	public class EditorEssentialsSceneInstaller : Installer
	{
		public override void InstallBindings()
		{
			Plugin.Log.Info("Installing EditorEssentialsSceneInstaller");
			// Clean this up!!!
			Container.Bind<VersionContext>().FromInstance(new(DeserializationPatch.beatmapVersion)).AsSingle();

			Container.Bind<ActiveViewMode>().AsSingle();

			//TODO: Improve this
			var objectsView = Resources.FindObjectsOfTypeAll<BeatmapObjectsView>().FirstOrDefault();
			Container.Bind<BeatmapObjectsView>().FromInstance(objectsView).AsSingle();

			Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] {"Normal" }, typeof(EditorNoteBasicMovement)));
			Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] {"Preview" }, typeof(EditorNoteGameMovement)));

			Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Normal" }, typeof(EditorObstacleBasicMovement)));
			Container.Bind<ValueTuple<string[], Type>>().WithId("Movement").FromInstance((new string[] { "Preview" }, typeof(EditorObstacleGameMovement)));
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
