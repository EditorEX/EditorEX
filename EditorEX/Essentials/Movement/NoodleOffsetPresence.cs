using System.Collections.Generic;
using Heck.Animation;
using NoodleExtensions;
using UnityEngine;
using static NoodleExtensions.NoodleObjectData;

namespace EditorEX.Essentials.Movement
{
    internal static class NoodleOffsetPresence
    {
        internal static bool HasDefinitePosition(
            AnimationObjectData? animation,
            IReadOnlyList<Track>? tracks
        )
        {
            if (animation?.LocalDefinitePosition != null)
            {
                return true;
            }

            return HasTrackPath<Vector3>(tracks, NoodleController.DEFINITE_POSITION);
        }

        internal static bool HasDissolve(
            AnimationObjectData? animation,
            IReadOnlyList<Track>? tracks
        )
        {
            if (animation?.LocalDissolve != null || animation?.LocalDissolveArrow != null)
            {
                return true;
            }

            return HasTrackFloat(tracks, NoodleController.DISSOLVE)
                || HasTrackFloat(tracks, NoodleController.DISSOLVE_ARROW);
        }

        private static bool HasTrackPath<T>(IReadOnlyList<Track>? tracks, string name)
            where T : struct
        {
            if (tracks == null)
            {
                return false;
            }

            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].FindPathProperty<T>(name) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasTrackFloat(IReadOnlyList<Track>? tracks, string name)
        {
            if (tracks == null)
            {
                return false;
            }

            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];
                if (
                    track.FindProperty<float>(name) != null
                    || track.FindPathProperty<float>(name) != null
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
