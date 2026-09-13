using System.Collections.Generic;
using HarmonyLib;
using Heck;
using Heck.Animation.Transform;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.Chroma.Patches.EnvironmentComponent;

// Based from https://github.com/Aeroluna/Heck
// This whole file effectively changes _rotZ and _posZ from directly affecting the coordinate to being an offset
internal class EditorTrackLaneRingOffset : IAffinity
{
    private readonly Dictionary<TrackLaneRing, Quaternion> _rotationOffsets = new();

    internal static void UpdatePosition(TrackLaneRing trackLaneRing)
    {
        trackLaneRing._positionOffset = trackLaneRing.transform.localPosition;
    }

    // 7 days...
    internal void CopyRing(TrackLaneRing originalRing, TrackLaneRing newRing)
    {
        if (_rotationOffsets.TryGetValue(originalRing, out Quaternion offset))
        {
            _rotationOffsets.Add(newRing, offset);
        }
    }

    internal void SetTransform(TrackLaneRing trackLaneRing, TransformData transformData)
    {
        if (transformData.Position.HasValue || transformData.LocalPosition.HasValue)
        {
            UpdatePosition(trackLaneRing);
            trackLaneRing._positionOffset = trackLaneRing.transform.localPosition;
            trackLaneRing._posZ = 0;
        }

        // ReSharper disable once InvertIf
        if (transformData.Rotation.HasValue || transformData.LocalRotation.HasValue)
        {
            UpdateRotation(trackLaneRing);
            trackLaneRing._rotZ = 0;
        }
    }

    internal void UpdateRotation(TrackLaneRing trackLaneRing)
    {
        _rotationOffsets[trackLaneRing] = trackLaneRing.transform.localRotation;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(TrackLaneRing), nameof(TrackLaneRing.FixedUpdateRing))]
    private bool TrackLaneRingFixedUpdateRing(
        float fixedDeltaTime,
        ref float ____prevRotZ,
        ref float ____rotZ,
        ref float ____prevPosZ,
        ref float ____posZ,
        float ____destRotZ,
        float ____rotationSpeed,
        float ____destPosZ,
        float ____moveSpeed
    )
    {
        ____prevRotZ = ____rotZ;
        ____rotZ = Mathf.Lerp(____rotZ, ____destRotZ, fixedDeltaTime * ____rotationSpeed);
        ____prevPosZ = ____posZ;
        ____posZ = Mathf.Lerp(____posZ, ____destPosZ, fixedDeltaTime * ____moveSpeed);

        return false;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(TrackLaneRing), nameof(TrackLaneRing.Init))]
    private void TrackLaneRingInit(ref float ____posZ, Vector3 position)
    {
        ____posZ = position.z;
    }

    // Vanilla LateUpdate skips LateUpdateRing while AudioTimeSyncController is Stopped.
    // The editor optionally injects GameCore's controller (always Stopped here); the editor
    // IAudioTimeSource cannot fill that field. Dest rotation still lerps, but transforms never apply.
    [AffinityPrefix]
    [AffinityPatch(typeof(TrackLaneRingsManager), nameof(TrackLaneRingsManager.LateUpdate))]
    private bool ApplyRingsWhileEditorAudioStopped(
        TrackLaneRing[] ____rings,
        TimeHelper ____timeHelper
    )
    {
        float interpolationFactor = ____timeHelper.InterpolationFactor;
        for (int i = 0; i < ____rings.Length; i++)
        {
            ____rings[i].LateUpdateRing(interpolationFactor);
        }

        return false;
    }

    [AffinityPatch(typeof(TrackLaneRing), nameof(TrackLaneRing.LateUpdateRing))]
    [AffinityPrefix]
    private bool ApplyOffset(
        TrackLaneRing __instance,
        float interpolationFactor,
        float ____prevRotZ,
        float ____rotZ,
        Vector3 ____positionOffset,
        float ____prevPosZ,
        float ____posZ,
        Transform ____transform
    )
    {
        if (!_rotationOffsets.TryGetValue(__instance, out Quaternion rotation))
        {
            rotation = Quaternion.identity;
        }

        float interpolatedZPos = ____prevPosZ + ((____posZ - ____prevPosZ) * interpolationFactor);
        Vector3 positionZOffset = rotation * Vector3.forward * interpolatedZPos;
        Vector3 pos = ____positionOffset + positionZOffset;

        float interpolatedZRot = ____prevRotZ + ((____rotZ - ____prevRotZ) * interpolationFactor);
        Quaternion rotationZOffset = Quaternion.AngleAxis(interpolatedZRot, Vector3.forward);
        Quaternion rot = rotation * rotationZOffset;

        ____transform.localRotation = rot;
        ____transform.localPosition = pos;

        return false;
    }
}
