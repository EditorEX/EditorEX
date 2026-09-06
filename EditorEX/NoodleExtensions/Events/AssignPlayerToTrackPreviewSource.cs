using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Managers;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace EditorEX.NoodleExtensions.Events
{
    internal sealed class AssignPlayerToTrackPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorAssignPlayerToTrack _assignPlayerToTrack;

        private AssignPlayerToTrackPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorAssignPlayerToTrack assignPlayerToTrack
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _assignPlayerToTrack = assignPlayerToTrack;
        }

        public void Build(IPreviewStateRegistry registry)
        {
            if (_editorDeserializedData == null)
            {
                return;
            }

            var items = new List<(float Beat, int Index, PlayerObject Target, Track Track)>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != ASSIGN_PLAYER_TO_TRACK)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(
                        customEvent,
                        out NoodlePlayerTrackEventData? data
                    )
                    || data == null
                )
                {
                    continue;
                }

                items.Add((customEvent.beat, index++, data.PlayerObject, data.Track));
            }

            items.Sort(
                (a, b) =>
                {
                    int beat = a.Beat.CompareTo(b.Beat);
                    return beat != 0 ? beat : a.Index.CompareTo(b.Index);
                }
            );

            for (int i = 0; i < items.Count; i++)
            {
                float from = items[i].Beat;
                float to = PreviewStateOwnership.NextExclusiveEnd(
                    items,
                    i,
                    item => item.Beat,
                    (left, right) =>
                        AssignPlayerToTrackOwnership.Conflicts(left.Target, right.Target)
                );
                Track? previous = PreviewStateOwnership.PreviousConflicting(
                    items,
                    i,
                    (left, right) =>
                        AssignPlayerToTrackOwnership.Conflicts(left.Target, right.Target),
                    item => item.Track
                );
                registry.Add(
                    from,
                    to,
                    new AssignPlayerToTrackPreviewAction(
                        _assignPlayerToTrack,
                        items[i].Target,
                        items[i].Track,
                        previous
                    )
                );
            }
        }
    }
}
