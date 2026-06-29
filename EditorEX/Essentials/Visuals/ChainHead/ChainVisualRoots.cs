using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BeatmapEditor3D.Visuals;
using HarmonyLib;
using UnityEngine;

namespace EditorEX.Essentials.Visuals.ChainHead
{
    // Shared visual-root structure for a chain. For the head and each link we build a positioned ROOT
    // object whose children are the original editor mesh ("Basic") and the gameplay clone ("Game"):
    //
    //   ChainHeadRoot (child of the chain root; rotated by the movement, position inherited from the
    //                  chain root which the jump/basic movement drives)
    //     +-- Basic  (the editor's head mesh, reparented here)
    //     +-- Game   (EditorChainHeadGameVisuals clones the gameplay head cube here)
    //   ChainLinkRoot_i (child of the chain root; positioned + rotated by the movement)
    //     +-- Basic  (the editor's ChainElementNoteView, reparented here)
    //     +-- Game   (the gameplay link clone)
    //
    // The movement drives the roots; the basic/game visuals each toggle (and, for game, populate) their
    // own child. Roots are rebuilt when the chain's link set changes. The editor pools its native
    // objects and reparents them on retime, so teardown reparents them back to the chain root before
    // destroying a root (otherwise Destroy would take the pooled editor object with it).
    internal class ChainVisualRoots : MonoBehaviour
    {
        private static readonly FieldInfo _headNoteTransformField = AccessTools.Field(
            typeof(ChainNoteView),
            "_headNoteTransform"
        );
        private static readonly FieldInfo _chainElementsField = AccessTools.Field(
            typeof(ChainNoteView),
            "_chainElements"
        );

        private const string _basicChildName = "EditorEXBasic";
        private const string _gameChildName = "EditorEXGame";

        public readonly struct LinkRoot
        {
            public LinkRoot(
                ChainElementNoteView element,
                Transform root,
                Transform basic,
                Transform game
            )
            {
                Element = element;
                Root = root;
                Basic = basic;
                Game = game;
            }

            public ChainElementNoteView Element { get; }
            public Transform Root { get; }
            public Transform Basic { get; }
            public Transform Game { get; }
        }

        public Transform HeadRoot { get; private set; }
        public Transform HeadBasic { get; private set; }
        public Transform HeadGame { get; private set; }

        private readonly List<LinkRoot> _links = new();
        public IReadOnlyList<LinkRoot> Links => _links;

        private ChainNoteView _chainView;
        private Transform _headEditorMesh;

        // (Re)build the head root and reconcile the per-link roots against the chain's current link set.
        // Returns false if the chain view isn't ready.
        public bool EnsureBuilt()
        {
            if (_chainView == null)
            {
                _chainView = GetComponent<ChainNoteView>();
            }
            if (_chainView == null)
            {
                return false;
            }

            BuildHead();
            ReconcileLinks();

            // The head mesh is the head note transform itself; keep it parented under Basic and
            // neutralised so the head root owns rotation. The editor pools its objects and rewrites
            // _headNoteTransform's parent/rotation on its own Init (object respawn), so re-attach and
            // re-zero it every build to avoid a doubled rotation or the mesh escaping to the chain root.
            if (_headEditorMesh != null && HeadBasic != null)
            {
                if (_headEditorMesh.parent != HeadBasic)
                {
                    _headEditorMesh.SetParent(HeadBasic, false);
                }
                _headEditorMesh.localPosition = Vector3.zero;
                _headEditorMesh.localRotation = Quaternion.identity;
            }

            return HeadRoot != null;
        }

        private void BuildHead()
        {
            if (HeadRoot != null)
            {
                return;
            }

            var headNoteTransform = (Transform)_headNoteTransformField.GetValue(_chainView);
            if (headNoteTransform == null)
            {
                return;
            }

            HeadRoot = NewRoot("ChainHeadRoot");
            HeadBasic = NewChild(_basicChildName, HeadRoot);
            HeadGame = NewChild(_gameChildName, HeadRoot);

            // The head note transform *is* the editor's head mesh (a direct child of the note view).
            // Reparent it under Basic so the head root owns rotation and the mesh is just a child.
            _headEditorMesh = headNoteTransform;
            headNoteTransform.SetParent(HeadBasic, false);
            headNoteTransform.localPosition = Vector3.zero;
            headNoteTransform.localRotation = Quaternion.identity;
        }

        private void ReconcileLinks()
        {
            if (_chainElementsField.GetValue(_chainView) is not IEnumerable elements)
            {
                return;
            }

            var current = new List<ChainElementNoteView>();
            foreach (ChainElementNoteView element in elements)
            {
                if (element != null)
                {
                    current.Add(element);
                }
            }

            bool sameSet = _links.Count == current.Count;
            if (sameSet)
            {
                for (int i = 0; i < current.Count; i++)
                {
                    if (_links[i].Element != current[i])
                    {
                        sameSet = false;
                        break;
                    }
                }
            }

            if (!sameSet)
            {
                TeardownLinks();

                foreach (ChainElementNoteView element in current)
                {
                    Transform root = NewRoot("ChainLinkRoot");
                    Transform basic = NewChild(_basicChildName, root);
                    Transform game = NewChild(_gameChildName, root);
                    _links.Add(new LinkRoot(element, root, basic, game));
                }
            }

            // (Re)attach each editor element under its Basic slot and neutralise its local transform.
            // The editor pools and reparents/repositions these on its own re-spawns, so even with an
            // unchanged set they can escape back to the chain (head) origin between inits; this snaps
            // them back under their positioned root.
            foreach (LinkRoot link in _links)
            {
                if (link.Element == null)
                {
                    continue;
                }
                if (link.Element.transform.parent != link.Basic)
                {
                    link.Element.transform.SetParent(link.Basic, false);
                }
                link.Element.transform.localPosition = Vector3.zero;
                link.Element.transform.localRotation = Quaternion.identity;
            }
        }

        private void TeardownLinks()
        {
            foreach (LinkRoot link in _links)
            {
                // Pooling guard: hand back whatever editor objects are *physically* under our Basic slot
                // right now (live children), NOT the stored element reference. The element pool can have
                // reassigned that instance to another chain, in which case it's no longer our child and
                // must not be touched -- otherwise we'd steal it and duplicate links. Anything still under
                // Basic genuinely belongs to us, so move it back to the chain root before destroying the
                // root (so Destroy doesn't take a pooled object with it).
                if (link.Basic != null && _chainView != null)
                {
                    for (int i = link.Basic.childCount - 1; i >= 0; i--)
                    {
                        link.Basic.GetChild(i).SetParent(_chainView.transform, false);
                    }
                }
                if (link.Root != null)
                {
                    Destroy(link.Root.gameObject);
                }
            }
            _links.Clear();
        }

        private Transform NewRoot(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }

        private static Transform NewChild(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }

        protected void OnDestroy()
        {
            // Return any pooled editor elements before this object (and our roots) are torn down.
            TeardownLinks();
        }
    }
}
