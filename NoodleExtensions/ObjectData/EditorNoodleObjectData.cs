using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using NoodleExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterEditor.NoodleExtensions.ObjectData
{
    internal class EditorNoodleObjectData : IObjectCustomData
    {
        internal Vector3 InternalStartPos { get; set; }

        internal Vector3 InternalMidPos { get; set; }

        internal Vector3 InternalEndPos { get; set; }

        internal Quaternion? WorldRotationQuaternion { get; }

        internal Quaternion? LocalRotationQuaternion { get; }

        internal List<Track> Track { get; }

        internal Quaternion InternalWorldRotation { get; set; }

        internal Quaternion InternalLocalRotation { get; set; }

        internal NoodleObjectData.AnimationObjectData AnimationObject { get; }

        internal Vector3 InternalNoteOffset { get; set; }

        internal bool? Uninteractable { get; }

        internal bool? Fake { get; }

        protected internal float? StartX { get; set; }

        internal float? StartY { get; }

        internal float? NJS { get; }

        internal float? SpawnOffset { get; }

        internal float? InternalAheadTime { get; set; }

        internal float? GetTimeProperty()
        {
            List<Track> track = Track;
            if (track == null)
            {
                return null;
            }
            return track.Select((Track n) => n.GetProperty<float>("time")).FirstOrDefault((float? n) => n != null);
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

        internal EditorNoodleObjectData(BaseEditorData noteEditorData, CustomData customData, Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> beatmapTracks, bool v2, bool leftHanded)
        {
            try
            {
                object rotation = customData.Get<object>(v2 ? "_rotation" : "worldRotation");
                if (rotation != null)
                {
                    List<object> list = rotation as List<object>;
                    if (list != null)
                    {
                        List<float> rot = list.Select(new Func<object, float>(Convert.ToSingle)).ToList();
                        WorldRotationQuaternion = new Quaternion?(Quaternion.Euler(rot[0], rot[1], rot[2]).Mirror(leftHanded));
                    }
                    else
                    {
                        WorldRotationQuaternion = new Quaternion?(Quaternion.Euler(0f, Convert.ToSingle(rotation), 0f).Mirror(leftHanded));
                    }
                }
                Fake = customData.Get<bool?>(v2 ? "_fake" : "NE_fake");
                Quaternion? quaternion = null;
                LocalRotationQuaternion = ((customData.GetQuaternion(v2 ? "_localRotation" : "localRotation") != null) ? new Quaternion?(quaternion.GetValueOrDefault().Mirror(leftHanded)) : null);
                IEnumerable<Track> nullableTrackArray = customData.GetNullableTrackArray(beatmapTracks, v2);
                Track = ((nullableTrackArray != null) ? nullableTrackArray.ToList() : null);
                CustomData animationData = customData.Get<CustomData>(v2 ? "_animation" : "animation");
                if (animationData != null)
                {
                    AnimationObject = new NoodleObjectData.AnimationObjectData(animationData.GetPointData<Vector3>(v2 ? "_position" : "offsetPosition", pointDefinitions), animationData.GetPointData<Quaternion>(v2 ? "_rotation" : "offsetWorldRotation", pointDefinitions), animationData.GetPointData<Vector3>(v2 ? "_scale" : "scale", pointDefinitions), animationData.GetPointData<Quaternion>(v2 ? "_localRotation" : "localRotation", pointDefinitions), animationData.GetPointData<float>(v2 ? "_dissolve" : "dissolve", pointDefinitions), animationData.GetPointData<float>(v2 ? "_dissolveArrow" : "dissolveArrow", pointDefinitions), animationData.GetPointData<float>(v2 ? "_interactable" : "interactable", pointDefinitions), animationData.GetPointData<Vector3>(v2 ? "_definitePosition" : "definitePosition", pointDefinitions));
                }
                Uninteractable = (v2 ? (!customData.Get<bool?>("_interactable")) : customData.Get<bool?>("uninteractable"));
                IEnumerable<float?> nullableFloats = customData.GetNullableFloats(v2 ? "_position" : "coordinates");
                IEnumerable<float?> position = ((nullableFloats != null) ? nullableFloats.ToList() : null);
                StartX = ((position != null) ? position.ElementAtOrDefault(0) : null);
                StartY = ((position != null) ? position.ElementAtOrDefault(1) : null);
                NJS = customData.Get<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed");
                SpawnOffset = customData.Get<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset");
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
        }
    }
}
