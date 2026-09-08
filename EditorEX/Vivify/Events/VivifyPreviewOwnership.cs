using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorEX.Vivify.Events
{
    internal enum VivifyObjectResourceKind
    {
        Instantiate,
        Destroy,
        Camera,
        Texture,
    }

    internal static class VivifyPreviewOwnership
    {
        public static bool CanWriteMaterial(Material? material) => material != null;

        public static bool CanWriteAnimator(Animator? animator) => animator != null;

        public static bool ConflictsObjectResource(
            IReadOnlyList<string> idsA,
            VivifyObjectResourceKind kindA,
            IReadOnlyList<string> idsB,
            VivifyObjectResourceKind kindB
        )
        {
            if (kindA == kindB)
            {
                return Conflicts(idsA, idsB);
            }

            bool prefabA =
                kindA == VivifyObjectResourceKind.Instantiate
                || kindA == VivifyObjectResourceKind.Destroy;
            bool prefabB =
                kindB == VivifyObjectResourceKind.Instantiate
                || kindB == VivifyObjectResourceKind.Destroy;
            return prefabA && prefabB && Conflicts(idsA, idsB);
        }

        public static float NextPrefabDestroy<T>(
            string prefabId,
            float fromBeat,
            IReadOnlyList<T> sorted,
            Func<T, float> beatOf,
            Func<T, IReadOnlyList<string>> idsOf
        )
        {
            float next = float.MaxValue;
            for (int i = 0; i < sorted.Count; i++)
            {
                float beat = beatOf(sorted[i]);
                if (beat < fromBeat || beat >= next)
                {
                    continue;
                }

                IReadOnlyList<string> ids = idsOf(sorted[i]);
                for (int j = 0; j < ids.Count; j++)
                {
                    if (ids[j] == prefabId)
                    {
                        next = beat;
                        break;
                    }
                }
            }

            return next;
        }

        public static bool Conflicts(IReadOnlyList<string> idsA, IReadOnlyList<string> idsB)
        {
            for (int i = 0; i < idsA.Count; i++)
            {
                for (int j = 0; j < idsB.Count; j++)
                {
                    if (idsA[i] == idsB[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool Conflicts(
            string assetA,
            IReadOnlyList<object> propertiesA,
            string assetB,
            IReadOnlyList<object> propertiesB
        )
        {
            if (assetA != assetB)
            {
                return false;
            }

            for (int i = 0; i < propertiesA.Count; i++)
            {
                for (int j = 0; j < propertiesB.Count; j++)
                {
                    if (Equals(propertiesA[i], propertiesB[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool NeedsMaterialWrite(float durationBeats, bool alreadyWritten)
        {
            return durationBeats > 0f || !alreadyWritten;
        }

        public static bool ConflictsPrefabAssignment(
            string keyA,
            object trackA,
            string keyB,
            object trackB,
            bool laterIsSingle
        )
        {
            return laterIsSingle && keyA == keyB && ReferenceEquals(trackA, trackB);
        }

        public static bool ConflictsCamera(
            string idA,
            IReadOnlyList<string> channelsA,
            string idB,
            IReadOnlyList<string> channelsB
        )
        {
            return idA == idB && Conflicts(channelsA, channelsB);
        }

        public static bool ConflictsProperty(object idA, object idB) => Equals(idA, idB);

        public static bool ConflictsAnimator(
            string prefabIdA,
            string nameA,
            string prefabIdB,
            string nameB
        )
        {
            return prefabIdA == prefabIdB && nameA == nameB;
        }

        public static bool ConflictsSetting(string nameA, string nameB) => nameA == nameB;

        public static List<string> CameraChannels(
            bool depthTextureMode,
            bool clearFlags,
            bool backgroundColor,
            bool culling,
            bool bloomPrePass,
            bool mainEffect
        )
        {
            var channels = new List<string>();
            if (depthTextureMode)
            {
                channels.Add("depthTextureMode");
            }

            if (clearFlags)
            {
                channels.Add("clearFlags");
            }

            if (backgroundColor)
            {
                channels.Add("backgroundColor");
            }

            if (culling)
            {
                channels.Add("culling");
            }

            if (bloomPrePass)
            {
                channels.Add("bloomPrePass");
            }

            if (mainEffect)
            {
                channels.Add("mainEffect");
            }

            return channels;
        }

        public static bool TryPostProcessingExclusiveEnd(
            float fromBeat,
            float durationBeats,
            out float toBeat
        )
        {
            if (durationBeats <= 0f)
            {
                toBeat = fromBeat;
                return false;
            }

            toBeat = fromBeat + durationBeats;
            return true;
        }
    }
}
