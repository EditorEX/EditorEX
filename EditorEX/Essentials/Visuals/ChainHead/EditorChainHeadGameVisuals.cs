using System.Collections.Generic;
using System.Reflection;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using Chroma;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using HarmonyLib;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Animation;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals.ChainHead
{
    // Preview/game-mode chain visuals. The shared ChainVisualRoots gives every head/link a positioned
    // root with a "Game" child slot; this provider instantiates the gameplay meshes into those slots
    // and toggles only them (the basic visuals owns the "Basic" slots). The head is a normal note, so
    // it clones the gameplay head note cube; each link clones the burst-slider element wrapper. Color
    // comes from the ColorManager; dissolve is driven per frame from the chain's Noodle/Chroma track,
    // applied uniformly across head and links.
    internal class EditorChainHeadGameVisuals : MonoBehaviour, IObjectVisuals
    {
        private static readonly FieldInfo _wrapperField = AccessTools.Field(
            typeof(BurstSliderGameNoteController),
            "_wrapperGO"
        );

        // Injected fields
        private VisualAssetProvider _visualAssetProvider = null!;
        private ColorManager _colorManager = null!;
        private IReadonlyBeatmapState _state = null!;
        private AudioDataModel _audioDataModel = null!;
        private EditorDeserializedData _noodleEditorDeserializedData = null!;
        private EditorDeserializedData _chromaEditorDeserializedData = null!;
        private AnimationHelper _animationHelper = null!;

        // Visuals fields
        private ChainEditorData? _editorData;
        private EditorNoodleBaseNoteData? _noodleData;
        private ChromaObjectData? _chromaData;
        private EditorNoteJump _noteJump;
        private ChainVisualRoots _visualRoots;
        private bool _active;
        private bool _subscribed;

        // Head visuals (cloned gameplay note cube under HeadGame).
        private GameObject _headGameRoot;
        private MaterialPropertyBlockController[] _headColorControllers;
        private CutoutEffect _headNoteCutout;
        private CutoutEffect _headArrowCutout;
        private GameObject[] _headArrowObjects;
        private GameObject _headCircleObject;

        // Per-link visuals (cloned burst-slider element wrapper under each link's Game slot).
        private readonly List<LinkVisual> _linkVisuals = new();

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")]
                EditorDeserializedData noodleEditorDeserializedData,
            [InjectOptional(Id = "Chroma")] EditorDeserializedData chromaEditorDeserializedData,
            AnimationHelper animationHelper,
            VisualAssetProvider visualAssetProvider,
            ColorManager colorManager,
            IReadonlyBeatmapState state,
            AudioDataModel audioDataModel
        )
        {
            _noodleEditorDeserializedData = noodleEditorDeserializedData;
            _chromaEditorDeserializedData = chromaEditorDeserializedData;
            _animationHelper = animationHelper;
            _visualAssetProvider = visualAssetProvider;
            _colorManager = colorManager;
            _state = state;
            _audioDataModel = audioDataModel;

            _noteJump = GetComponent<EditorNoteJump>();
            _visualRoots = GetComponent<ChainVisualRoots>();
        }

        public void Init(BaseEditorData? editorData)
        {
            _editorData = editorData as ChainEditorData;

            EditorNoodleBaseNoteData? noodleData = null;
            _noodleEditorDeserializedData?.Resolve(_editorData, out noodleData);
            _noodleData = noodleData;

            ChromaObjectData? chromaData = null;
            _chromaEditorDeserializedData?.Resolve(_editorData, out chromaData);
            _chromaData = chromaData;

            if (!EnsureSetup())
            {
                // Assets are still loading; rebuild once they arrive (with the data we now have).
                if (!_subscribed)
                {
                    _subscribed = true;
                    _visualAssetProvider.onFinishLoading += HandleAssetsLoaded;
                }
                return;
            }

            ApplyColors();
            ResetCutouts();
            ApplyVisibility();
        }

        private void HandleAssetsLoaded()
        {
            _visualAssetProvider.onFinishLoading -= HandleAssetsLoaded;
            _subscribed = false;
            if (_editorData != null)
            {
                Init(_editorData);
            }
        }

        // Build the head clone once and reconcile the per-link clones against the chain's current roots.
        // Returns false if the gameplay prefabs aren't loaded yet or the roots aren't ready.
        private bool EnsureSetup()
        {
            if (
                _visualAssetProvider.burstSliderHeadPrefab == null
                || _visualAssetProvider.burstSliderElementPrefab == null
            )
            {
                return false;
            }

            if (_visualRoots == null || !_visualRoots.EnsureBuilt())
            {
                return false;
            }

            if (_headGameRoot == null)
            {
                BuildHead(_visualRoots.HeadGame);
            }

            ReconcileLinks();
            return true;
        }

        private void BuildHead(Transform headGameSlot)
        {
            _headGameRoot = Instantiate(
                _visualAssetProvider.burstSliderHeadPrefab.transform.Find("NoteCube"),
                headGameSlot,
                false
            ).gameObject;
            _headGameRoot.name = "ChainHeadGameCube";
            _headGameRoot.transform.localPosition = Vector3.zero;
            _headGameRoot.transform.localRotation = Quaternion.identity;

            _headArrowObjects =
            [
                _headGameRoot.transform.Find("NoteArrow").gameObject,
                _headGameRoot.transform.Find("NoteArrowGlow").gameObject,
            ];
            _headCircleObject = _headGameRoot.transform.Find("NoteCircleGlow").gameObject;

            _headColorControllers =
            [
                _headGameRoot.GetComponent<MaterialPropertyBlockController>(),
                _headCircleObject.GetComponent<MaterialPropertyBlockController>(),
                _headArrowObjects[1].GetComponent<MaterialPropertyBlockController>(),
            ];

            _headNoteCutout = _headGameRoot.GetComponent<CutoutEffect>();
            _headArrowCutout = _headArrowObjects[0].GetComponent<CutoutEffect>();
        }

        private void ReconcileLinks()
        {
            IReadOnlyList<ChainVisualRoots.LinkRoot> links = _visualRoots.Links;

            // If the link set is unchanged, keep the existing clones.
            if (_linkVisuals.Count == links.Count)
            {
                bool same = true;
                for (int i = 0; i < links.Count; i++)
                {
                    if (_linkVisuals[i].slot != links[i].Game)
                    {
                        same = false;
                        break;
                    }
                }
                if (same)
                {
                    return;
                }
            }

            ClearLinkVisuals();

            var wrapperPrefab = GetElementWrapper();
            if (wrapperPrefab == null)
            {
                return;
            }

            foreach (ChainVisualRoots.LinkRoot link in links)
            {
                var clone = Instantiate(wrapperPrefab, link.Game, false);
                clone.name = "ChainLinkGameMesh";
                clone.transform.localPosition = Vector3.zero;
                clone.transform.localRotation = Quaternion.identity;
                clone.SetActive(true);

                _linkVisuals.Add(
                    new LinkVisual
                    {
                        slot = link.Game,
                        clone = clone,
                        colorControllers =
                            clone.GetComponentsInChildren<MaterialPropertyBlockController>(true),
                        cutouts = clone.GetComponentsInChildren<CutoutEffect>(true),
                    }
                );
            }
        }

        // The burst element's visual lives under its controller's _wrapperGO; clone that so we don't
        // drag in the gameplay controller (with its Zenject injects) attached to the prefab root.
        private GameObject GetElementWrapper()
        {
            var controller =
                _visualAssetProvider.burstSliderElementPrefab.GetComponent<BurstSliderGameNoteController>();
            if (controller == null)
            {
                return null;
            }
            return (GameObject)_wrapperField.GetValue(controller);
        }

        private void ClearLinkVisuals()
        {
            foreach (LinkVisual link in _linkVisuals)
            {
                if (link.clone != null)
                {
                    Destroy(link.clone);
                }
            }
            _linkVisuals.Clear();
        }

        private void ApplyColors()
        {
            if (_editorData == null)
            {
                return;
            }

            Color color = _colorManager.ColorForType(_editorData.colorType).ColorWithAlpha(1f);

            if (_headGameRoot != null)
            {
                SetControllerColors(_headColorControllers, color);

                bool anyDirection = _editorData.cutDirection == NoteCutDirection.Any;
                foreach (GameObject arrowObject in _headArrowObjects)
                {
                    arrowObject.SetActive(!anyDirection);
                }
                _headCircleObject.SetActive(anyDirection);
                _headCircleObject.GetComponent<MeshRenderer>().enabled = anyDirection;
            }

            foreach (LinkVisual link in _linkVisuals)
            {
                SetControllerColors(link.colorControllers, color);
            }
        }

        private static void SetControllerColors(
            MaterialPropertyBlockController[]? controllers,
            Color color
        )
        {
            if (controllers == null)
            {
                return;
            }
            foreach (MaterialPropertyBlockController controller in controllers)
            {
                if (controller == null)
                {
                    continue;
                }
                controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, color);
                controller.ApplyChanges();
            }
        }

        private void ResetCutouts()
        {
            _headNoteCutout?.SetCutout(0f);
            _headArrowCutout?.SetCutout(0f);
            foreach (LinkVisual link in _linkVisuals)
            {
                SetCutouts(link.cutouts, 0f);
            }
        }

        private static void SetCutouts(CutoutEffect[]? cutouts, float value)
        {
            if (cutouts == null)
            {
                return;
            }
            foreach (CutoutEffect cutout in cutouts)
            {
                if (cutout != null)
                {
                    cutout.SetCutout(value);
                }
            }
        }

        // Swap the basic and game slots together: when this provider is active the gameplay clones show
        // and the editor meshes hide, and vice versa. The game provider owns the swap (rather than each
        // side toggling itself) so it stays correct on first-load-in-game and after a roots rebuild,
        // where the basic slots would otherwise default to active with no one to hide them.
        private void ApplyVisibility()
        {
            if (_visualRoots != null)
            {
                if (_visualRoots.HeadBasic != null)
                {
                    _visualRoots.HeadBasic.gameObject.SetActive(!_active);
                }
                foreach (ChainVisualRoots.LinkRoot link in _visualRoots.Links)
                {
                    if (link.Basic != null)
                    {
                        link.Basic.gameObject.SetActive(!_active);
                    }
                }
            }

            if (_headGameRoot != null)
            {
                _headGameRoot.SetActive(_active);
            }
            foreach (LinkVisual link in _linkVisuals)
            {
                if (link.clone != null)
                {
                    link.clone.SetActive(_active);
                }
            }
        }

        public void Enable()
        {
            _active = true;
            ApplyVisibility();
        }

        public void Disable()
        {
            _active = false;
            ApplyVisibility();
        }

        public void ManualUpdate()
        {
            EditorNoodleBaseNoteData? noodleData = _noodleData;
            if (noodleData == null)
            {
                return;
            }

            IReadOnlyList<Track>? tracks = noodleData.Track;
            NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
            if (tracks == null && animationObject == null)
            {
                return;
            }

            float normalTime;
            float? time2 = noodleData.GetTimeProperty();
            if (time2.HasValue)
            {
                normalTime = time2.Value;
            }
            else if (_noteJump != null && _editorData != null)
            {
                float jumpDuration = _noteJump.jumpDuration;
                float elapsedTime =
                    _audioDataModel.bpmData.BeatToSeconds(_state.beat)
                    - (
                        _audioDataModel.bpmData.BeatToSeconds(_editorData.beat)
                        - (jumpDuration * 0.5f)
                    );
                normalTime = elapsedTime / jumpDuration;
            }
            else
            {
                normalTime = 0f;
            }

            _animationHelper.GetObjectOffset(
                animationObject,
                tracks,
                normalTime,
                out _,
                out _,
                out _,
                out _,
                out float? dissolveNote,
                out float? dissolveArrow,
                out _
            );

            float noteCutout = 1f - dissolveNote.GetValueOrDefault(1f);
            float arrowCutout = 1f - dissolveArrow.GetValueOrDefault(1f);

            _headNoteCutout?.SetCutout(noteCutout);
            _headArrowCutout?.SetCutout(arrowCutout);
            if (_headArrowObjects != null && _editorData != null)
            {
                _headArrowObjects[1]
                    .SetActive(
                        _editorData.cutDirection != NoteCutDirection.Any && dissolveArrow == 1f
                    );
            }

            foreach (LinkVisual link in _linkVisuals)
            {
                SetCutouts(link.cutouts, noteCutout);
            }

            ChromaObjectData? chromaData = _chromaData;
            if (chromaData == null)
            {
                return;
            }

            IReadOnlyList<Track>? chromaTracks = chromaData.Track;
            PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
            if (chromaTracks == null && pathPointDefinition == null)
            {
                return;
            }

            global::Chroma.Animation.AnimationHelper.GetColorOffset(
                pathPointDefinition,
                chromaTracks,
                normalTime,
                out Color? colorOffset
            );

            if (colorOffset == null)
            {
                return;
            }

            Color animatedColor = colorOffset.Value.ColorWithAlpha(1f);
            SetControllerColors(_headColorControllers, animatedColor);
            foreach (LinkVisual link in _linkVisuals)
            {
                SetControllerColors(link.colorControllers, animatedColor);
            }
        }

        public GameObject GetVisualRoot()
        {
            return _headGameRoot != null ? _headGameRoot : gameObject;
        }

        protected void OnDestroy()
        {
            if (_subscribed && _visualAssetProvider != null)
            {
                _visualAssetProvider.onFinishLoading -= HandleAssetsLoaded;
                _subscribed = false;
            }
        }

        private sealed class LinkVisual
        {
            public Transform slot;
            public GameObject clone;
            public MaterialPropertyBlockController[] colorControllers;
            public CutoutEffect[] cutouts;
        }
    }
}
