using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Events;
using Heck.Animation;
using Vivify;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifySetRenderingSettingsPreviewAction : IPreviewStateAction
    {
        private readonly EditorSetRenderingSettings _settings;
        private readonly RenderingSettingsProperty _property;
        private readonly float _fromBeat;
        private readonly float _durationBeats;
        private readonly Functions _easing;
        private bool _active;
        private bool _written;

        public VivifySetRenderingSettingsPreviewAction(
            EditorSetRenderingSettings settings,
            RenderingSettingsProperty property,
            float fromBeat,
            float durationBeats,
            Functions easing
        )
        {
            _settings = settings;
            _property = property;
            _fromBeat = fromBeat;
            _durationBeats = durationBeats;
            _easing = easing;
        }

        public void Execute()
        {
            if (_active)
            {
                return;
            }

            _active = true;
        }

        public void Reverse()
        {
            if (!_active)
            {
                return;
            }

            _settings.Reset(_property.Name);
            _written = false;
            _active = false;
        }

        public void Tick(float beat)
        {
            if (!_active || !VivifyPreviewOwnership.NeedsMaterialWrite(_durationBeats, _written))
            {
                return;
            }

            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat,
                _fromBeat,
                _durationBeats,
                repeat: 0,
                _easing,
                out _
            );
            _settings.ApplyAtProgress(_property, progress);
            _written = true;
        }
    }
}
