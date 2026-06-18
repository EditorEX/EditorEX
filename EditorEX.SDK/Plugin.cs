using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.SDK.Installers;
using IPA;
using IPA.Loader;
using ModestTree;
using SiraUtil.Zenject;
using IpaLogger = IPA.Logging.Logger;

namespace EditorEX.SDK;

[Plugin(RuntimeOptions.DynamicInit)]
internal class Plugin
{
    [Init]
    public Plugin(IpaLogger logger, Zenjector zenjector, PluginMetadata pluginMetadata)
    {
        zenjector.UseLogger(logger);
        zenjector.UseMetadataBinder<Plugin>();

        zenjector.Install<EditorSDKAppInstaller>(Location.App);
        zenjector.Install<EditorSDKModelsInstaller, BeatmapEditorDataModelsInstaller>();
        zenjector.Install<
            EditorSDKViewControllersInstaller,
            BeatmapEditorViewControllersInstaller
        >();
    }
}
