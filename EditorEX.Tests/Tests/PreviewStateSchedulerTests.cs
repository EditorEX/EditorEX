using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class PreviewStateSchedulerTests
    {
        [Fact]
        public void Apply_executes_then_ticks_newly_active_action()
        {
            var log = new List<string>();
            var scheduler = new PreviewStateScheduler();
            scheduler.Add(0f, 10f, new RecordingAction("A", log));

            scheduler.Apply(5f);

            Assert.Equal(new[] { "E:A", "T:A@5" }, log);
        }

        [Fact]
        public void Apply_at_exclusive_end_reverses_previous_and_executes_next()
        {
            var log = new List<string>();
            var scheduler = new PreviewStateScheduler();
            scheduler.Add(0f, 10f, new RecordingAction("A", log));
            scheduler.Add(10f, float.MaxValue, new RecordingAction("B", log));

            scheduler.Apply(5f);
            log.Clear();
            scheduler.Apply(10f);

            Assert.Equal(new[] { "R:A", "E:B", "T:B@10" }, log);
        }

        [Fact]
        public void Apply_backward_reexecutes_previous_owner()
        {
            var log = new List<string>();
            var scheduler = new PreviewStateScheduler();
            scheduler.Add(0f, 10f, new RecordingAction("A", log));
            scheduler.Add(10f, float.MaxValue, new RecordingAction("B", log));

            scheduler.Apply(12f);
            log.Clear();
            scheduler.Apply(5f);

            Assert.Equal(new[] { "R:B", "E:A", "T:A@5" }, log);
        }

        [Fact]
        public void Add_skips_empty_interval()
        {
            var log = new List<string>();
            var scheduler = new PreviewStateScheduler();
            scheduler.Add(10f, 10f, new RecordingAction("Skip", log));
            scheduler.Add(10f, float.MaxValue, new RecordingAction("Keep", log));

            scheduler.Apply(10f);

            Assert.Equal(new[] { "E:Keep", "T:Keep@10" }, log);
        }

        [Fact]
        public void Same_from_reverses_later_order_first()
        {
            var log = new List<string>();
            var scheduler = new PreviewStateScheduler();
            scheduler.Add(0f, float.MaxValue, new RecordingAction("A", log));
            scheduler.Add(0f, float.MaxValue, new RecordingAction("B", log));

            scheduler.Apply(1f);
            log.Clear();
            scheduler.Apply(-1f);

            Assert.Equal(new[] { "R:B", "R:A" }, log);
        }

        [Fact]
        public void ReverseAll_reverses_remaining_actives()
        {
            var log = new List<string>();
            var scheduler = new PreviewStateScheduler();
            scheduler.Add(0f, float.MaxValue, new RecordingAction("A", log));
            scheduler.Add(5f, float.MaxValue, new RecordingAction("B", log));

            scheduler.Apply(8f);
            log.Clear();
            scheduler.ReverseAll();

            Assert.Equal(new[] { "R:B", "R:A" }, log);
        }

        [Fact]
        public void Late_already_executed_add_in_range_joins_active_without_execute()
        {
            var log = new List<string>();
            var action = new RecordingAction("G", log);
            var scheduler = new PreviewStateScheduler();
            scheduler.Apply(16f);
            action.Execute();
            log.Clear();

            scheduler.Add(0f, float.MaxValue, action, true);
            scheduler.Apply(16f);

            Assert.Equal(new[] { "T:G@16" }, log);
        }

        [Fact]
        public void Late_already_executed_add_out_of_range_reverses_on_apply()
        {
            var log = new List<string>();
            var action = new RecordingAction("G", log);
            var scheduler = new PreviewStateScheduler();
            scheduler.Apply(5f);
            action.Execute();
            log.Clear();

            scheduler.Add(10f, 20f, action, true);
            scheduler.Apply(5f);

            Assert.Equal(new[] { "R:G" }, log);
        }

        private sealed class RecordingAction : IPreviewStateAction
        {
            private readonly string _name;
            private readonly List<string> _log;

            public RecordingAction(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void Execute() => _log.Add($"E:{_name}");

            public void Reverse() => _log.Add($"R:{_name}");

            public void Tick(float beat) => _log.Add($"T:{_name}@{beat}");
        }
    }
}
