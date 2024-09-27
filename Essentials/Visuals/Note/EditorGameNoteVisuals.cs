using BeatmapEditor3D.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.Essentials.Visuals
{
    internal class EditorGameNoteVisuals : MonoBehaviour, IObjectVisuals
    {
        private GameObject _gameRoot;

        public void Init(BaseEditorData editorData)
        {
        }

        public void Enable()
        {
            _gameRoot?.SetActive(true);
        }

        public void Disable()
        {
            _gameRoot?.SetActive(false);
        }

        public void ManualUpdate()
        {

        }
    }
}
