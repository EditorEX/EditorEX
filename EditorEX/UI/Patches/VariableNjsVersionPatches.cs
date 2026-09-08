using BeatmapEditor3D.Commands;
using BeatmapEditor3D.Views;
using EditorEX.MapData.Contexts;
using SiraUtil.Affinity;
using UnityEngine.UI;

namespace EditorEX.UI.Patches
{
    internal class VariableNjsVersionPatches : IAffinity
    {
        [AffinityPatch(typeof(StatusBarView), nameof(StatusBarView.SetStatusViewState))]
        [AffinityPostfix]
        private void PostfixSetStatusViewState(StatusBarView __instance)
        {
            ApplyStatusBarToggle(__instance);
        }

        [AffinityPatch(typeof(StatusBarView), nameof(StatusBarView.DidActivate))]
        [AffinityPostfix]
        private void PostfixStatusBarDidActivate(StatusBarView __instance)
        {
            ApplyStatusBarToggle(__instance);
            if (
                VariableNjsSupport.IsAvailable(MapContext.Version)
                || !__instance._beatmapState.variableNjsEditingEnabled
            )
            {
                return;
            }

            __instance._signalBus.Fire(new ChangeVariableNjsEditingSignal(false));
        }

        [AffinityPatch(
            typeof(NoteJumpSpeedToolbarView),
            nameof(NoteJumpSpeedToolbarView.SetValues)
        )]
        [AffinityPostfix]
        private void PostfixToolbarSetValues(NoteJumpSpeedToolbarView __instance)
        {
            if (VariableNjsSupport.IsAvailable(MapContext.Version))
            {
                return;
            }

            SetButtonInteractable(__instance._njs0Button, false);
            SetButtonInteractable(__instance._njs2Button, false);
            SetButtonInteractable(__instance._njs4Button, false);
            SetButtonInteractable(__instance._njsNeg2Button, false);
            SetButtonInteractable(__instance._njsNeg4Button, false);
            SetButtonInteractable(__instance._noneButton, false);
            SetButtonInteractable(__instance._linearButton, false);
            SetButtonInteractable(__instance._inQuadButton, false);
            SetButtonInteractable(__instance._outQuadButton, false);
            if (__instance._extensionToggle != null)
            {
                __instance._extensionToggle.interactable = false;
            }
        }

        [AffinityPatch(
            typeof(ChangeVariableNjsEditingCommand),
            nameof(ChangeVariableNjsEditingCommand.Execute)
        )]
        [AffinityPrefix]
        private bool PrefixChangeVariableNjsEditing(ChangeVariableNjsEditingCommand __instance)
        {
            return !__instance._signal.enable || VariableNjsSupport.IsAvailable(MapContext.Version);
        }

        [AffinityPatch(
            typeof(PlaceNoteJumpSpeedEventCommand),
            nameof(PlaceNoteJumpSpeedEventCommand.Execute)
        )]
        [AffinityPrefix]
        private bool PrefixPlaceNoteJumpSpeedEvent()
        {
            return VariableNjsSupport.IsAvailable(MapContext.Version);
        }

        private static void ApplyStatusBarToggle(StatusBarView view)
        {
            Toggle toggle = view._variableNjsToggle;
            if (toggle == null)
            {
                return;
            }

            bool available = VariableNjsSupport.IsAvailable(MapContext.Version);
            toggle.interactable = available;
            if (!available)
            {
                toggle.gameObject.SetActive(false);
            }
        }

        private static void SetButtonInteractable(Button? button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}
