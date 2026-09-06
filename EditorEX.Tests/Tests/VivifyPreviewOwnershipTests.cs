using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using EditorEX.Vivify.Events;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class VivifyPreviewOwnershipTests
    {
        [Fact]
        public void Same_id_conflicts()
        {
            Assert.True(VivifyPreviewOwnership.Conflicts(["cube"], ["cube"]));
        }

        [Fact]
        public void Different_ids_do_not_conflict()
        {
            Assert.False(VivifyPreviewOwnership.Conflicts(["cube"], ["wall"]));
        }

        [Fact]
        public void Destroy_array_conflicts_when_it_contains_the_id()
        {
            Assert.True(VivifyPreviewOwnership.Conflicts(["cube"], ["wall", "cube"]));
        }

        [Fact]
        public void Anonymous_empty_ids_do_not_conflict()
        {
            Assert.False(VivifyPreviewOwnership.Conflicts([], ["cube"]));
            Assert.False(VivifyPreviewOwnership.Conflicts([], []));
        }

        [Fact]
        public void Post_processing_end_is_from_plus_duration()
        {
            Assert.True(
                VivifyPreviewOwnership.TryPostProcessingExclusiveEnd(8f, 2f, out float to)
            );
            Assert.Equal(10f, to);
        }

        [Fact]
        public void Duration_zero_post_processing_is_not_registered()
        {
            Assert.False(
                VivifyPreviewOwnership.TryPostProcessingExclusiveEnd(4f, 0f, out float to)
            );
            Assert.Equal(4f, to);
        }

        [Fact]
        public void Same_material_and_property_conflict()
        {
            Assert.True(
                VivifyPreviewOwnership.Conflicts(
                    "assets/materials/note/glassnote.mat",
                    [1],
                    "assets/materials/note/glassnote.mat",
                    [1]
                )
            );
        }

        [Fact]
        public void Different_material_properties_do_not_conflict()
        {
            Assert.False(
                VivifyPreviewOwnership.Conflicts(
                    "assets/materials/note/glassnote.mat",
                    [1],
                    "assets/materials/note/glassnote.mat",
                    [2]
                )
            );
            Assert.False(
                VivifyPreviewOwnership.Conflicts(
                    "assets/materials/note/glassnote.mat",
                    [1],
                    "assets/materials/note/dropnote.mat",
                    [1]
                )
            );
        }

        [Fact]
        public void Shared_property_on_the_same_material_conflicts()
        {
            Assert.True(
                VivifyPreviewOwnership.Conflicts(
                    "assets/materials/note/glassnote.mat",
                    [1, 2],
                    "assets/materials/note/glassnote.mat",
                    [2, 3]
                )
            );
        }

        [Fact]
        public void Split_skybox_setup_keeps_light_when_zoom_conflicts()
        {
            var items = new List<(float Beat, string Asset, object Id)>
            {
                (0f, "introskybox", "_Zoom"),
                (0f, "introskybox", "_Light"),
                (1.25f, "introskybox", "_Zoom"),
            };

            float ExclusiveEnd(int index) =>
                PreviewStateOwnership.NextExclusiveEnd(
                    items,
                    index,
                    item => item.Beat,
                    (left, right) =>
                        VivifyPreviewOwnership.Conflicts(
                            left.Asset,
                            [left.Id],
                            right.Asset,
                            [right.Id]
                        )
                );

            Assert.Equal(1.25f, ExclusiveEnd(0));
            Assert.Equal(float.MaxValue, ExclusiveEnd(1));
        }

        [Fact]
        public void Destroyed_or_missing_material_is_not_written()
        {
            Assert.False(VivifyPreviewOwnership.CanWriteMaterial(null));
        }

        [Fact]
        public void Instant_material_events_write_once()
        {
            Assert.True(VivifyPreviewOwnership.NeedsMaterialWrite(0f, alreadyWritten: false));
            Assert.False(VivifyPreviewOwnership.NeedsMaterialWrite(0f, alreadyWritten: true));
        }

        [Fact]
        public void Animated_material_events_keep_writing()
        {
            Assert.True(VivifyPreviewOwnership.NeedsMaterialWrite(0.2f, alreadyWritten: true));
        }

        [Fact]
        public void Later_single_prefab_assignment_conflicts_on_the_same_key_and_track()
        {
            var track = new object();
            Assert.True(
                VivifyPreviewOwnership.ConflictsPrefabAssignment(
                    "note",
                    track,
                    "note",
                    track,
                    laterIsSingle: true
                )
            );
        }

        [Fact]
        public void Later_additive_prefab_assignment_does_not_end_the_earlier_owner()
        {
            var track = new object();
            Assert.False(
                VivifyPreviewOwnership.ConflictsPrefabAssignment(
                    "note",
                    track,
                    "note",
                    track,
                    laterIsSingle: false
                )
            );
        }

        [Fact]
        public void Prefab_assignments_on_different_keys_or_tracks_do_not_conflict()
        {
            var track = new object();
            Assert.False(
                VivifyPreviewOwnership.ConflictsPrefabAssignment(
                    "note",
                    track,
                    "bomb",
                    track,
                    laterIsSingle: true
                )
            );
            Assert.False(
                VivifyPreviewOwnership.ConflictsPrefabAssignment(
                    "note",
                    track,
                    "note",
                    new object(),
                    laterIsSingle: true
                )
            );
        }

        [Fact]
        public void Same_camera_channel_conflicts()
        {
            Assert.True(
                VivifyPreviewOwnership.ConflictsCamera(
                    "_Main",
                    ["depthTextureMode"],
                    "_Main",
                    ["depthTextureMode", "bloomPrePass"]
                )
            );
        }

        [Fact]
        public void Different_camera_ids_or_channels_do_not_conflict()
        {
            Assert.False(
                VivifyPreviewOwnership.ConflictsCamera(
                    "_Main",
                    ["depthTextureMode"],
                    "bloom",
                    ["depthTextureMode"]
                )
            );
            Assert.False(
                VivifyPreviewOwnership.ConflictsCamera(
                    "_Main",
                    ["depthTextureMode"],
                    "_Main",
                    ["bloomPrePass"]
                )
            );
        }

        [Fact]
        public void Same_global_or_animator_or_render_property_conflicts()
        {
            Assert.True(VivifyPreviewOwnership.ConflictsProperty(1, 1));
            Assert.False(VivifyPreviewOwnership.ConflictsProperty(1, 2));
            Assert.True(VivifyPreviewOwnership.ConflictsAnimator("cube", "_Zoom", "cube", "_Zoom"));
            Assert.False(VivifyPreviewOwnership.ConflictsAnimator("cube", "_Zoom", "cube", "_Cut"));
            Assert.True(VivifyPreviewOwnership.ConflictsSetting("fog", "fog"));
            Assert.False(VivifyPreviewOwnership.ConflictsSetting("fog", "fogColor"));
        }

        [Fact]
        public void Single_after_additive_ends_both_same_track_owners()
        {
            var track = new object();
            var items = new List<(float Beat, string Key, object Track, bool Single)>
            {
                (0f, "note", track, true),
                (4f, "note", track, false),
                (8f, "note", track, true),
            };

            float ExclusiveEnd(int index) =>
                PreviewStateOwnership.NextExclusiveEnd(
                    items,
                    index,
                    item => item.Beat,
                    (left, right) =>
                        VivifyPreviewOwnership.ConflictsPrefabAssignment(
                            left.Key,
                            left.Track,
                            right.Key,
                            right.Track,
                            right.Single
                        )
                );

            Assert.Equal(8f, ExclusiveEnd(0));
            Assert.Equal(8f, ExclusiveEnd(1));
            Assert.Equal(float.MaxValue, ExclusiveEnd(2));
        }
    }
}
