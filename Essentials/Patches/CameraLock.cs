using BeatmapEditor3D.Controller;
using BeatmapEditor3D.DataModels.Events.Conversion;
using EditorEX.Chroma.Patches;
using EditorEX.Essentials.ViewMode;
using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.Essentials.Patches
{
    internal class CameraLock : IAffinity
    {
        private static readonly MethodInfo _oldPosition = AccessTools.PropertyGetter(typeof(Transform), "position");
        private static readonly MethodInfo _newLocalPosition = AccessTools.PropertyGetter(typeof(Transform), "localPosition");

        private static readonly MethodInfo _oldSetPosition = AccessTools.PropertySetter(typeof(Transform), "position");
        private static readonly MethodInfo _newSetLocalPosition = AccessTools.PropertySetter(typeof(Transform), "localPosition");

        private readonly ActiveViewMode _activeViewMode;

        private Vector3? toSetPosition;
        private Quaternion? toSetRotation;
        private Quaternion? toSetCameraRotation;

        private Quaternion? cameraPreviousRotation;
        private Vector3? movementPreviousPosition;
        private Quaternion? movementPreviousRotation;

        private CameraLock(
            ActiveViewMode activeViewMode)

        {
            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += ModeChanged;
        }

        private void ModeChanged()
        {
            if (_activeViewMode.Mode.LockCamera == _activeViewMode.LastMode.LockCamera) return;
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

        [AffinityPatch(typeof(BeatmapEditor360CameraController), nameof(BeatmapEditor360CameraController.Update))]
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
                __instance._mouseLook.LookRotation(__instance._uiCameraMovementTransform, __instance._uiCameraTransform);
                Vector3 vector = __instance._uiCameraMovementTransform.localPosition;
                Vector3 vector2 = Vector3.zero;
                if (Input.GetKey(KeyCode.W))
                {
                    vector2 = __instance._uiCameraTransform.forward;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    vector2 = -__instance._uiCameraTransform.forward;
                }
                vector2 = __instance.transform.parent.InverseTransformVector(vector2);

                Vector3 vector3 = Vector3.zero;
                if (Input.GetKey(KeyCode.D))
                {
                    vector3 = __instance._uiCameraTransform.right;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    vector3 = -__instance._uiCameraTransform.right;
                }
                vector3 = __instance.transform.parent.InverseTransformVector(vector3);

                Vector3 vector4 = Vector3.zero;
                if (Input.GetKey(KeyCode.Space))
                {
                    vector4 = __instance.transform.up;
                }
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    vector4 = -__instance.transform.up;
                }
                vector4 = __instance.transform.parent.InverseTransformVector(vector4);

                if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                {
                    __instance._currentMoveIntensity = __instance._fastMoveIntensity;
                }
                if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
                {
                    __instance._currentMoveIntensity = __instance._slowMoveIntensity;
                }
                if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
                {
                    __instance._currentMoveIntensity = __instance._defaultMoveIntensity;
                }
                vector += (vector2 + vector3 + vector4) * (__instance._currentMoveIntensity * Time.deltaTime);
                __instance._uiCameraMovementTransform.localPosition = vector;
            }

            return false;
        }

        [AffinityPatch(typeof(BeatmapEditor360CameraController), nameof(BeatmapEditor360CameraController.Update))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Callvirt, _oldPosition))
                .Set(OpCodes.Callvirt, _newLocalPosition)
                .MatchForward(true, new CodeMatch(OpCodes.Callvirt, _oldSetPosition))
                .Set(OpCodes.Callvirt, _newSetLocalPosition)
                .InstructionEnumeration();
            return result;
        }
    }
}
