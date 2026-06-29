using BeatmapEditor3D.DataModels;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals.ChainHead
{
    // Normal-mode chain visuals. The shared ChainVisualRoots gives every head/link a positioned root
    // with a "Basic" child slot holding the editor's original mesh; this provider just toggles those
    // slots (the game visuals owns the "Game" slots). On a mode switch the controller disables the
    // outgoing provider before enabling the incoming one, so basic and game never both show.
    internal class EditorChainHeadBasicVisuals : MonoBehaviour, IObjectVisuals
    {
        private ChainVisualRoots _visualRoots;
        private bool _active;

        [Inject]
        private void Construct()
        {
            _visualRoots = GetComponent<ChainVisualRoots>();
        }

        public void Init(BaseEditorData? editorData)
        {
            _visualRoots?.EnsureBuilt();
            ApplyVisibility();
        }

        public void Enable()
        {
            _active = true;
            ApplyVisibility();
        }

        public void Disable()
        {
            _active = false;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (_visualRoots == null)
            {
                return;
            }

            if (_visualRoots.HeadBasic != null)
            {
                _visualRoots.HeadBasic.gameObject.SetActive(_active);
            }
            foreach (ChainVisualRoots.LinkRoot link in _visualRoots.Links)
            {
                if (link.Basic != null)
                {
                    link.Basic.gameObject.SetActive(_active);
                }
            }
        }

        public void ManualUpdate() { }

        public GameObject GetVisualRoot()
        {
            return _visualRoots != null && _visualRoots.HeadBasic != null
                ? _visualRoots.HeadBasic.gameObject
                : gameObject;
        }
    }
}
