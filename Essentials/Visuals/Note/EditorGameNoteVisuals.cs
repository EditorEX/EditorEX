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
    internal class EditorGameNoteVisuals : MonoBehaviour, IObjectVisuals
    {
        private GameObject _gameRoot;

        [Inject]
        private void Construct()
        {
            _gameRoot = Instantiate(transform.parent.Find("NoteCube"), transform.parent, false).gameObject;
            _gameRoot.name = "GamerNoterCuber";
        }

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
