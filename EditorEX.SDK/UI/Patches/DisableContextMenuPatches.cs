using EditorEX.SDK.Components;
using SiraUtil.Affinity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.SDK.UI.Patches
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
            var modal = _contextMenuComponent.Modal;
            if (modal == null || !modal.IsPushed)
            {
                return;
            }

            if (handler is not Component component)
            {
                return;
            }

            if (!component.transform.IsChildOf(modal.ViewTransform))
            {
                modal.IsPushed = false;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(DevicelessVRHelper),
            nameof(DevicelessVRHelper.GetAnyJoystickMaxAxis)
        )]
        private void PatchScoll(Vector2 __result)
        {
            var modal = _contextMenuComponent.Modal;
            if (__result.magnitude > 0.001f)
            {
                if (modal != null)
                {
                    modal.IsPushed = false;
                }
            }
        }
    }
}
