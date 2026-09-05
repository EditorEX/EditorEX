using System;
using EditorEX.Essentials.PreviewState;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifyResourcePreviewAction : IPreviewStateAction
    {
        private readonly Action _execute;
        private readonly Action _reverse;
        private readonly Action<float>? _tick;
        private bool _active;

        public VivifyResourcePreviewAction(
            Action execute,
            Action reverse,
            Action<float>? tick = null
        )
        {
            _execute = execute;
            _reverse = reverse;
            _tick = tick;
        }

        public void Execute()
        {
            if (_active)
            {
                return;
            }

            _execute();
            _active = true;
        }

        public void Reverse()
        {
            if (!_active)
            {
                return;
            }

            _reverse();
            _active = false;
        }

        public void Tick(float beat)
        {
            if (!_active)
            {
                return;
            }

            _tick?.Invoke(beat);
        }
    }
}
