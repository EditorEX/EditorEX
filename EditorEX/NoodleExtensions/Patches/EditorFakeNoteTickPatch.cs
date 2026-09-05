using BeatmapEditor3D.Controller;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.Heck.Deserialize;
using NoodleExtensions;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.NoodleExtensions.Patches
{
    internal class EditorFakeNoteTickPatch : IAffinity
    {
        private readonly EditorDeserializedData? _noodle;

        private EditorFakeNoteTickPatch(
            [InjectOptional(Id = NoodleController.ID)] EditorDeserializedData? deserializedData
        )
        {
            _noodle = deserializedData;
        }

        [AffinityPatch(
            typeof(EditorAudioFeedbackController),
            nameof(EditorAudioFeedbackController.HandlePlayHeadPositionChanged)
        )]
        [AffinityPrefix]
        private bool Prefix(EditorAudioFeedbackController __instance, int currentSample)
        {
            if (
                __instance._soundIsTemporaryDisabled
                || __instance._beatmapState.editingMode != BeatmapEditingMode.Objects
                || __instance._beatmapEditorSettingsDataModel.zenMode
                || !__instance._beatmapState.isPlaying
            )
            {
                return false;
            }

            float beat = __instance._audioDataModel.bpmData.SampleToBeat(currentSample);
            bool play = false;
            while (
                __instance._currentFrameId >= 0
                && __instance._currentFrameId
                    < __instance._beatmapObjectsDataModel.beatmapObjectsFrames.Count
                && beat
                    > __instance
                        ._beatmapObjectsDataModel
                        .beatmapObjectsFrames[__instance._currentFrameId]
                        .beat
            )
            {
                BeatmapObjectsFrameDataContainer dataContainer = __instance
                    ._beatmapObjectsDataModel
                    .beatmapObjectsFrames[__instance._currentFrameId]
                    .dataContainer;
                play |= EditorFakeNoteTick.FrameHasTickableNote(dataContainer, _noodle);
                __instance._currentFrameId++;
            }

            if (play)
            {
                __instance._audioSource.PlayOneShot(__instance._notePassedFeedback);
            }

            return false;
        }
    }
}
