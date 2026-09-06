using System;
using EditorEX.Essentials.Visuals.Note;
using EditorEX.Vivify.Events;
using Heck.Animation;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class VivifyPrefabPreviewSyncTests
    {
        [Fact]
        public void First_tick_and_rewind_need_absolute_seek()
        {
            Assert.True(VivifyPrefabPreviewSync.NeedsReset(-1f, 0f));
            Assert.True(VivifyPrefabPreviewSync.NeedsReset(5f, 3f));
        }

        [Fact]
        public void Forward_tick_does_not_reset()
        {
            Assert.False(VivifyPrefabPreviewSync.NeedsReset(3f, 5f));
            Assert.False(VivifyPrefabPreviewSync.NeedsReset(5f, 5f));
        }

        [Fact]
        public void Held_playhead_applies_zero_animator_delta()
        {
            Assert.Equal(0f, VivifyPrefabPreviewSync.AnimatorDelta(5f, 5f));
        }

        [Fact]
        public void Animator_delta_is_absolute_on_reset_and_relative_forward()
        {
            Assert.Equal(0f, VivifyPrefabPreviewSync.AnimatorDelta(-1f, 0f));
            Assert.Equal(2f, VivifyPrefabPreviewSync.AnimatorDelta(-1f, 2f));
            Assert.Equal(2f, VivifyPrefabPreviewSync.AnimatorDelta(3f, 5f));
            Assert.Equal(3f, VivifyPrefabPreviewSync.AnimatorDelta(5f, 3f));
        }

        [Fact]
        public void Elapsed_clamps_before_start()
        {
            Assert.Equal(0f, VivifyPrefabPreviewSync.ElapsedSeconds(10f, 8f));
            Assert.Equal(2f, VivifyPrefabPreviewSync.ElapsedSeconds(10f, 12f));
        }

        [Fact]
        public void Particles_restart_only_on_first_tick()
        {
            Assert.True(VivifyPrefabPreviewSync.ShouldRestartParticles(-1f, 2f));
            Assert.False(VivifyPrefabPreviewSync.ShouldRestartParticles(5f, 3f));
            Assert.False(VivifyPrefabPreviewSync.ShouldRestartParticles(3f, 5f));
            Assert.False(VivifyPrefabPreviewSync.ShouldRestartParticles(5f, 5f));
        }

        [Fact]
        public void Particles_do_not_resimulate_on_rewind()
        {
            Assert.False(VivifyPrefabPreviewSync.ShouldSimulateParticles(5f, 3f));
            Assert.False(VivifyPrefabPreviewSync.ShouldSimulateParticles(5f, 5f));
            Assert.True(VivifyPrefabPreviewSync.ShouldSimulateParticles(-1f, 2f));
            Assert.True(VivifyPrefabPreviewSync.ShouldSimulateParticles(3f, 5f));
        }

        [Fact]
        public void Assigned_note_prefab_holds_the_hit_pose_until_the_playhead_passes_it()
        {
            Assert.Equal(16f, VivifyPrefabPreviewSync.NoteSeekSeconds(10f, 16f));
            Assert.Equal(20f, VivifyPrefabPreviewSync.NoteSeekSeconds(20f, 16f));
            Assert.Equal(0f, VivifyPrefabPreviewSync.NoteSeekSeconds(0f, 0f));
        }

        [Fact]
        public void Prefab_dictionary_changed_can_be_subscribed_when_the_event_is_internal()
        {
            var manager = new EditorVivifyNotePrefabManager();
            Action<Track> handler = _ => { };
            manager.Subscribe(manager.ColorNotePrefabs, handler);
            manager.Unsubscribe(manager.ColorNotePrefabs, handler);
        }
    }
}
