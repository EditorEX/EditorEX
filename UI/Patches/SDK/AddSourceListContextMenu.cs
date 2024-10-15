using EditorEX.UI.Components;
using HMUI;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Patches.SDK
{
    // Adds context menu to the beatmap list sources switcher 
    internal class AddSourceListContextMenu : IAffinity
    {
        private readonly IInstantiator _instantiator;

        private AddSourceListContextMenu(
            IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        [AffinityPatch(typeof(TextSegmentedControl), nameof(TextSegmentedControl.CellForCellNumber))]
        [AffinityPostfix]
        private void SetData(TextSegmentedControl __instance, int cellNumber, SegmentedControlCell __result)
        {
            if (__instance.gameObject.name != "SourcesSegmentedControl")
            {
                return;
            }
            SourceListContextMenu contextMenu = __result.gameObject.GetComponent<SourceListContextMenu>();
            if (cellNumber == __instance.NumberOfCells() - 1)
            {
                if (contextMenu != null)
                {
                    GameObject.Destroy(contextMenu);
                }
                return;
            }
            if (__result.gameObject.GetComponent<SourceListContextMenu>() == null)
            {
                contextMenu = _instantiator.InstantiateComponent<SourceListContextMenu>(__result.gameObject);
            }
            contextMenu.SetData(__instance._texts[cellNumber]);
        }
    }
}
