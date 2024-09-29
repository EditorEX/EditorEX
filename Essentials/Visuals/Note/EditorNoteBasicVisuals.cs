using BeatmapEditor3D.DataModels;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals
{
    internal class EditorNoteBasicVisuals : MonoBehaviour, IObjectVisuals
    {
        private GameObject _basicRoot;

        private bool active;

        [Inject]
        private void Construct()
        {
            _basicRoot = transform?.Find("NoteCube")?.gameObject ?? gameObject;
            Disable();
        }

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
