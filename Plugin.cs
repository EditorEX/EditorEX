using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BetterEditor.Chroma.Deserializer;
using BetterEditor.Chroma.Installers;
using BetterEditor.Essentials.Installers;
using BetterEditor.Heck.Deserializer;
using BetterEditor.Heck.Installers;
using BetterEditor.NoodleExtensions.Deserializer;
using BetterEditor.NoodleExtensions.Installers;
using BetterEditor.UI.SideBar;
using HarmonyLib;
using IPA;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace BetterEditor
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static Harmony Harmony { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Harmony = new Harmony("futuremapper.BetterEditor");

            zenjector.Install<EditorEssentialsModelsInstaller, BeatmapEditorDataModelsInstaller>();
            zenjector.Install<EditorCustomJSONDataModelsInstaller, BeatmapEditorDataModelsInstaller>();
            zenjector.Install<EditorHeckModelsInstaller, BeatmapEditorDataModelsInstaller>();

            zenjector.Install<EditorHeckSceneInstaller, BeatmapLevelEditorSceneSetup>();
            zenjector.Install<EditorNoodleSceneInstaller, BeatmapLevelEditorSceneSetup>();
            zenjector.Install<EditorEssentialsSceneInstaller, BeatmapLevelEditorSceneSetup>();
            zenjector.Install<EditorChromaSceneInstaller, BeatmapLevelEditorSceneSetup>();

            zenjector.Install<EditorChromaMainInstaller, BeatmapEditorMainInstaller>();


            EditorDeserializerManager.Register<EditorNoodleCustomDataManager>("NoodleExtensions").Enabled = true;
            EditorDeserializerManager.Register<EditorHeckCustomDataManager>("Heck").Enabled = true;
            EditorDeserializerManager.Register<EditorChromaCustomDataManager>("Chroma").Enabled = true;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            SideBarUI.RegisterButton(BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("BetterEditor.UI.Resources.lockcamera.png"), "Lock Camera", (x) =>
            {

            });
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Harmony.UnpatchSelf();
        }
    }
}
