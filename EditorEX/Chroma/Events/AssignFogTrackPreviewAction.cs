using EditorEX.Essentials.PreviewState;
using Heck.Animation;

namespace EditorEX.Chroma.Events
{
    internal sealed class AssignFogTrackPreviewAction : IPreviewStateAction
    {
        private readonly EditorFogAnimatorV2 _fogAnimator;
        private readonly Track _track;
        private bool _active;

        public AssignFogTrackPreviewAction(EditorFogAnimatorV2 fogAnimator, Track track)
        {
            _fogAnimator = fogAnimator;
            _track = track;
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

            _fogAnimator.RestoreDefaults();
            _active = false;
        }

        public void Tick(float beat)
        {
            if (!_active)
            {
                return;
            }

            _fogAnimator.Apply(_track);
        }
    }
}
