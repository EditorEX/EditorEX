using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Deserialize;
using NoodleExtensions;
using UnityEngine;
using static NoodleExtensions.NoodleObjectData;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.ObjectData
{
    internal class EditorNoodleObjectData : IObjectCustomData
    {
        internal AnimationObjectData? AnimationObject { get; }
        internal bool? Fake { get; }
        internal Quaternion? LocalRotationQuaternion { get; }
        internal float? Njs { get; }
        internal float? SpawnOffset { get; }
        internal float? StartY { get; }
        internal IReadOnlyList<Track>? Track { get; }
        internal bool? Uninteractable { get; }
        internal Quaternion? WorldRotationQuaternion { get; }
        internal float? ScaleX { get; }
        internal float? ScaleY { get; }
        internal float? ScaleZ { get; }
        internal float? InternalAheadTime { get; set; }
        internal Vector3 InternalEndPos { get; set; }
        internal Quaternion InternalLocalRotation { get; set; }
        internal Vector3 InternalMidPos { get; set; }
        internal Vector3 InternalNoteOffset { get; set; }
        internal Vector3 InternalStartPos { get; set; }
        internal Quaternion InternalWorldRotation { get; set; }
        internal float? StartX { get; private protected set; }

        internal float? GetTimeProperty()
        {
            IReadOnlyList<Track>? tracks = Track;

            if (tracks != null)
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    float? time = tracks[i].GetProperty<float>("time");
                    if (time.HasValue)
                    {
                        return time;
                    }
                }
            }

            return null;
        }

        internal EditorNoodleObjectData(EditorNoodleObjectData original)
        {
            WorldRotationQuaternion = original.WorldRotationQuaternion;
            LocalRotationQuaternion = original.LocalRotationQuaternion;
            Track = original.Track;
            AnimationObject = original.AnimationObject;
            Uninteractable = original.Uninteractable;
            Fake = original.Fake;
        }

        internal EditorNoodleObjectData(
            BaseEditorData? noteEditorData,
            CustomData customData,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2,
            bool leftHanded
        )
        {
            try
            {
                object? rotation = customData.Get<object>(
                    v2 ? HeckController.V2_ROTATION : NoodleController.WORLD_ROTATION
                );
                if (rotation != null)
                {
                    if (rotation is List<object> list)
                    {
                        List<float> rot = list.Select(Convert.ToSingle).ToList();
                        WorldRotationQuaternion = Quaternion
                            .Euler(rot[0], rot[1], rot[2])
                            .Mirror(leftHanded);
                    }
                    else
                    {
                        WorldRotationQuaternion = Quaternion
                            .Euler(0, Convert.ToSingle(rotation), 0)
                            .Mirror(leftHanded);
                    }
                }

                Fake = customData.Get<bool?>(
                    v2 ? NoodleController.V2_FAKE_NOTE : NoodleController.INTERNAL_FAKE_NOTE
                );

                LocalRotationQuaternion = customData
                    .GetQuaternion(
                        v2 ? HeckController.V2_LOCAL_ROTATION : HeckController.LOCAL_ROTATION
                    )
                    ?.Mirror(leftHanded);

                Track = customData.GetNullableTrackArray(beatmapTracks, v2)?.ToList();

                CustomData? animationData = customData.Get<CustomData>(
                    v2 ? HeckController.V2_ANIMATION : HeckController.ANIMATION
                );
                if (animationData != null)
                {
                    AnimationObject = new AnimationObjectData(
                        animationData.GetPointData<Vector3>(
                            v2 ? HeckController.V2_POSITION : NoodleController.OFFSET_POSITION,
                            pointDefinitions
                        ),
                        animationData.GetPointData<Quaternion>(
                            v2 ? HeckController.V2_ROTATION : NoodleController.OFFSET_ROTATION,
                            pointDefinitions
                        ),
                        animationData.GetPointData<Vector3>(
                            v2 ? HeckController.V2_SCALE : HeckController.SCALE,
                            pointDefinitions
                        ),
                        animationData.GetPointData<Quaternion>(
                            v2 ? HeckController.V2_LOCAL_ROTATION : HeckController.LOCAL_ROTATION,
                            pointDefinitions
                        ),
                        animationData.GetPointData<float>(
                            v2 ? NoodleController.V2_DISSOLVE : NoodleController.DISSOLVE,
                            pointDefinitions
                        ),
                        animationData.GetPointData<float>(
                            v2
                                ? NoodleController.V2_DISSOLVE_ARROW
                                : NoodleController.DISSOLVE_ARROW,
                            pointDefinitions
                        ),
                        animationData.GetPointData<float>(
                            v2 ? NoodleController.V2_CUTTABLE : NoodleController.INTERACTABLE,
                            pointDefinitions
                        ),
                        animationData.GetPointData<Vector3>(
                            v2
                                ? NoodleController.V2_DEFINITE_POSITION
                                : NoodleController.DEFINITE_POSITION,
                            pointDefinitions
                        )
                    );
                }

                Uninteractable = v2
                    ? !customData.Get<bool?>(NoodleController.V2_CUTTABLE)
                    : customData.Get<bool?>(NoodleController.UNINTERACTABLE);

                IEnumerable<float?>? position = customData
                    .GetNullableFloats(
                        v2 ? HeckController.V2_POSITION : NoodleController.NOTE_OFFSET
                    )
                    ?.ToList();
                StartX = position?.ElementAtOrDefault(0);
                StartY = position?.ElementAtOrDefault(1);

                if (!v2)
                {
                    IEnumerable<float?>? scale = customData
                        .GetNullableFloats(HeckController.SCALE)
                        ?.ToList();
                    ScaleX = scale?.ElementAtOrDefault(0);
                    ScaleY = scale?.ElementAtOrDefault(1);
                    ScaleZ = scale?.ElementAtOrDefault(2);
                }

                Njs = customData.Get<float?>(
                    v2 ? NoodleController.V2_NOTE_JUMP_SPEED : NoodleController.NOTE_JUMP_SPEED
                );
                SpawnOffset = customData.Get<float?>(
                    v2 ? NoodleController.V2_NOTE_SPAWN_OFFSET : NoodleController.NOTE_SPAWN_OFFSET
                );
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
