using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals.Obstacle
{
    internal class EditorObstacleGameVisuals : MonoBehaviour, IObjectVisuals
    {
        // Injected fields
        private VisualAssetProvider _visualAssetProvider = null!;
        private ColorManager _colorManager = null!;
        private IReadonlyBeatmapState _state = null!;
        private EditorDeserializedData _editorDeserializedData = null!;

        // Visuals fields
        private ObstacleEditorData? _editorData;
        private EditorNoodleObstacleData? _noodleData;
        private GameObject _gameRoot;

        private StretchableObstacle _stretchableObstacle;

        private CutoutEffect _wallCutout;
        private CutoutEffect _frameCutout;

        // *sigh*
        private GameObject _basicFrame;

        private bool _active;
        private float _lastWallCutout = float.NaN;
        private float _lastFrameCutout = float.NaN;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            VisualAssetProvider visualAssetProvider,
            ColorManager colorManager,
            IReadonlyBeatmapState state
        )
        {
            _editorDeserializedData = editorDeserializedData;
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
            var core = Instantiate(
                _visualAssetProvider.obstaclePrefab.transform.Find("ObstacleCore"),
                _gameRoot.transform,
                false
            ).gameObject;
            var hideWrapper = Instantiate(
                _visualAssetProvider.obstaclePrefab.transform.Find("HideWrapper"),
                _gameRoot.transform,
                false
            ).gameObject;

            Destroy(core.transform.Find("Collider").gameObject);
            Destroy(core.transform.Find("DepthWrite").gameObject);

            core.layer = 11;

            _stretchableObstacle = _gameRoot.AddComponent<StretchableObstacle>();

            _wallCutout = core.GetComponent<CutoutEffect>();
            _frameCutout = hideWrapper.transform.Find("ObstacleFrame").GetComponent<CutoutEffect>();

            _stretchableObstacle._obstacleFrame =
                _frameCutout.GetComponent<ParametricBoxFrameController>();
            _stretchableObstacle._obstacleCore = core.transform;

            _stretchableObstacle._materialPropertyBlockControllers = new[]
            {
                core.GetComponent<MaterialPropertyBlockController>(),
                hideWrapper
                    .transform.Find("ObstacleFrame")
                    .GetComponent<MaterialPropertyBlockController>(),
            };

            _basicFrame = gameObject.transform.Find("ObstacleFrame").gameObject;

            Disable();
        }

        public void Init(BaseEditorData? editorData)
        {
            _editorData = editorData as ObstacleEditorData;
            _lastWallCutout = float.NaN;
            _lastFrameCutout = float.NaN;

            EditorNoodleObstacleData? noodleData = null;
            _editorDeserializedData?.Resolve(_editorData, out noodleData);
            _noodleData = noodleData;

            if (_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }

            var obstacleColor = _colorManager.obstaclesColor;

            _stretchableObstacle.SetAllProperties(
                _stretchableObstacle._obstacleFrame.width,
                _stretchableObstacle._obstacleFrame.height,
                _stretchableObstacle._obstacleFrame.length,
                obstacleColor,
                _state.beat
            );

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
            EditorNoodleObstacleData? noodleData = _noodleData;
            if (noodleData == null)
            {
                return;
            }

            if (
                DissolveCutout.TryGetChanged(
                    noodleData.InternalDissolve,
                    ref _lastWallCutout,
                    out float wallCutout
                )
            )
            {
                _wallCutout.SetCutout(wallCutout);
            }

            if (
                DissolveCutout.TryGetChanged(
                    noodleData.InternalDissolve,
                    ref _lastFrameCutout,
                    out float frameCutout
                )
            )
            {
                _frameCutout.SetCutout(frameCutout);
            }
        }

        public GameObject GetVisualRoot()
        {
            return _gameRoot;
        }
    }
}
