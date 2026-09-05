using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using UnityEngine;
using static Heck.HeckController;

namespace EditorEX.Essentials.PreviewState
{
    internal sealed class PreviewOriginalTransform : MonoBehaviour
    {
        private TransformData _spawnData;
        private bool _leftHanded;
        private bool _v2;

        [SerializeField]
        private Vector3 _livePosition;

        [SerializeField]
        private Quaternion _liveRotation;

        [SerializeField]
        private Vector3 _liveScale;
        private bool _captured;

        public void Capture(TransformData spawnData, bool leftHanded, bool v2)
        {
            _spawnData = spawnData;
            _leftHanded = leftHanded;
            _v2 = v2;
            // AssignTrackParent often has empty TransformData; snapshot the pose
            // after Apply so Reverse can put the object back.
            _livePosition = transform.position;
            _liveRotation = transform.rotation;
            _liveScale = transform.localScale;
            _captured = true;
        }

        public void Restore(bool position, bool rotation, bool scale)
        {
            if (!_captured || (!position && !rotation && !scale))
            {
                return;
            }

            PreviewOriginalPose.SpawnChannels selected = PreviewOriginalPose.SelectSpawnChannels(
                restorePosition: position,
                restoreRotation: rotation,
                restoreScale: scale,
                spawnPosition: _spawnData.Position,
                spawnLocalPosition: _spawnData.LocalPosition,
                spawnRotation: _spawnData.Rotation,
                spawnLocalRotation: _spawnData.LocalRotation,
                spawnScale: _spawnData.Scale
            );

            Vector3? spawnScale = selected.Scale;
            Vector3? spawnPosition = selected.Position;
            Vector3? spawnLocalPosition = selected.LocalPosition;
            Quaternion? spawnRotation = selected.Rotation;
            Quaternion? spawnLocalRotation = selected.LocalRotation;

            if (_v2)
            {
                if (spawnPosition.HasValue)
                {
                    spawnPosition = spawnPosition.Value * 0.6f;
                }

                if (spawnLocalPosition.HasValue)
                {
                    spawnLocalPosition = spawnLocalPosition.Value * 0.6f;
                }
            }

            if (_leftHanded)
            {
                spawnScale = spawnScale?.Mirror();
                spawnPosition = spawnPosition?.Mirror();
                spawnRotation = spawnRotation?.Mirror();
                spawnLocalPosition = spawnLocalPosition?.Mirror();
                spawnLocalRotation = spawnLocalRotation?.Mirror();
            }

            if (spawnScale.HasValue)
            {
                transform.localScale = spawnScale.Value;
            }

            if (spawnLocalPosition.HasValue)
            {
                transform.localPosition = spawnLocalPosition.Value;
            }
            else if (spawnPosition.HasValue)
            {
                transform.position = spawnPosition.Value;
            }

            if (spawnLocalRotation.HasValue)
            {
                transform.localRotation = spawnLocalRotation.Value;
            }
            else if (spawnRotation.HasValue)
            {
                transform.rotation = spawnRotation.Value;
            }

            PreviewOriginalPose.SpawnChannels filled = PreviewOriginalPose.WithLiveFallback(
                selected,
                restorePosition: position,
                restoreRotation: rotation,
                restoreScale: scale,
                liveWorldPos: _livePosition,
                liveWorldRot: _liveRotation,
                liveScale: _liveScale
            );

            if (filled.Scale.HasValue && !selected.Scale.HasValue)
            {
                transform.localScale = filled.Scale.Value;
            }

            if (
                filled.Position.HasValue
                && !selected.Position.HasValue
                && !selected.LocalPosition.HasValue
            )
            {
                transform.position = filled.Position.Value;
            }

            if (
                filled.Rotation.HasValue
                && !selected.Rotation.HasValue
                && !selected.LocalRotation.HasValue
            )
            {
                transform.rotation = filled.Rotation.Value;
            }
        }

        public static void RestoreUnanimated(Track track)
        {
            bool position = PreviewOriginalPose.PositionUnanimated(
                track.GetProperty<Vector3>(POSITION),
                track.GetProperty<Vector3>(LOCAL_POSITION)
            );
            bool rotation = PreviewOriginalPose.RotationUnanimated(
                track.GetProperty<Quaternion>(ROTATION),
                track.GetProperty<Quaternion>(LOCAL_ROTATION)
            );
            bool scale = track.GetProperty<Vector3>(SCALE) == null;
            if (!position && !rotation && !scale)
            {
                return;
            }

            foreach (GameObject gameObject in track.GameObjects)
            {
                if (gameObject == null)
                {
                    continue;
                }

                gameObject
                    .GetComponent<PreviewOriginalTransform>()
                    ?.Restore(position, rotation, scale);
            }
        }
    }
}
