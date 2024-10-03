using BeatmapEditor3D.Controller;
using EditorEX.Essentials.ViewMode;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.Essentials.Patches
{
    internal class CameraLock : IAffinity
    {
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
                movementPreviousPosition = __instance._uiCameraMovementTransform.position;
                __instance._uiCameraMovementTransform.position = toSetPosition.Value;
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
            return !_activeViewMode.Mode.LockCamera;
        }
    }
}
