using BeatmapEditor3D;
using BetterEditor.Heck.Events;
using BetterEditor.NoodleExtensions.Manager;
using NoodleExtensions.Animation;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BetterEditor.NoodleExtensions.Installers
{
    public class EditorNoodleSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<AnimationHelper>().AsSingle();

            Container.BindInterfacesTo<EditorAssignTrackParent>().AsSingle();

            Container.BindInterfacesAndSelfTo<EditorSpawnDataManager>().AsSingle();
        }
    }
}
