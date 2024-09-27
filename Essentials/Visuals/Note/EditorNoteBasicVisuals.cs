using BeatmapEditor3D.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.Essentials.Visuals
{
    internal class EditorNoteBasicVisuals : MonoBehaviour, IObjectVisuals
    {
        private GameObject _basicRoot;

        private bool active;

        public void Init(BaseEditorData editorData)
        {
            if (active)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        public void Enable()
        {
            _basicRoot?.SetActive(true);
            active = true;
        }

        public void Disable()
        {
            _basicRoot?.SetActive(false);
            active = false;
        }

        public void ManualUpdate()
        {

        }

        public GameObject GetVisualRoot()
        {
            return _basicRoot;
        }
    }
}
