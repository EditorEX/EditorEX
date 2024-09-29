using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.UI.Factories;
using EditorEX.Util;
using HarmonyLib;
using IPA.Utilities;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static BeatmapEditor3D.DataModels.BeatmapsCollectionDataModel;

namespace EditorEX.UI.Patches
{
    internal class MapFilteringPatches : IAffinity
    {
        private StringInputFactory _stringInputFactory;

        private TMP_InputField _filterInput;

        private MapFilteringPatches(
            StringInputFactory stringInputFactory)
        {
            _stringInputFactory = stringInputFactory;
        }

        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.DidActivate))]
        [AffinityPrefix]
        private void AddUI(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                _filterInput = _stringInputFactory.Create(__instance.transform, "Filter", x =>
                {
                    ApplyFilter(__instance);
                });
                _filterInput.transform.parent.localPosition = new Vector3(-950f, 130f, 0f);
                (_filterInput.transform.parent as RectTransform).sizeDelta = new Vector2(400f, 40f);
            }
        }

        private void ApplyFilter(BeatmapsListViewController instance)
        {
            var filteredMaps = BeatmapFilterUtil.Filter(instance._beatmapsCollectionDataModel._beatmapInfos, _filterInput.text);
            instance._beatmapsListTableView.SetData(filteredMaps);
        }

        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.DidActivate))]
        [AffinityPostfix]
        private void Filter(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
            {
                ApplyFilter(__instance);
            }
        }


        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.HandleBeatmapsCollectionDataModelUpdated))]
        [AffinityPrefix]
        private bool KeepFilter(BeatmapsListViewController __instance)
        {
            ApplyFilter(__instance);
            return false;
        }

        // Maintains the proper index.
        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.HandleBeatmapListTableViewOpenBeatmap))]
        [AffinityPrefix]
        private void FixIndex(BeatmapsListViewController __instance, ref int idx)
        {
            var filteredMaps = __instance._beatmapsListTableView._beatmapInfos;
            idx = __instance._beatmapsCollectionDataModel.beatmapInfos.ToList().IndexOf(filteredMaps[idx]);
        }
    }
}
