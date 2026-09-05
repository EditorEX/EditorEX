using EditorEX.Essentials.PreviewState;
using UnityEngine;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class PreviewOriginalPoseTests
    {
        [Fact]
        public void Position_is_unanimated_only_when_both_position_channels_are_null()
        {
            Assert.True(PreviewOriginalPose.PositionUnanimated(null, null));
            Assert.False(PreviewOriginalPose.PositionUnanimated(Vector3.zero, null));
            Assert.False(PreviewOriginalPose.PositionUnanimated(null, Vector3.one));
        }

        [Fact]
        public void Rotation_is_unanimated_only_when_both_rotation_channels_are_null()
        {
            Assert.True(PreviewOriginalPose.RotationUnanimated(null, null));
            Assert.False(
                PreviewOriginalPose.RotationUnanimated(new Quaternion(0f, 0f, 0f, 1f), null)
            );
        }

        [Fact]
        public void Restore_replaces_only_requested_channels()
        {
            var originalRot = new Quaternion(0f, 1f, 0f, 0f);
            var currentRot = new Quaternion(1f, 0f, 0f, 0f);
            var pose = new PreviewOriginalPose(
                new Vector3(1000f, 0f, 0f),
                originalRot,
                Vector3.one
            );

            (Vector3 pos, Quaternion rot, Vector3 scale) = pose.Restored(
                position: true,
                rotation: false,
                scale: false,
                currentPos: Vector3.zero,
                currentRot: currentRot,
                currentScale: new Vector3(2f, 2f, 2f)
            );

            Assert.Equal(new Vector3(1000f, 0f, 0f), pos);
            Assert.Equal(currentRot, rot);
            Assert.Equal(new Vector3(2f, 2f, 2f), scale);
        }

        [Fact]
        public void Spawn_channel_select_does_not_clobber_still_animated_position()
        {
            PreviewOriginalPose.SpawnChannels selected = PreviewOriginalPose.SelectSpawnChannels(
                restorePosition: false,
                restoreRotation: true,
                restoreScale: false,
                spawnPosition: new Vector3(1003.591f, 10.115f, -99934f),
                spawnLocalPosition: null,
                spawnRotation: new Quaternion(0f, 1f, 0f, 0f),
                spawnLocalRotation: null,
                spawnScale: new Vector3(0.89f, -0.145f, 0.80f)
            );

            Assert.Null(selected.Position);
            Assert.Null(selected.LocalPosition);
            Assert.Null(selected.Scale);
            Assert.Equal(new Quaternion(0f, 1f, 0f, 0f), selected.Rotation);
            Assert.Null(selected.LocalRotation);
        }

        [Fact]
        public void Spawn_channel_select_keeps_requested_world_and_local()
        {
            var spawnPos = new Vector3(1f, 2f, 3f);
            var spawnLocalPos = new Vector3(4f, 5f, 6f);
            var spawnScale = new Vector3(0.5f, 0.5f, 0.5f);

            PreviewOriginalPose.SpawnChannels selected = PreviewOriginalPose.SelectSpawnChannels(
                restorePosition: true,
                restoreRotation: false,
                restoreScale: true,
                spawnPosition: spawnPos,
                spawnLocalPosition: spawnLocalPos,
                spawnRotation: new Quaternion(0f, 1f, 0f, 0f),
                spawnLocalRotation: Quaternion.identity,
                spawnScale: spawnScale
            );

            Assert.Equal(spawnPos, selected.Position);
            Assert.Equal(spawnLocalPos, selected.LocalPosition);
            Assert.Equal(spawnScale, selected.Scale);
            Assert.Null(selected.Rotation);
            Assert.Null(selected.LocalRotation);
        }

        [Fact]
        public void Empty_spawn_json_falls_back_to_live_world_pose()
        {
            PreviewOriginalPose.SpawnChannels selected = PreviewOriginalPose.SelectSpawnChannels(
                restorePosition: true,
                restoreRotation: true,
                restoreScale: true,
                spawnPosition: null,
                spawnLocalPosition: null,
                spawnRotation: null,
                spawnLocalRotation: null,
                spawnScale: null
            );

            PreviewOriginalPose.SpawnChannels filled = PreviewOriginalPose.WithLiveFallback(
                selected,
                restorePosition: true,
                restoreRotation: true,
                restoreScale: true,
                liveWorldPos: Vector3.zero,
                liveWorldRot: Quaternion.identity,
                liveScale: Vector3.one
            );

            Assert.Equal(Vector3.zero, filled.Position);
            Assert.Null(filled.LocalPosition);
            Assert.Equal(Quaternion.identity, filled.Rotation);
            Assert.Null(filled.LocalRotation);
            Assert.Equal(Vector3.one, filled.Scale);
        }

        [Fact]
        public void Live_fallback_does_not_fill_unrequested_position()
        {
            PreviewOriginalPose.SpawnChannels selected = PreviewOriginalPose.SelectSpawnChannels(
                restorePosition: false,
                restoreRotation: true,
                restoreScale: false,
                spawnPosition: null,
                spawnLocalPosition: null,
                spawnRotation: null,
                spawnLocalRotation: null,
                spawnScale: null
            );

            PreviewOriginalPose.SpawnChannels filled = PreviewOriginalPose.WithLiveFallback(
                selected,
                restorePosition: false,
                restoreRotation: true,
                restoreScale: false,
                liveWorldPos: new Vector3(9f, 9f, 9f),
                liveWorldRot: Quaternion.identity,
                liveScale: Vector3.one
            );

            Assert.Null(filled.Position);
            Assert.Null(filled.LocalPosition);
            Assert.Null(filled.Scale);
            Assert.Equal(Quaternion.identity, filled.Rotation);
        }
    }
}
