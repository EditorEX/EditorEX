using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Animation;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals.Obstacle
{
    internal class EditorObstacleGameVisuals : MonoBehaviour, IObjectVisuals
    {
        // Injected fields
        private VisualAssetProvider _visualAssetProvider;
        private ColorManager _colorManager;
        private IReadonlyBeatmapState _state;
        private EditorDeserializedData _editorDeserializedData;
        private AnimationHelper _animationHelper;

        // Visuals fields
        private ObstacleEditorData? _editorData;
        private GameObject _gameRoot;

        private StretchableObstacle _stretchableObstacle;

        private CutoutEffect _wallCutout;
        private CutoutEffect _frameCutout;

        // *sigh* 
        private GameObject _basicFrame;

        private bool _active;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
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

            if (_visualAssetProvider.gameNotePrefab == null)
            {
                _visualAssetProvider.onFinishLoading += SetupObjectAndUnbind;
            }
            else
            {
                SetupObject();
            }
        }

        private void SetupObjectAndUnbind()
        {
            _visualAssetProvider.onFinishLoading -= SetupObjectAndUnbind;
            SetupObject();
        }

        private void SetupObject()
        {
            _gameRoot = new GameObject("GameWallRoot");
            _gameRoot.transform.SetParent(transform, false);
            var core = Instantiate(_visualAssetProvider.obstaclePrefab.transform.Find("ObstacleCore"), _gameRoot.transform, false).gameObject;
            var hideWrapper = Instantiate(_visualAssetProvider.obstaclePrefab.transform.Find("HideWrapper"), _gameRoot.transform, false).gameObject;

            Destroy(core.transform.Find("Collider").gameObject);
            Destroy(core.transform.Find("DepthWrite").gameObject);

            core.layer = 11;

            _stretchableObstacle = _gameRoot.AddComponent<StretchableObstacle>();

            _wallCutout = core.GetComponent<CutoutEffect>();
            _frameCutout = hideWrapper.transform.Find("ObstacleFrame").GetComponent<CutoutEffect>();

            _stretchableObstacle._obstacleFrame = _frameCutout.GetComponent<ParametricBoxFrameController>();
            _stretchableObstacle._obstacleCore = core.transform;

            _stretchableObstacle._materialPropertyBlockControllers = new[]
            {
                core.GetComponent<MaterialPropertyBlockController>(),
                hideWrapper.transform.Find("ObstacleFrame").GetComponent<MaterialPropertyBlockController>()
            };

            _basicFrame = gameObject.transform.Find("ObstacleFrame").gameObject;

            Disable();
        }

        public void Init(BaseEditorData? editorData)
        {
            _editorData = editorData as ObstacleEditorData;

            if (_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }

            var obstacleColor = _colorManager.obstaclesColor;

            _stretchableObstacle.SetAllProperties(_stretchableObstacle._obstacleFrame.width, _stretchableObstacle._obstacleFrame.height, _stretchableObstacle._obstacleFrame.length, obstacleColor, _state.beat);

            _wallCutout.SetCutout(0f);
            _frameCutout.SetCutout(0f);
        }

        public void Enable()
        {
            _gameRoot?.SetActive(true);
            _basicFrame?.SetActive(false);
            _active = true;
        }

        public void Disable()
        {
            _gameRoot?.SetActive(false);
            _basicFrame?.SetActive(true);
            _active = false;
        }

        public void ManualUpdate()
        {
            if (!(_editorDeserializedData?.Resolve(_editorData, out EditorNoodleObstacleData? noodleData) ?? false) || noodleData == null)
            {
                return;
            }

            IReadOnlyList<Track>? tracks = noodleData.Track;
            NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
            if (tracks == null && animationObject == null)
            {
                return;
            }

            var normalTime = noodleData.GetTimeProperty() ?? 0f;

            _animationHelper.GetObjectOffset(
                animationObject,
                tracks,
                normalTime,
                out Vector3? _,
                out Quaternion? _,
                out Vector3? _,
                out Quaternion? _,
                out float? dissolve,
                out float? _,
                out float? _);

            _wallCutout.SetCutout(1f - dissolve.GetValueOrDefault(1f));

            _frameCutout.SetCutout(1f - dissolve.GetValueOrDefault(1f));
        }

        public GameObject GetVisualRoot()
        {
            return _gameRoot;
        }
    }
}
