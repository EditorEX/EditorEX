using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using UnityEngine;

namespace EditorEX.Essentials.Movement.ChainHead.MovementProvider
{
    public class EditorChainHeadGameMovement : MonoBehaviour, IObjectMovement
    {
        public void Disable()
        {
            throw new NotImplementedException();
        }

        public void Enable()
        {
            throw new NotImplementedException();
        }

        public void Init(BaseEditorData? editorData, IVariableMovementDataProvider variableMovementDataProvider, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot)
        {
            throw new NotImplementedException();
        }

        public void ManualUpdate()
        {
            throw new NotImplementedException();
        }

        public void Setup(BaseEditorData? editorData)
        {
            throw new NotImplementedException();
        }
    }
}