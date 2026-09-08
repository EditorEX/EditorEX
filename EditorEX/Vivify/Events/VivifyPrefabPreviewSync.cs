using System;
using UnityEngine;
using Vivify.Controllers.Sync;

namespace EditorEX.Vivify.Events
{
    internal static class VivifyPrefabPreviewSync
    {
        public static bool NeedsReset(float previousElapsed, float elapsed)
        {
            return previousElapsed < 0f || elapsed + 0.0001f < previousElapsed;
        }

        public static float ElapsedSeconds(float startSeconds, float currentSeconds)
        {
            float elapsed = currentSeconds - startSeconds;
            return elapsed > 0f ? elapsed : 0f;
        }

        // Gameplay AnimatorSyncController seeks to the note's song time on spawn, then only
        // advances after AudioTimeSyncController.songTime passes it. Match that so approaching
        // notes show the hit pose instead of the clip's often-hidden t=0 frame.
        public static float NoteSeekSeconds(float currentSeconds, float noteSeconds)
        {
            return currentSeconds > noteSeconds ? currentSeconds : noteSeconds;
        }

        public static float AnimatorDelta(float previousElapsed, float elapsed)
        {
            if (NeedsReset(previousElapsed, elapsed))
            {
                return elapsed > 0f ? elapsed : 0f;
            }

            float delta = elapsed - previousElapsed;
            return delta > 0f ? delta : 0f;
        }

        public static bool ShouldRestartParticles(float previousElapsed, float elapsed)
        {
            return previousElapsed < 0f;
        }

        public static bool ShouldSimulateParticles(float previousElapsed, float elapsed)
        {
            return previousElapsed < 0f || elapsed > previousElapsed + 0.0001f;
        }

        public static void Apply(Targets targets, float previousElapsed, float elapsedSeconds)
        {
            bool reset = NeedsReset(previousElapsed, elapsedSeconds);
            float animatorDelta = AnimatorDelta(previousElapsed, elapsedSeconds);
            bool restartParticles = ShouldRestartParticles(previousElapsed, elapsedSeconds);
            bool simulateParticles = ShouldSimulateParticles(previousElapsed, elapsedSeconds);

            foreach (Animator animator in targets.Animators)
            {
                animator.enabled = true;
                if (reset)
                {
                    animator.Rebind();
                }

                animator.Update(animatorDelta);
                // PreviewStateManager does not Tick while paused at the same beat.
                // Leave Unity's clock off so the last seek holds.
                animator.enabled = false;
            }

            foreach (ParticleSystem particle in targets.Particles)
            {
                // Unity cannot step particles backward; leave the last pose on rewind.
                if (simulateParticles)
                {
                    particle.Simulate(animatorDelta, true, restartParticles, false);
                }

                particle.Pause(true);
            }

            foreach (SyncController sync in targets.Syncs)
            {
                sync.enabled = false;
            }
        }

        internal sealed class Targets
        {
            internal Animator[] Animators { get; private set; } = Array.Empty<Animator>();

            internal ParticleSystem[] Particles { get; private set; } =
                Array.Empty<ParticleSystem>();

            internal SyncController[] Syncs { get; private set; } = Array.Empty<SyncController>();

            internal void Capture(GameObject? instance)
            {
                if (instance == null)
                {
                    Clear();
                    return;
                }

                Animators = instance.GetComponentsInChildren<Animator>(true);
                Particles = RootParticleSystems(
                    instance.GetComponentsInChildren<ParticleSystem>(true)
                );
                Syncs = instance.GetComponentsInChildren<SyncController>(true);
            }

            internal void Clear()
            {
                Animators = Array.Empty<Animator>();
                Particles = Array.Empty<ParticleSystem>();
                Syncs = Array.Empty<SyncController>();
            }

            private static ParticleSystem[] RootParticleSystems(ParticleSystem[] particles)
            {
                int count = 0;
                for (int i = 0; i < particles.Length; i++)
                {
                    if (!IsNestedParticleSystem(particles[i]))
                    {
                        particles[count++] = particles[i];
                    }
                }

                if (count == particles.Length)
                {
                    return particles;
                }

                var roots = new ParticleSystem[count];
                Array.Copy(particles, roots, count);
                return roots;
            }

            private static bool IsNestedParticleSystem(ParticleSystem particle)
            {
                return particle.transform.parent != null
                    && particle.transform.parent.GetComponent<ParticleSystem>() != null;
            }
        }
    }
}
