using BeatmapEditor3D.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.Essentials.Visuals
{
    internal class EditorBasicNoteVisuals : MonoBehaviour, IObjectVisuals
    {
        private GameObject _basicRoot;

        public void Init(BaseEditorData editorData)
        {
        }

        public void Enable()
        {
            _basicRoot?.SetActive(true);
        }

        public void Disable()
        {
            _basicRoot?.SetActive(false);
        }

        public void ManualUpdate()
        {

        }
    }
}
