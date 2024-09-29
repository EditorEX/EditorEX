using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using HarmonyLib;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Animation;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals
{
    internal class EditorBombGameVisuals : MonoBehaviour, IObjectVisuals
    {
        // Injected fields
        private VisualAssetProvider _visualAssetProvider;
        private ColorManager _colorManager;
        private IReadonlyBeatmapState _state;
        private EditorDeserializedData _editorDeserializedData;
        private AnimationHelper _animationHelper;

        // Visuals fields
        private bool _active;

        [Inject]
        private void Construct(
            [Inject(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            VisualAssetProvider visualAssetProvider,
            ColorManager colorManager,
            IReadonlyBeatmapState state)
        {
            _editorDeserializedData = editorDeserializedData;
            _animationHelper = animationHelper;
            _visualAssetProvider = visualAssetProvider;
            _colorManager = colorManager;
            _state = state;
        }

        private void SetupObject()
        {
            Disable();
        }

        public void Init(BaseEditorData editorData)
        {
            if (_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        public void Enable()
        {
            gameObject?.SetActive(true);
            _active = true;
        }

        public void Disable()
        {
            gameObject?.SetActive(false);
            _active = false;
        }

        public void ManualUpdate()
        {
        }

        public GameObject GetVisualRoot()
        {
            return gameObject;
        }
    }
}
