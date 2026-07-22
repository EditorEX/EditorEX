using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomDataModels;
using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.UI.Patches.EditBeatmapLevel;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Affinity hooks for <see cref="EditBeatmapLevelViewController"/>. Kept thin — the
    /// actual layout composition and feature logic live in the helpers under
    /// <see cref="EditorEX.UI.Patches.EditBeatmapLevel"/>, constructed here from the same
    /// injected dependencies.
    /// </summary>
    internal class EditBeatmapLevelPatches : IAffinity
    {
        private readonly BeatmapLevelDataModel _beatmapLevelDataModel;
        private readonly AddressableSignalBus _addressableSignalBus;

        private readonly EditBeatmapLevelCoverController _coverController;
        private readonly EditBeatmapLevelCharacteristicUI _characteristicUI;
        private readonly EditBeatmapLevelCharacteristicTabs _characteristicTabs;
        private readonly EditBeatmapLevelSongInfoUI _songInfoUI;

        private EditBeatmapLevelPatches(
            BeatmapLevelDataModel beatmapLevelDataModel,
            ILevelCustomDataModel levelCustomDataModel,
            IInstantiator instantiator,
            AddressableSignalBus addressableSignalBus,
            LazyInject<BeatmapProjectManager> beatmapProjectManager,
            IColorCollector colorCollector,
            EnvironmentsListModel environmentsListModel,
            CustomPlatformsListModel customPlatformsListModel,
            IReactiveContainer reactiveContainer
        )
        {
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _addressableSignalBus = addressableSignalBus;

            _coverController = new EditBeatmapLevelCoverController(beatmapProjectManager);
            _characteristicTabs = new EditBeatmapLevelCharacteristicTabs(instantiator);
            _characteristicUI = new EditBeatmapLevelCharacteristicUI(
                beatmapLevelDataModel,
                levelCustomDataModel,
                beatmapProjectManager
            );
            _songInfoUI = new EditBeatmapLevelSongInfoUI(
                levelCustomDataModel,
                environmentsListModel,
                customPlatformsListModel,
                colorCollector,
                reactiveContainer,
                _coverController,
                _characteristicUI
            );
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(EditBeatmapLevelViewController),
            nameof(EditBeatmapLevelViewController.RefreshData)
        )]
        private void RefreshData(EditBeatmapLevelViewController __instance, bool clearModifiedState)
        {
            if (!__instance._beatmapLevelDataModel.isLoaded || !__instance._audioDataModel.isLoaded)
            {
                return;
            }

            _songInfoUI.RefreshData(clearModifiedState);

            // Same binding as EditBeatmapLevelViewController.RefreshData → CoverImageInputView.SetCoverImagePath
            _coverController.LoadCoverImage(
                _beatmapLevelDataModel.coverImageFilePath,
                triggerUpdate: false
            );

            //ReloadContributors();
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(EditBeatmapLevelViewController),
            nameof(EditBeatmapLevelViewController.HandleBeatmapProjectSaved)
        )]
        private void HandleBeatmapProjectSaved(EditBeatmapLevelViewController __instance)
        {
            _songInfoUI.HandleBeatmapProjectSaved();

            //ReloadContributors();
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(EditBeatmapLevelViewController),
            nameof(EditBeatmapLevelViewController.DidActivate)
        )]
        private void ModifyUI(EditBeatmapLevelViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                _characteristicTabs.AddCustomCharacteristicTab(__instance, "Lawless");
                _characteristicTabs.AddCustomCharacteristicTab(__instance, "Lightshow", true);

                _songInfoUI.BuildLayout(__instance);

                __instance.gameObject.SetActive(true);
            }
        }
    }
}
