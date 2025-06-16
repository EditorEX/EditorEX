using EditorEX.UI.ContextMenu;
using SiraUtil.Affinity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.UI.Patches.SDK
{
    // If we click anything that is not a child of the modal or if the we scroll, we should close the modal.
    internal class DisableContextMenuPatches : IAffinity
    {
        private readonly ContextMenuComponent _contextMenuComponent;

        private DisableContextMenuPatches(ContextMenuComponent contextMenuComponent)
        {
            _contextMenuComponent = contextMenuComponent;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(ExecuteEvents),
            nameof(ExecuteEvents.Execute),
            AffinityMethodType.Normal,
            null,
            typeof(IPointerClickHandler),
            typeof(BaseEventData)
        )]
        private void PatchExecute(IPointerClickHandler handler, BaseEventData eventData)
        {
            if (
                _contextMenuComponent.modal.isShown
                && handler is Component component
                && !component.transform.IsChildOf(_contextMenuComponent.modal.transform)
            )
            {
                _contextMenuComponent.modal.Hide();
            }
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(DevicelessVRHelper),
            nameof(DevicelessVRHelper.GetAnyJoystickMaxAxis)
        )]
        private void PatchScoll(Vector2 __result)
        {
            if (__result.magnitude > 0.001f)
            {
                _contextMenuComponent.modal.Hide();
            }
        }
    }
}
