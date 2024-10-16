using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Chroma.Deserializer;
using EditorEX.Chroma.Installers;
using EditorEX.Config;
using EditorEX.Config.Installers;
using EditorEX.CustomJSONData.Installers;
using EditorEX.Essentials.Installers;
using EditorEX.Essentials.ViewMode;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.Installers;
using EditorEX.NoodleExtensions.Deserialize;
using EditorEX.NoodleExtensions.Installers;
using EditorEX.UI.Installers;
using IPA;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace EditorEX
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, IPA.Config.Config conf)
        {
            MainConfig config = conf.Generated<MainConfig>();
            zenjector.UseLogger(logger);
            zenjector.UseMetadataBinder<Plugin>();

            zenjector.Install<EditorEssentialsModelsInstaller, BeatmapEditorDataModelsInstaller>();
            zenjector.Install<EditorCustomJSONDataModelsInstaller, BeatmapEditorDataModelsInstaller>();
            zenjector.Install<EditorHeckModelsInstaller, BeatmapEditorDataModelsInstaller>();

            zenjector.Install<EditorUIViewControllersInstaller, BeatmapEditorViewControllersInstaller>();

            zenjector.Install<EditorHeckSceneInstaller, BeatmapLevelEditorSceneSetup>();
            zenjector.Install<EditorNoodleSceneInstaller, BeatmapLevelEditorSceneSetup>();
            zenjector.Install<EditorEssentialsSceneInstaller, BeatmapLevelEditorSceneSetup>();
            zenjector.Install<EditorChromaSceneInstaller, BeatmapLevelEditorSceneSetup>();
            //zenjector.Install<EditorAnalyzerSceneInstaller, BeatmapLevelEditorInstaller>();

            zenjector.Install<EditorChromaMainInstaller, BeatmapEditorMainInstaller>();

            zenjector.Install<EditorCustomJSONDataAppInstaller>(Location.App);
            zenjector.Install<EditorEssentialsAppInstaller>(Location.App);
            zenjector.Install<EditorConfigAppInstaller>(Location.App, config);

            EditorDeserializerManager.Register<EditorNoodleCustomDataDeserializer>("NoodleExtensions").Enabled = true;
            EditorDeserializerManager.Register<EditorHeckCustomDataDeserializer>("Heck").Enabled = true;
            EditorDeserializerManager.Register<EditorChromaCustomDataDeserializer>("Chroma").Enabled = true;

            ViewModeRepository.RegisterViewMode(new ViewMode("Normal", "normal", false, true, false));
            ViewModeRepository.RegisterViewMode(new ViewMode("Preview", "preview", true, false, false));
            ViewModeRepository.RegisterViewMode(new ViewMode("Preview w/ Camlock", "preview-lock-cam", true, false, true));
        }
    }
}