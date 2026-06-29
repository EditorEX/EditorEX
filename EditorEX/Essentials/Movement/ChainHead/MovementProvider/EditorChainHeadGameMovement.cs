using System;
using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Visuals;
using EditorEX.Essentials.Visuals.ChainHead;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.ChainHead.MovementProvider
{
    // Game-view movement for a chain (burst slider).
    //
    // The chain head is, for movement purposes, just a note: it jumps in exactly like a note, so we
    // reuse the shared EditorNoteFloorMovement + EditorNoteJump machinery (driven here the same way
    // EditorNoteGameMovement drives it). Because the head note sits at the ChainNoteView's local
    // origin, jumping the root transform jumps the head.
    //
    // Each chain link (ChainElementNoteView) is a child of that root and runs its own note jump, laid
    // out to match the game's BurstSliderSpawner.ProcessSliderData: a quadratic bezier (whose
    // endpoints are the head/tail jump apexes) gives each link a planar (x, y) and a tangent; the link
    // note jumps with x offset += pos.x, pos.y folded into its gravity base, at a beat lerped between
    // head and tail, and is rotated by the tangent. Those per-link inputs are constant, so we compute
    // them once per Init; each frame we evaluate the link's jump and the head's jump and place the
    // link relative to the head (link.localPosition = linkJump - headJump) in unrotated local space,
    // since the link inherits the root's world rotation.
    public class EditorChainHeadGameMovement : MonoBehaviour, IObjectMovement
    {
        private const float _zOffset = 0.25f;
        private const float _endDistanceOffset = 500f;

        // Head movement (shared note machinery)
        private EditorNoteFloorMovement _floorMovement = null!;
        private EditorNoteJump _jump = null!;
        private MovementPhase _movementPhase;

        private Vector3 _position;
        private Vector3 _prevPosition;
        private Vector3 _localPosition;
        private Vector3 _prevLocalPosition;

        // Chain fields
        private ChainEditorData? _editorData;
        private IVariableMovementDataProvider _variableMovementDataProvider = null!;
        private IEditorBeatmapObjectSpawnMovementData _editorBeatmapObjectSpawnMovementData = null!;
        private HeadVisualRoot? _headVisualRoot;
        private ChainVisualRoots _visualRoots = null!;

        // Cached head jump inputs.
        private Vector3 _headNoteOffset;
        private Vector3 _tailNoteOffset;
        private float _headGravityBase;
        private float _tailGravityBase;
        private float _headNoteTime;
        private float _tailNoteTime;
        private float _headEndRotation;

        // Per-link jump inputs, precomputed once (ProcessSliderData is deterministic per chain).
        // rotation is the link's final (tangent) angle; the live rotation is driven per frame by the
        // EditorNoteJump rotation curve (identity through the floor, then rotating to this angle -- wound
        // to match the head -- early in the jump), so it must be recomputed per frame rather than baked in.
        private readonly List<(
            Transform transform,
            Vector3 offset,
            float gravityBase,
            float noteTime,
            float rotation
        )> _links = new();

        // Injected fields
        private AudioDataModel _audioDataModel = null!;
        private IAudioTimeSource _audioTimeSyncController = null!;

        [Inject]
        private void Construct(
            AudioDataModel audioDataModel,
            IAudioTimeSource audioTimeSyncController
        )
        {
            _audioDataModel = audioDataModel;
            _audioTimeSyncController = audioTimeSyncController;
        }

        protected void Awake()
        {
            _floorMovement = GetComponent<EditorNoteFloorMovement>();
            _jump = GetComponent<EditorNoteJump>();

            _movementPhase = MovementPhase.None;
            _floorMovement.floorMovementDidFinishEvent += HandleFloorMovementDidFinish;
            _jump.noteJumpDidFinishEvent += HandleNoteJumpDidFinish;
        }

        protected void OnDestroy()
        {
            if (_floorMovement)
            {
                _floorMovement.floorMovementDidFinishEvent -= HandleFloorMovementDidFinish;
            }
            if (_jump)
            {
                _jump.noteJumpDidFinishEvent -= HandleNoteJumpDidFinish;
            }
        }

        public void Init(
            BaseEditorData? editorData,
            IVariableMovementDataProvider variableMovementDataProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData,
            Func<IObjectVisuals>? getVisualRoot
        )
        {
            _editorData = editorData as ChainEditorData;
            if (_editorData == null)
            {
                return;
            }

            _variableMovementDataProvider = variableMovementDataProvider;
            _editorBeatmapObjectSpawnMovementData = movementData;

            // Build the shared head/link visual roots (basic + game children under a positioned root)
            // and rotate the head root with the jump, the same role the visuals root plays for a note.
            _visualRoots = GetComponent<ChainVisualRoots>();
            _visualRoots.EnsureBuilt();

            var headVisualRoot = new HeadVisualRoot(_visualRoots.HeadRoot.gameObject);
            _headVisualRoot = headVisualRoot;
            Func<IObjectVisuals> getHeadVisualRoot = () => headVisualRoot;

            var sliderSpawnData = _editorBeatmapObjectSpawnMovementData.GetSliderSpawnData(
                _editorData
            );

            _headNoteOffset = sliderSpawnData.headNoteOffset;
            _headNoteOffset.z += _zOffset;
            _tailNoteOffset = sliderSpawnData.tailNoteOffset;
            _tailNoteOffset.z += _zOffset;
            _headGravityBase = sliderSpawnData.headGravityBase;
            _tailGravityBase = sliderSpawnData.tailGravityBase;
            _headNoteTime = _audioDataModel.bpmData.BeatToSeconds(_editorData.beat);
            _tailNoteTime = _audioDataModel.bpmData.BeatToSeconds(_editorData.tailBeat);

            float worldRotation = 0f;
            float flipYSide = 0f;
            float endRotation = _editorData.cutDirection.RotationAngle();
            _headEndRotation = endRotation;

            _floorMovement.Init(
                _editorData,
                _variableMovementDataProvider,
                worldRotation,
                _headNoteTime,
                _headNoteOffset,
                _headNoteOffset,
                getHeadVisualRoot
            );
            _position = _floorMovement.SetToStart();
            _prevPosition = _position;
            _localPosition = _prevLocalPosition = _floorMovement.localPosition;

            _jump.Init(
                _editorData,
                _variableMovementDataProvider,
                _headNoteTime,
                worldRotation,
                _headNoteOffset,
                _headNoteOffset,
                _headGravityBase,
                flipYSide,
                endRotation,
                getHeadVisualRoot
            );

            _movementPhase = MovementPhase.MovingOnTheFloor;

            CacheChainElements();
        }

        // Precompute each link's note-jump inputs the way BurstSliderSpawner.ProcessSliderData does.
        // The bezier runs between the head and tail jump apexes (note offset + the apex rise), so the
        // links curve through the air exactly as in game; pos.x shifts the link's lane, pos.y feeds its
        // gravity base, and the tangent gives its rotation.
        private void CacheChainElements()
        {
            _links.Clear();

            int sliceCount = _editorData!.sliceCount;
            if (sliceCount < 2)
            {
                return;
            }

            float halfJumpDuration = _variableMovementDataProvider.halfJumpDuration;
            float headGravity = _variableMovementDataProvider.CalculateCurrentNoteJumpGravity(
                _headGravityBase
            );
            float tailGravity = _variableMovementDataProvider.CalculateCurrentNoteJumpGravity(
                _tailGravityBase
            );
            float headApexRise = headGravity * halfJumpDuration * halfJumpDuration * 0.5f;
            float tailApexRise = tailGravity * halfJumpDuration * halfJumpDuration * 0.5f;

            Vector2 headApex = new Vector2(_headNoteOffset.x, _headNoteOffset.y + headApexRise);
            Vector2 chainVector =
                new Vector2(_tailNoteOffset.x, _tailNoteOffset.y + tailApexRise) - headApex;
            float magnitude = chainVector.magnitude;
            float controlAngle =
                (_editorData.cutDirection.RotationAngle() - 90f) * (Mathf.PI / 180f);
            Vector2 controlPoint =
                0.5f * magnitude * new Vector2(Mathf.Cos(controlAngle), Mathf.Sin(controlAngle));
            float squishAmount = _editorData.squishAmount;

            foreach (ChainVisualRoots.LinkRoot link in _visualRoots.Links)
            {
                float fraction = (float)link.Element.linkId / (sliceCount - 1);
                BezierCurve(
                    Vector2.zero,
                    controlPoint,
                    chainVector,
                    fraction * squishAmount,
                    out Vector2 pos,
                    out Vector2 tangent
                );

                Vector3 offset = _headNoteOffset + new Vector3(pos.x, 0f, 0f);
                float gravityBase = headApexRise + pos.y;
                float noteTime = Mathf.LerpUnclamped(_headNoteTime, _tailNoteTime, fraction);
                float rotation = Vector2.SignedAngle(new Vector2(0f, -1f), tangent);

                // Drive the link's visual root (its basic + game meshes ride along), not the editor
                // element directly.
                _links.Add((link.Root, offset, gravityBase, noteTime, rotation));
            }
        }

        public void Setup(BaseEditorData? editorData) { }

        public void ManualUpdate()
        {
            // Drive the head (the root) with the shared note jump/floor machinery.
            _prevPosition = _position;
            _prevLocalPosition = _localPosition;
            if (_movementPhase == MovementPhase.MovingOnTheFloor)
            {
                _position = _floorMovement.ManualUpdate();
                _localPosition = _floorMovement.localPosition;
            }
            else
            {
                _position = _jump.ManualUpdate();
                _localPosition = _jump.localPosition;
            }

            UpdateChainElements();
        }

        // Place each link relative to the head: both run the same closed-form note jump (link at its
        // own time/offset/gravity), and the link's local position is the difference. The root carries
        // the links along via its own transform.
        private void UpdateChainElements()
        {
            if (_links.Count == 0)
            {
                return;
            }

            float songTime = _audioTimeSyncController.songTime;
            float halfJumpDuration = _variableMovementDataProvider.halfJumpDuration;
            float moveDuration = _variableMovementDataProvider.moveDuration;
            Vector3 headLocal = ComputeJumpLocalPosition(
                songTime,
                _headNoteTime,
                _headNoteOffset,
                _headGravityBase
            );

            foreach (
                (
                    Transform transform,
                    Vector3 offset,
                    float gravityBase,
                    float noteTime,
                    float rotation
                ) in _links
            )
            {
                if (transform == null || transform.parent != base.transform)
                {
                    continue;
                }

                // Hide the link until it starts sliding in, the same way the floor movement gates the
                // head note. Otherwise the jump extrapolates far off-screen out of the spawn window and
                // the links visibly fly away when scrubbing past the chain.
                bool active = songTime > noteTime - moveDuration - halfJumpDuration;
                GameObject linkObject = transform.gameObject;
                if (linkObject.activeSelf != active)
                {
                    linkObject.SetActive(active);
                }
                if (!active)
                {
                    continue;
                }

                Vector3 linkLocal = ComputeJumpLocalPosition(
                    songTime,
                    noteTime,
                    offset,
                    gravityBase
                );
                transform.localPosition = linkLocal - headLocal;
                transform.localRotation = ComputeJumpRotation(songTime, noteTime, rotation);
            }
        }

        // Closed-form note position (unrotated, in the spawn-movement local frame), matching
        // EditorNoteFloorMovement + EditorNoteJump: a constant-speed floor slide until the jump
        // begins, then the parabolic jump (with the late "shoot past the player" z pull). The chain
        // uses moveStart == moveEnd == jumpEnd offset, so a single offset feeds all three terms.
        private Vector3 ComputeJumpLocalPosition(
            float songTime,
            float noteTime,
            Vector3 offset,
            float gravityBase
        )
        {
            float halfJumpDuration = _variableMovementDataProvider.halfJumpDuration;
            Vector3 moveStart = _variableMovementDataProvider.moveStartPosition + offset;
            Vector3 moveEnd = _variableMovementDataProvider.moveEndPosition + offset;
            Vector3 jumpEnd = _variableMovementDataProvider.jumpEndPosition + offset;

            if (songTime < noteTime - halfJumpDuration)
            {
                float moveDuration = _variableMovementDataProvider.moveDuration;
                float floorTime = songTime - (noteTime - moveDuration - halfJumpDuration);
                return Vector3.LerpUnclamped(moveStart, moveEnd, floorTime / moveDuration);
            }

            float gravity = _variableMovementDataProvider.CalculateCurrentNoteJumpGravity(
                gravityBase
            );
            float num = songTime - (noteTime - halfJumpDuration);
            float num2 = num / _variableMovementDataProvider.jumpDuration;

            Vector3 localPosition;
            if (Mathf.Approximately(moveEnd.x, jumpEnd.x))
            {
                localPosition.x = moveEnd.x;
            }
            else if (num2 < 0.25f)
            {
                localPosition.x = moveEnd.x + (jumpEnd.x - moveEnd.x) * InOutQuad(num2 * 4f);
            }
            else
            {
                localPosition.x = jumpEnd.x;
            }
            localPosition.z = Mathf.LerpUnclamped(moveEnd.z, jumpEnd.z, num2);
            localPosition.y =
                moveEnd.y + gravity * halfJumpDuration * num - gravity * num * num * 0.5f;

            if (num2 >= 0.75f)
            {
                float t = (num2 - 0.75f) / 0.25f;
                t = t * t * t;
                localPosition.z -= Mathf.LerpUnclamped(0f, _endDistanceOffset, t);
            }

            return localPosition;
        }

        private Quaternion ComputeJumpRotation(float songTime, float noteTime, float endRotation)
        {
            float halfJumpDuration = _variableMovementDataProvider.halfJumpDuration;
            float jumpDuration = _variableMovementDataProvider.jumpDuration;
            float num = songTime - (noteTime - halfJumpDuration);
            float num2 = num / jumpDuration;
            float num2Clamped = Mathf.Clamp(num2, 0f, 0.5f);

            var endRotationQuat = Quaternion.Euler(0f, 0f, endRotation);
            var middleRotationQuat = new Quaternion { eulerAngles = endRotationQuat.eulerAngles };

            Quaternion quaternion;
            if (num2Clamped < 0.125f)
            {
                quaternion = Quaternion.Slerp(
                    Quaternion.identity,
                    middleRotationQuat,
                    Mathf.Sin(num2Clamped * 3.1415927f * 4f)
                );
            }
            else
            {
                quaternion = Quaternion.Slerp(
                    middleRotationQuat,
                    endRotationQuat,
                    Mathf.Sin((num2Clamped - 0.125f) * 3.1415927f * 2f)
                );
            }

            return quaternion;
        }

        // Quadratic bezier, identical to BurstSliderSpawner.BezierCurve.
        private static void BezierCurve(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            float t,
            out Vector2 pos,
            out Vector2 tangent
        )
        {
            float num = 1f - t;
            pos = (num * num * p0) + (2f * num * t * p1) + (t * t * p2);
            tangent = (2f * (1f - t) * (p1 - p0)) + (2f * t * (p2 - p1));
        }

        private static float InOutQuad(float t) =>
            t < 0.5f ? 2f * t * t : 1f - (((-2f * t) + 2f) * ((-2f * t) + 2f)) / 2f;

        public void Enable() { }

        // Nothing to restore here: when we hand off to basic mode, EditorChainHeadBasicMovement.Init
        // re-lays out the head note transform and link element roots into the static chain shape, so
        // the jump's last-frame poses are overwritten there rather than reset here.
        public void Disable() { }

        private void HandleFloorMovementDidFinish()
        {
            _movementPhase = MovementPhase.Jumping;
            _position = _jump.ManualUpdate();
            _localPosition = _jump.localPosition;
        }

        private void HandleNoteJumpDidFinish()
        {
            _movementPhase = MovementPhase.None;
        }

        private enum MovementPhase
        {
            None,
            MovingOnTheFloor,
            Jumping,
        }

        // Minimal IObjectVisuals wrapper so the shared jump/floor machinery has a visual root to rotate
        // and toggle. For a chain that root is the head note transform.
        private sealed class HeadVisualRoot : IObjectVisuals
        {
            private readonly GameObject _root;

            public HeadVisualRoot(GameObject root)
            {
                _root = root;
            }

            public GameObject GetVisualRoot() => _root;

            public void Init(BaseEditorData? editorData) { }

            public void ManualUpdate() { }

            public void Enable() { }

            public void Disable() { }
        }
    }
}
