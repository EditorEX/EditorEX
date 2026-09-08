using System;
using System.Collections.Generic;
using IntervalTree;

namespace EditorEX.Essentials.PreviewState
{
    internal sealed class PreviewStateScheduler : IPreviewStateRegistry
    {
        private readonly IntervalTree<float, PreviewStateEntry> _tree = new();
        private readonly HashSet<PreviewStateEntry> _active = new();
        private readonly List<PreviewStateEntry> _query = new();
        private readonly List<PreviewStateEntry> _removed = new();
        private readonly List<PreviewStateEntry> _added = new();
        private readonly List<PreviewStateEntry> _pendingExecuted = new();
        private readonly Action<Exception>? _onError;
        private int _order;
        private int _generation;
        private float _beat;
        private bool _hasApplied;

        public PreviewStateScheduler()
            : this(null) { }

        public PreviewStateScheduler(Action<Exception>? onError)
        {
            _onError = onError;
        }

        public void Add(
            float fromBeat,
            float toBeat,
            IPreviewStateAction action,
            bool alreadyExecuted = false
        )
        {
            if (!(fromBeat < toBeat))
            {
                return;
            }

            var entry = new PreviewStateEntry(fromBeat, toBeat, _order++, action);
            _tree.Add(fromBeat, toBeat, entry);

            if (!_hasApplied)
            {
                return;
            }

            // Defer until Apply so a DelayedStart loop can still look up newly
            // spawned objects before out-of-range Reverse runs.
            if (alreadyExecuted)
            {
                _pendingExecuted.Add(entry);
            }
        }

        public void Refresh()
        {
            if (!_hasApplied)
            {
                return;
            }

            Apply(_beat);
        }

        public void Apply(float beat)
        {
            _beat = beat;
            _hasApplied = true;

            for (int i = 0; i < _pendingExecuted.Count; i++)
            {
                PreviewStateEntry entry = _pendingExecuted[i];
                if (beat >= entry.From && beat < entry.To)
                {
                    _active.Add(entry);
                }
                else
                {
                    InvokeReverse(entry.Action);
                }
            }

            _pendingExecuted.Clear();

            _tree.Rebuild();
            _query.Clear();
            if (_tree.root != null)
            {
                QueryPointInto(_tree.root, beat, _query);
            }

            int generation = ++_generation;
            _removed.Clear();
            _added.Clear();

            for (int i = 0; i < _query.Count; i++)
            {
                PreviewStateEntry entry = _query[i];
                entry.Generation = generation;
                if (!_active.Contains(entry))
                {
                    _added.Add(entry);
                }
            }

            foreach (PreviewStateEntry entry in _active)
            {
                if (entry.Generation != generation)
                {
                    _removed.Add(entry);
                }
            }

            if (_removed.Count > 1)
            {
                _removed.Sort(CompareDescending);
            }

            if (_added.Count > 1)
            {
                _added.Sort(CompareAscending);
            }

            for (int i = 0; i < _removed.Count; i++)
            {
                PreviewStateEntry entry = _removed[i];
                _active.Remove(entry);
                InvokeReverse(entry.Action);
            }

            for (int i = 0; i < _added.Count; i++)
            {
                PreviewStateEntry entry = _added[i];
                _active.Add(entry);
                InvokeExecute(entry.Action);
            }

            for (int i = 0; i < _query.Count; i++)
            {
                InvokeTick(_query[i].Action, beat);
            }
        }

        public void ReverseAll()
        {
            _removed.Clear();
            _removed.AddRange(_pendingExecuted);
            _removed.AddRange(_active);
            _pendingExecuted.Clear();
            if (_removed.Count > 1)
            {
                _removed.Sort(CompareDescending);
            }

            for (int i = 0; i < _removed.Count; i++)
            {
                InvokeReverse(_removed[i].Action);
            }

            _active.Clear();
        }

        private void InvokeExecute(IPreviewStateAction action)
        {
            try
            {
                action.Execute();
            }
            catch (Exception e)
            {
                Handle(e);
            }
        }

        private void InvokeReverse(IPreviewStateAction action)
        {
            try
            {
                action.Reverse();
            }
            catch (Exception e)
            {
                Handle(e);
            }
        }

        private void InvokeTick(IPreviewStateAction action, float beat)
        {
            try
            {
                action.Tick(beat);
            }
            catch (Exception e)
            {
                Handle(e);
            }
        }

        private void Handle(Exception e)
        {
            if (_onError == null)
            {
                throw e;
            }

            _onError(e);
        }

        // Stock IntervalTreeNode.Query(value) allocates a List at every visited node
        // and AddRange-copies children. Walk once into a reused buffer. Half-open
        // [from, to) is applied here (stock Query is inclusive on both ends).
        private static void QueryPointInto(
            IntervalTreeNode<float, PreviewStateEntry> node,
            float beat,
            List<PreviewStateEntry> acc
        )
        {
            RangeValuePair<float, PreviewStateEntry>[]? items = node.items;
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    RangeValuePair<float, PreviewStateEntry> rv = items[i];
                    if (rv.From > beat)
                    {
                        break;
                    }

                    if (rv.From <= beat && beat < rv.To)
                    {
                        acc.Add(rv.Value);
                    }
                }
            }

            if (node.leftNode != null && beat < node.center)
            {
                QueryPointInto(node.leftNode, beat, acc);
            }
            else if (node.rightNode != null && beat > node.center)
            {
                QueryPointInto(node.rightNode, beat, acc);
            }
        }

        private static int CompareAscending(PreviewStateEntry a, PreviewStateEntry b)
        {
            int from = a.From.CompareTo(b.From);
            return from != 0 ? from : a.Order.CompareTo(b.Order);
        }

        private static int CompareDescending(PreviewStateEntry a, PreviewStateEntry b)
        {
            return CompareAscending(b, a);
        }

        private sealed class PreviewStateEntry
        {
            public PreviewStateEntry(float from, float to, int order, IPreviewStateAction action)
            {
                From = from;
                To = to;
                Order = order;
                Action = action;
            }

            public float From { get; }

            public float To { get; }

            public int Order { get; }

            public IPreviewStateAction Action { get; }

            public int Generation;
        }
    }
}
