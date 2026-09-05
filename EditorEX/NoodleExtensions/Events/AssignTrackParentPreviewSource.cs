using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using EditorEX.MapData.Contexts;
using EditorEX.NoodleExtensions.Animation;
using Heck.Animation;
using Heck.Animation.Transform;
using NoodleExtensions;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace EditorEX.NoodleExtensions.Events
{
    internal sealed class AssignTrackParentPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly bool _leftHanded;
        private readonly TransformControllerFactory _transformControllerFactory;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly HashSet<EditorParentObject> _parentObjects = new();

        private AssignTrackParentPreviewSource(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData deserializedData,
            [Inject(Id = "leftHanded")] bool leftHanded,
            TransformControllerFactory transformControllerFactory,
            ICustomDataRepository customDataRepository
        )
        {
            _editorDeserializedData = deserializedData;
            _leftHanded = leftHanded;
            _transformControllerFactory = transformControllerFactory;
            _customDataRepository = customDataRepository;
        }

        public void Build(IPreviewStateRegistry registry)
        {
            if (_editorDeserializedData == null)
            {
                return;
            }

            var items = new List<(float Beat, int Index, NoodleParentTrackEventData Data)>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != ASSIGN_TRACK_PARENT)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(
                        customEvent,
                        out NoodleParentTrackEventData? data
                    )
                    || data == null
                )
                {
                    continue;
                }

                items.Add((customEvent.beat, index++, data));
            }

            items.Sort(
                (a, b) =>
                {
                    int beat = a.Beat.CompareTo(b.Beat);
                    return beat != 0 ? beat : a.Index.CompareTo(b.Index);
                }
            );

            bool v2 = (MapContext.Version?.Major ?? 3) == 2;
            for (int i = 0; i < items.Count; i++)
            {
                float from = items[i].Beat;
                float to = PreviewStateOwnership.NextExclusiveEnd(
                    items,
                    i,
                    item => item.Beat,
                    (left, right) => Conflicts(left.Data, right.Data)
                );
                registry.Add(
                    from,
                    to,
                    new AssignTrackParentPreviewAction(
                        items[i].Data,
                        _leftHanded,
                        v2,
                        _transformControllerFactory,
                        _parentObjects
                    )
                );
            }
        }

        private static bool Conflicts(NoodleParentTrackEventData a, NoodleParentTrackEventData b)
        {
            if (a.ParentTrack == b.ParentTrack)
            {
                return true;
            }

            foreach (Track childA in a.ChildrenTracks)
            {
                foreach (Track childB in b.ChildrenTracks)
                {
                    if (childA == childB)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
