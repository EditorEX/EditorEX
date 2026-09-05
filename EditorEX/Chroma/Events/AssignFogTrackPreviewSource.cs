using System.Collections.Generic;
using Chroma;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using Heck.Animation;
using Zenject;
using static EditorEX.Chroma.Constants;

namespace EditorEX.Chroma.Events
{
    internal sealed class AssignFogTrackPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorFogAnimatorV2 _fogAnimator;

        private AssignFogTrackPreviewSource(
            [InjectOptional(Id = "Chroma")] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorFogAnimatorV2 fogAnimator
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _fogAnimator = fogAnimator;
        }

        public void Build(IPreviewStateRegistry registry)
        {
            if (_editorDeserializedData == null)
            {
                return;
            }

            var items = new List<(float Beat, int Index, Track Track)>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != ASSIGN_FOG_TRACK)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(
                        customEvent,
                        out ChromaAssignFogEventData? data
                    )
                    || data == null
                )
                {
                    continue;
                }

                items.Add((customEvent.beat, index++, data.Track));
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
                    (_, _) => true
                );
                registry.Add(
                    from,
                    to,
                    new AssignFogTrackPreviewAction(_fogAnimator, items[i].Track)
                );
            }
        }
    }
}
