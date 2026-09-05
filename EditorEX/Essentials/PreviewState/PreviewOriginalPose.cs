using UnityEngine;

namespace EditorEX.Essentials.PreviewState
{
    internal readonly struct PreviewOriginalPose
    {
        private readonly Vector3 _localPosition;
        private readonly Quaternion _localRotation;
        private readonly Vector3 _localScale;

        public PreviewOriginalPose(
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale
        )
        {
            _localPosition = localPosition;
            _localRotation = localRotation;
            _localScale = localScale;
        }

        public static bool PositionUnanimated(Vector3? world, Vector3? local)
        {
            return world == null && local == null;
        }

        public static bool RotationUnanimated(Quaternion? world, Quaternion? local)
        {
            return world == null && local == null;
        }

        public static SpawnChannels SelectSpawnChannels(
            bool restorePosition,
            bool restoreRotation,
            bool restoreScale,
            Vector3? spawnPosition,
            Vector3? spawnLocalPosition,
            Quaternion? spawnRotation,
            Quaternion? spawnLocalRotation,
            Vector3? spawnScale
        )
        {
            return new SpawnChannels(
                restorePosition ? spawnPosition : null,
                restorePosition ? spawnLocalPosition : null,
                restoreRotation ? spawnRotation : null,
                restoreRotation ? spawnLocalRotation : null,
                restoreScale ? spawnScale : null
            );
        }

        public static SpawnChannels WithLiveFallback(
            SpawnChannels selected,
            bool restorePosition,
            bool restoreRotation,
            bool restoreScale,
            Vector3 liveWorldPos,
            Quaternion liveWorldRot,
            Vector3 liveScale
        )
        {
            Vector3? position = selected.Position;
            Vector3? localPosition = selected.LocalPosition;
            if (restorePosition && position == null && localPosition == null)
            {
                position = liveWorldPos;
            }

            Quaternion? rotation = selected.Rotation;
            Quaternion? localRotation = selected.LocalRotation;
            if (restoreRotation && rotation == null && localRotation == null)
            {
                rotation = liveWorldRot;
            }

            Vector3? scale = selected.Scale;
            if (restoreScale && scale == null)
            {
                scale = liveScale;
            }

            return new SpawnChannels(position, localPosition, rotation, localRotation, scale);
        }

        public (Vector3 Position, Quaternion Rotation, Vector3 Scale) Restored(
            bool position,
            bool rotation,
            bool scale,
            Vector3 currentPos,
            Quaternion currentRot,
            Vector3 currentScale
        )
        {
            return (
                position ? _localPosition : currentPos,
                rotation ? _localRotation : currentRot,
                scale ? _localScale : currentScale
            );
        }

        internal readonly struct SpawnChannels
        {
            public SpawnChannels(
                Vector3? position,
                Vector3? localPosition,
                Quaternion? rotation,
                Quaternion? localRotation,
                Vector3? scale
            )
            {
                Position = position;
                LocalPosition = localPosition;
                Rotation = rotation;
                LocalRotation = localRotation;
                Scale = scale;
            }

            public Vector3? Position { get; }

            public Vector3? LocalPosition { get; }

            public Quaternion? Rotation { get; }

            public Quaternion? LocalRotation { get; }

            public Vector3? Scale { get; }
        }
    }
}
