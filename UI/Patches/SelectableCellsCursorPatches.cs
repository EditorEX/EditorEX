using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMUI;
using SiraUtil.Affinity;
using UnityEngine.EventSystems;

namespace EditorEX.UI.Patches
{
    public class SelectableCellsCursorPatches : IAffinity
    {
        public static List<SelectableCell> SelectableCells { get; } = new List<SelectableCell>();
        [AffinityPatch(typeof(UIBehaviour), nameof(UIBehaviour.OnEnable))]
        [AffinityPostfix]
        private void OnEnable(UIBehaviour __instance)
        {
            if (__instance is not SelectableCell selectableCell) return;
            if (SelectableCells.Contains(selectableCell)) return;
            SelectableCells.Add(selectableCell);
        }

        [AffinityPatch(typeof(UIBehaviour), nameof(UIBehaviour.OnDisable))]
        [AffinityPostfix]
        private void OnDisable(UIBehaviour __instance)
        {
            if (__instance is not SelectableCell selectableCell) return;
            if (!SelectableCells.Contains(selectableCell)) return;
            SelectableCells.Remove(selectableCell);
        }
    }
}