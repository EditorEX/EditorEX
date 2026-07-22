using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using BeatSaberMarkupLanguage;
using HMUI;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Patches.EditBeatmapLevel
{
    /// <summary>
    /// Clones the <see cref="DifficultyBeatmapSetTabButton"/> prefab to add tabs for
    /// custom characteristics registered by SongCore (Lawless / Lightshow).
    /// </summary>
    internal class EditBeatmapLevelCharacteristicTabs
    {
        private readonly IInstantiator _instantiator;

        public EditBeatmapLevelCharacteristicTabs(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        private static BeatmapCharacteristicSO? GetCustomCharacteristic(string serializedName)
        {
            try
            {
                foreach (var so in SongCore.Collections.customCharacteristics)
                {
                    if (so != null && so.serializedName == serializedName)
                        return so;
                }
            }
            catch
            {
                // SongCore not present / not initialised — feature degrades gracefully.
            }
            return null;
        }

        public void AddCustomCharacteristicTab(
            EditBeatmapLevelViewController controller,
            string serializedName,
            bool end = false
        )
        {
            var so = GetCustomCharacteristic(serializedName);
            if (so == null)
                return;

            var existing = controller._difficultyBeatmapSetTabButtons;
            if (existing == null || existing.Length == 0)
                return;
            if (existing.Any(b => b != null && b.beatmapCharacteristic == so))
                return;

            var template = end ? existing[4] : existing[1];
            // Instantiate through Zenject (NOT Object.Instantiate) so the button's
            // [Inject] dependencies are populated on the clone. The tab's
            // SelectableStateController._tweeningManager is injected; without injection it
            // is null and the hover state transition (ColorGraphicStateTransition.StartTween)
            // throws an NRE.
            var clone = _instantiator.InstantiatePrefab(
                template.gameObject,
                template.transform.parent
            );
            clone.name = "DifficultyBeatmapSetTabButton_" + serializedName;
            var cloneButton = clone.GetComponent<DifficultyBeatmapSetTabButton>();
            if (cloneButton == null)
            {
                Object.Destroy(clone);
                return;
            }
            cloneButton._beatmapCharacteristic = so;

            var label = clone.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (label != null)
                label.text = serializedName;

            var list = existing.ToList();
            list.Add(cloneButton);
            controller._difficultyBeatmapSetTabButtons = list.ToArray();
            if (end)
            {
                existing[4]
                    .transform.Find("Background8px/Background")
                    .GetComponent<ImageView>()
                    .SetImageAsync("#WhitePixel", false);
            }
        }
    }
}
