using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BeatmapEditor3D.Visuals;
using HarmonyLib;
using UnityEngine;

namespace EditorEX.Essentials.Visuals.ChainHead
{
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
            TeardownLinks();
        }
    }
}
