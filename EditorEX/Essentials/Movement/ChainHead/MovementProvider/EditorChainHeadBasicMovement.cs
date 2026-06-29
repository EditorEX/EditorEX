using System;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using EditorEX.Essentials.Visuals.ChainHead;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.ChainHead.MovementProvider
{
    public class EditorChainHeadBasicMovement : MonoBehaviour, IObjectMovement
    {
        private const float LaneWidth = 0.8f;
        private const float YOffset = 0.5f;

        private ChainEditorData? _editorData;
        private ChainVisualRoots _visualRoots = null!;

        private BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper = null!;

        [Inject]
        public void Construct(BeatmapObjectPlacementHelper beatmapObjectPlacementHelper)
        {
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;
        }

        public void Init(
            BaseEditorData? editorData,
            IVariableMovementDataProvider variableMovementDataProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData,
            Func<IObjectVisuals>? getVisualRoot
        )
        {
            if (editorData == null || editorData is not ChainEditorData)
            {
                throw new ArgumentNullException(nameof(editorData));
            }
            _editorData = editorData as ChainEditorData;
            float z = _beatmapObjectPlacementHelper.BeatToPosition(editorData.beat);
            transform.localPosition = new Vector3(
                (_editorData.column - 1.5f) * LaneWidth,
                YOffset + _editorData.row * LaneWidth,
                z
            );

            _visualRoots = GetComponent<ChainVisualRoots>();
            _visualRoots.EnsureBuilt();

            LayoutChain(z);
        }

        private void LayoutChain(float headZ)
        {
            if (_editorData == null)
            {
                return;
            }

            if (_visualRoots.HeadRoot != null)
            {
                _visualRoots.HeadRoot.localPosition = Vector3.zero;
                _visualRoots.HeadRoot.localRotation = Quaternion.AngleAxis(
                    _editorData.cutDirection.RotationAngle(),
                    Vector3.forward
                );
                if (!_visualRoots.HeadRoot.gameObject.activeSelf)
                {
                    _visualRoots.HeadRoot.gameObject.SetActive(true);
                }
            }

            int sliceCount = _editorData.sliceCount;
            if (sliceCount < 2 || _visualRoots.Links.Count == 0)
            {
                return;
            }

            float tailZ = _beatmapObjectPlacementHelper.BeatToPosition(_editorData.tailBeat);

            Vector2 start = new Vector2(
                (_editorData.column - 1.5f) * LaneWidth,
                YOffset + _editorData.row * LaneWidth
            );
            Vector2 end = new Vector2(
                (_editorData.tailColumn - 1.5f) * LaneWidth,
                YOffset + _editorData.tailRow * LaneWidth
            );
            Vector2 chainVector = end - start;
            float controlAngle =
                (_editorData.cutDirection.RotationAngle() - 90f) * (Mathf.PI / 180f);
            Vector2 controlPoint =
                0.5f
                * chainVector.magnitude
                * new Vector2(Mathf.Cos(controlAngle), Mathf.Sin(controlAngle));
            float squishAmount = _editorData.squishAmount;
            float zEnd = tailZ - headZ;

            foreach (ChainVisualRoots.LinkRoot link in _visualRoots.Links)
            {
                if (link.Element == null || link.Root == null)
                {
                    continue;
                }

                float fraction = (float)link.Element.linkId / (sliceCount - 1);
                BezierCurve(
                    Vector2.zero,
                    controlPoint,
                    chainVector,
                    fraction * squishAmount,
                    out Vector2 pos,
                    out Vector2 tangent
                );

                link.Root.localPosition = new Vector3(pos.x, pos.y, zEnd * fraction);
                link.Root.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    Vector2.SignedAngle(new Vector2(0f, -1f), tangent)
                );

                if (!link.Root.gameObject.activeSelf)
                {
                    link.Root.gameObject.SetActive(true);
                }
            }
        }

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

        public void Enable() { }

        public void Disable() { }

        public void Setup(BaseEditorData? editorData) { }

        public void ManualUpdate()
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.z = _beatmapObjectPlacementHelper.BeatToPosition(_editorData.beat);
            transform.localPosition = localPosition;
        }
    }
}
