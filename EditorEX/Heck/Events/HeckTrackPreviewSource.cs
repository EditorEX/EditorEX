using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.EventData;
using Zenject;
using static EditorEX.Heck.Constants;

namespace EditorEX.Heck.Events
{
    internal sealed class HeckTrackPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;

        private HeckTrackPreviewSource(
            [InjectOptional(Id = "Heck")] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
        }

        public void Build(IPreviewStateRegistry registry)
        {
            if (_editorDeserializedData == null)
            {
                return;
            }

            var items =
                new List<(
                    float Beat,
                    int Index,
                    bool Path,
                    EditorCoroutineEventData Data,
                    EditorCoroutineEventData.CoroutineInfo Info
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                bool path;
                if (customEvent.eventType == ANIMATE_TRACK)
                {
                    path = false;
                }
                else if (customEvent.eventType == ASSIGN_PATH_ANIMATION)
                {
                    path = true;
                }
                else
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(
                        customEvent,
                        out EditorCoroutineEventData? data
                    )
                    || data == null
                )
                {
                    continue;
                }

                foreach (EditorCoroutineEventData.CoroutineInfo info in data.CoroutineInfos)
                {
                    items.Add((customEvent.beat, index++, path, data, info));
                }
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
                    (left, right) => Conflicts(left.Info, right.Info)
                );
                registry.Add(
                    from,
                    to,
                    new HeckTrackPreviewAction(items[i].Data, items[i].Info, from, items[i].Path)
                );
            }
        }

        internal static bool Conflicts(
            EditorCoroutineEventData.CoroutineInfo a,
            EditorCoroutineEventData.CoroutineInfo b
        )
        {
            return a.Track == b.Track && a.Property == b.Property;
        }
    }
}
