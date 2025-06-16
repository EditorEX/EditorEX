using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using NoodleExtensions.Animation;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals.Note
{
    internal class EditorBombGameVisuals : MonoBehaviour, IObjectVisuals
    {
        // Injected fields
        private VisualAssetProvider _visualAssetProvider = null!;
        private ColorManager _colorManager = null!;
        private IReadonlyBeatmapState _state = null!;
        private EditorDeserializedData _editorDeserializedData = null!;
        private AnimationHelper _animationHelper = null!;

        // Visuals fields
        private bool _active;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            VisualAssetProvider visualAssetProvider,
            ColorManager colorManager,
            IReadonlyBeatmapState state
        )
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

        public void Init(BaseEditorData? editorData)
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

        public void ManualUpdate() { }

        public GameObject GetVisualRoot()
        {
            return gameObject;
        }
    }
}
