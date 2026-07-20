using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.Controller;
using EditorEX.Essentials.Features.ViewMode;
using HarmonyLib;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.Essentials.Patches
{
    internal class CameraLock : IAffinity
    {
        private static readonly MethodInfo _oldPosition = AccessTools.PropertyGetter(
            typeof(Transform),
            "position"
        );
        private static readonly MethodInfo _newLocalPosition = AccessTools.PropertyGetter(
            typeof(Transform),
            "localPosition"
        );

        private static readonly MethodInfo _oldSetPosition = AccessTools.PropertySetter(
            typeof(Transform),
            "position"
        );
        private static readonly MethodInfo _newSetLocalPosition = AccessTools.PropertySetter(
            typeof(Transform),
            "localPosition"
        );

        private readonly ActiveViewMode _activeViewMode;

        private Vector3? toSetPosition;
        private Quaternion? toSetRotation;
        private Quaternion? toSetCameraRotation;

        private Quaternion? cameraPreviousRotation;
        private Vector3? movementPreviousPosition;
        private Quaternion? movementPreviousRotation;

        private CameraLock(ActiveViewMode activeViewMode)
        {
            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += ModeChanged;
        }

        private void ModeChanged()
        {
            if (_activeViewMode.Mode.LockCamera == _activeViewMode.LastMode.LockCamera)
                return;
            if (_activeViewMode.Mode.LockCamera)
            {
                toSetPosition = new Vector3(0f, 1.8f, 0f);
                toSetRotation = Quaternion.identity;
                toSetCameraRotation = Quaternion.identity;
            }
            else
            {
                toSetPosition = movementPreviousPosition;
                toSetRotation = movementPreviousRotation;
                toSetCameraRotation = cameraPreviousRotation;
            }
        }

        [AffinityPatch(
            typeof(BeatmapEditor360CameraController),
            nameof(BeatmapEditor360CameraController.Update)
        )]
        [AffinityPrefix]
        private bool CameraControllerUpdate(BeatmapEditor360CameraController __instance)
        {
            if (toSetPosition.HasValue)
            {
                movementPreviousPosition = __instance._uiCameraMovementTransform.localPosition;
                __instance._uiCameraMovementTransform.localPosition = toSetPosition.Value;
                toSetPosition = null;
            }
            if (toSetCameraRotation.HasValue)
            {
                cameraPreviousRotation = __instance._uiCameraTransform.transform.localRotation;
                __instance._uiCameraTransform.transform.localRotation = toSetCameraRotation.Value;
                toSetCameraRotation = null;
            }
            if (toSetRotation.HasValue)
            {
                movementPreviousRotation = __instance._uiCameraMovementTransform.localRotation;
                __instance._uiCameraMovementTransform.localRotation = toSetRotation.Value;
                toSetRotation = null;
            }

            if (!_activeViewMode.Mode.LockCamera)
            {
                if (!__instance._beatmapState.cameraMoving)
                {
                    return false;
                }
                __instance._mouseLook.LookRotation(
                    __instance._uiCameraMovementTransform,
                    __instance._uiCameraTransform
                );
                Vector3 vector = __instance._uiCameraMovementTransform.localPosition;
                Vector3 vector2 = Vector3.zero;
                if (
                    __instance._movementDirection.HasFlag(
                        BeatmapEditor360CameraController.Direction.Forward
                    )
                )
                {
                    vector2 = __instance._uiCameraTransform.forward;
                }
                if (
                    __instance._movementDirection.HasFlag(
                        BeatmapEditor360CameraController.Direction.Backward
                    )
                )
                {
                    vector2 = -__instance._uiCameraTransform.forward;
                }
                vector2 = __instance.transform.parent.InverseTransformVector(vector2);

                Vector3 vector3 = Vector3.zero;
                if (
                    __instance._movementDirection.HasFlag(
                        BeatmapEditor360CameraController.Direction.Right
                    )
                )
                {
                    vector3 = __instance._uiCameraTransform.right;
                }
                if (
                    __instance._movementDirection.HasFlag(
                        BeatmapEditor360CameraController.Direction.Left
                    )
                )
                {
                    vector3 = -__instance._uiCameraTransform.right;
                }
                vector3 = __instance.transform.parent.InverseTransformVector(vector3);

                Vector3 vector4 = Vector3.zero;
                if (
                    __instance._movementDirection.HasFlag(
                        BeatmapEditor360CameraController.Direction.Up
                    )
                )
                {
                    vector4 = __instance.transform.up;
                }
                if (
                    __instance._movementDirection.HasFlag(
                        BeatmapEditor360CameraController.Direction.Down
                    )
                )
                {
                    vector4 = -__instance.transform.up;
                }
                vector4 = __instance.transform.parent.InverseTransformVector(vector4);

                if (__instance._movementIntensityIncreased)
                {
                    __instance._currentMoveIntensity = __instance._fastMoveIntensity;
                }
                else if (__instance._movementIntensityDecreased)
                {
                    __instance._currentMoveIntensity = __instance._slowMoveIntensity;
                }
                else
                {
                    __instance._currentMoveIntensity = __instance._defaultMoveIntensity;
                }
                vector +=
                    (vector2 + vector3 + vector4)
                    * (__instance._currentMoveIntensity * Time.deltaTime);
                __instance._uiCameraMovementTransform.localPosition = vector;
            }

            return false;
        }
    }
}
