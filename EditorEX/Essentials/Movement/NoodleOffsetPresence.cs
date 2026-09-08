using System.Collections.Generic;
using Heck;
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

        internal static bool HasObjectOffset(
            AnimationObjectData? animation,
            IReadOnlyList<Track>? tracks
        )
        {
            if (
                animation?.LocalPosition != null
                || animation?.LocalRotation != null
                || animation?.LocalScale != null
                || animation?.LocalLocalRotation != null
                || animation?.LocalDissolve != null
                || animation?.LocalDissolveArrow != null
                || animation?.LocalCuttable != null
            )
            {
                return true;
            }

            if (tracks == null)
            {
                return false;
            }

            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];
                if (
                    HasChannel<Vector3>(track, NoodleController.OFFSET_POSITION)
                    || HasChannel<Quaternion>(track, NoodleController.OFFSET_ROTATION)
                    || HasChannel<Vector3>(track, HeckController.SCALE)
                    || HasChannel<Quaternion>(track, HeckController.LOCAL_ROTATION)
                    || HasChannel<float>(track, NoodleController.DISSOLVE)
                    || HasChannel<float>(track, NoodleController.DISSOLVE_ARROW)
                    || HasChannel<float>(track, NoodleController.INTERACTABLE)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasChannel<T>(Track track, string name)
            where T : struct
        {
            return track.FindProperty<T>(name) != null || track.FindPathProperty<T>(name) != null;
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
                if (HasChannel<float>(tracks[i], name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
