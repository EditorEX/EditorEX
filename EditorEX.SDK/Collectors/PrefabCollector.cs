using System;
using BeatmapEditor3D;
using HMUI;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Collectors
{
    //TODO: Check if this is needed after reactive refactor (used to collect UI prefabs)
    public class PrefabCollector : IInitializable
    {
        private DiContainer _container = null!;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
        }
    }
}
