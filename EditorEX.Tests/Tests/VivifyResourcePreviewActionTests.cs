using EditorEX.Vivify.Events;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class VivifyResourcePreviewActionTests
    {
        [Fact]
        public void Execute_is_idempotent_until_reversed()
        {
            int executes = 0;
            int reverses = 0;
            var action = new VivifyResourcePreviewAction(() => executes++, () => reverses++);

            action.Execute();
            action.Execute();
            action.Reverse();
            action.Execute();

            Assert.Equal(2, executes);
            Assert.Equal(1, reverses);
        }

        [Fact]
        public void Reverse_is_safe_before_execute()
        {
            int reverses = 0;
            var action = new VivifyResourcePreviewAction(() => { }, () => reverses++);

            action.Reverse();

            Assert.Equal(0, reverses);
        }

        [Fact]
        public void Tick_only_runs_while_active()
        {
            int ticks = 0;
            var action = new VivifyResourcePreviewAction(() => { }, () => { }, _ => ticks++);

            action.Tick(1f);
            action.Execute();
            action.Tick(2f);
            action.Reverse();
            action.Tick(3f);

            Assert.Equal(1, ticks);
        }
    }
}
