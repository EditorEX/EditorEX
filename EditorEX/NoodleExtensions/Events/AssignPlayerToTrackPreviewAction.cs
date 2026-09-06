using EditorEX.Essentials.PreviewState;
using Heck.Animation;
using NoodleExtensions.Managers;

namespace EditorEX.NoodleExtensions.Events
{
    internal sealed class AssignPlayerToTrackPreviewAction : IPreviewStateAction
    {
        private readonly EditorAssignPlayerToTrack _assignPlayerToTrack;
        private readonly PlayerObject _playerObject;
        private readonly Track _track;
        private readonly Track? _previous;
        private bool _active;

        public AssignPlayerToTrackPreviewAction(
            EditorAssignPlayerToTrack assignPlayerToTrack,
            PlayerObject playerObject,
            Track track,
            Track? previous
        )
        {
            _assignPlayerToTrack = assignPlayerToTrack;
            _playerObject = playerObject;
            _track = track;
            _previous = previous;
        }

        public void Execute()
        {
            if (_active)
            {
                return;
            }

            _assignPlayerToTrack.Assign(_playerObject, _track);
            _active = true;
        }

        public void Reverse()
        {
            if (!_active)
            {
                return;
            }

            _assignPlayerToTrack.Restore(_playerObject, _previous);
            _active = false;
        }

        public void Tick(float beat) { }
    }
}
