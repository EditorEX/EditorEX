using BeatmapEditor3D.InputSystem;
using SiraUtil.Affinity;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Zenject;

namespace EditorEX.SDKImplementation.Patches
{
    public class FixUnityExplorerInputError : IAffinity
    {
        [AffinityPatch(typeof(InputReceiver), "IsPointerOverUI")]
        [AffinityPrefix]
        private bool FixInputModuleOverride()
        {
            if (EventSystem.current == null || EventSystem.current.currentInputModule == null)
            {
                return true;
            }
            return EventSystem.current.currentInputModule is InputSystemUIInputModule;
        }
    }
}