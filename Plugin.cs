using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Analyzer.Installers;
using EditorEX.Chroma.Deserializer;
using EditorEX.Chroma.Installers;
using EditorEX.CustomJSONData.Patches;
using EditorEX.Essentials.Installers;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.Installers;
using EditorEX.NoodleExtensions.Deserialize;
using EditorEX.NoodleExtensions.Installers;
using EditorEX.UI.SideBar;
using HarmonyLib;
using IPA;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace EditorEX
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;

            //zenjector.Install<EditorEssentialsModelsInstaller, BeatmapEditorDataModelsInstaller>();
            zenjector.Install<EditorCustomJSONDataModelsInstaller, BeatmapEditorDataModelsInstaller>();
            //zenjector.Install<EditorHeckModelsInstaller, BeatmapEditorDataModelsInstaller>();

            //zenjector.Install<EditorHeckSceneInstaller, BeatmapLevelEditorSceneSetup>();
            //zenjector.Install<EditorNoodleSceneInstaller, BeatmapLevelEditorSceneSetup>();
            //zenjector.Install<EditorEssentialsSceneInstaller, BeatmapLevelEditorSceneSetup>();
            //zenjector.Install<EditorChromaSceneInstaller, BeatmapLevelEditorSceneSetup>();

            //zenjector.Install<EditorAnalyzerSceneInstaller, BeatmapLevelEditorInstaller>();

            //zenjector.Install<EditorChromaMainInstaller, BeatmapEditorMainInstaller>();

            //EditorDeserializerManager.Register<EditorNoodleCustomDataDeserializer>("NoodleExtensions").Enabled = true;
            //EditorDeserializerManager.Register<EditorNoodleCustomDataDeserializer>("Heck").Enabled = true;
            //EditorDeserializerManager.Register<EditorChromaCustomDataManager>("Chroma").Enabled = true;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            SideBarUI.RegisterButton(BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("EditorEX.UI.Resources.lockcamera.png"), "Lock Camera", (x) =>
            {

            });
        }
    }
}
