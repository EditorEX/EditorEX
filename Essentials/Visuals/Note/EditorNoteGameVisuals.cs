using BeatmapEditor3D.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals
{
    internal class EditorNoteGameVisuals : MonoBehaviour, IObjectVisuals
    {
        private GameObject _gameRoot;

        private bool active;

        [Inject]
        private void Construct()
        {
            _gameRoot = Instantiate(transform.Find("NoteCube"), transform, false).gameObject;
            _gameRoot.name = "GamerNoterCuber";
            Disable();
        }

        public void Init(BaseEditorData editorData)
        {
            if  (active)
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
            _gameRoot?.SetActive(true);
            active = true;
        }

        public void Disable()
        {
            _gameRoot?.SetActive(false);
            active = false;
        }

        public void ManualUpdate()
        {

        }

        public GameObject GetVisualRoot()
        {
            return _gameRoot;
        }
    }
}
