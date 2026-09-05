using System;
using System.Collections.Generic;
using IntervalTree;

namespace EditorEX.Essentials.PreviewState
{
    internal sealed class PreviewStateScheduler : IPreviewStateRegistry
    {
        private readonly IntervalTree<float, PreviewStateEntry> _tree = new();
        private readonly HashSet<PreviewStateEntry> _active = new();
        private readonly List<PreviewStateEntry> _removed = new();
        private readonly List<PreviewStateEntry> _added = new();
        private readonly List<PreviewStateEntry> _pendingExecuted = new();
        private readonly Action<Exception>? _onError;
        private int _order;
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

            foreach (PreviewStateEntry entry in _pendingExecuted)
            {
                if (beat >= entry.From && beat < entry.To)
                {
                    _active.Add(entry);
                }
                else
                {
                    Invoke(entry.Action.Reverse);
                }
            }

            _pendingExecuted.Clear();

            var current = new HashSet<PreviewStateEntry>();
            foreach (PreviewStateEntry entry in _tree.Query(beat))
            {
                if (beat >= entry.From && beat < entry.To)
                {
                    current.Add(entry);
                }
            }

            _removed.Clear();
            _added.Clear();
            foreach (PreviewStateEntry entry in _active)
            {
                if (!current.Contains(entry))
                {
                    _removed.Add(entry);
                }
            }

            foreach (PreviewStateEntry entry in current)
            {
                if (!_active.Contains(entry))
                {
                    _added.Add(entry);
                }
            }

            _removed.Sort(CompareDescending);
            _added.Sort(CompareAscending);

            foreach (PreviewStateEntry entry in _removed)
            {
                Invoke(entry.Action.Reverse);
            }

            foreach (PreviewStateEntry entry in _added)
            {
                Invoke(entry.Action.Execute);
            }

            foreach (PreviewStateEntry entry in current)
            {
                Invoke(() => entry.Action.Tick(beat));
            }

            _active.Clear();
            foreach (PreviewStateEntry entry in current)
            {
                _active.Add(entry);
            }
        }

        public void ReverseAll()
        {
            _removed.Clear();
            _removed.AddRange(_pendingExecuted);
            _removed.AddRange(_active);
            _pendingExecuted.Clear();
            _removed.Sort(CompareDescending);
            foreach (PreviewStateEntry entry in _removed)
            {
                Invoke(entry.Action.Reverse);
            }

            _active.Clear();
        }

        private void Invoke(Action call)
        {
            try
            {
                call();
            }
            catch (Exception e)
            {
                if (_onError == null)
                {
                    throw;
                }

                _onError(e);
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
        }
    }
}
