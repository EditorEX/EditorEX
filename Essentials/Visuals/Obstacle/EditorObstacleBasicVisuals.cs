using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals.Obstacle
{
    internal class EditorObstacleBasicVisuals : MonoBehaviour, IObjectVisuals
    {
        private GameObject _basicRoot;
        private GameObject _basicRootFrame;
        private StretchableObstacle _stretchableObstacle;

        private ObstacleEditorData _editorData;

        private bool _active;

        private ColorManager _colorManager = null!;
        private BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper = null!;

        [Inject]
        private void Construct(
            ColorManager colorManager,
            BeatmapObjectPlacementHelper beatmapObjectPlacementHelper
        )
        {
            _colorManager = colorManager;
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;

            _basicRoot = transform?.Find("ObstacleCore")?.gameObject ?? gameObject;
            _basicRootFrame = transform?.Find("ObstacleFrame")?.gameObject ?? gameObject;

            _stretchableObstacle = gameObject.GetComponent<StretchableObstacle>();

            Disable();
        }

        public void Init(BaseEditorData? baseEditorData)
        {
            _editorData = baseEditorData as ObstacleEditorData;
            if (_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }

            //float length = _beatmapObjectPlacementHelper.BeatToPosition(_editorData.beat + _editorData.duration) - _beatmapObjectPlacementHelper.BeatToPosition(_editorData.beat);

            //_stretchableObstacle.SetSizeAndColor(_editorData.width * 0.8f, _editorData.height * 0.8f, length, _colorManager.obstaclesColor);
            _active = true;
        }

        public void Enable()
        {
            _basicRoot?.SetActive(false);
            _basicRootFrame?.SetActive(true);

            //_stretchableObstacle._obstacleFrame = _basicRootFrame.GetComponent<ParametricBoxFrameController>();
            //_stretchableObstacle._obstacleCore = _basicRoot.transform;
        }

        public void Disable()
        {
            _basicRoot?.SetActive(false);
            _basicRootFrame?.SetActive(false);
            _active = false;
        }

        public void ManualUpdate() { }

        public GameObject GetVisualRoot()
        {
            return _basicRoot;
        }
    }
}
