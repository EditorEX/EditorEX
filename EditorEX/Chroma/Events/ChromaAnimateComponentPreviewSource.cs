using System.Collections.Generic;
using Chroma;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using Heck.Animation;
using Zenject;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;
using static EditorEX.Chroma.Constants;

namespace EditorEX.Chroma.Events
{
    internal sealed class ChromaAnimateComponentPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;

        private ChromaAnimateComponentPreviewSource(
            [InjectOptional(Id = "Chroma")] EditorDeserializedData deserializedData,
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
                    Track Track,
                    string Component,
                    string Property,
                    PointDefinition<float> Points,
                    float Duration,
                    Functions Easing
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != ANIMATE_COMPONENT)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(
                        customEvent,
                        out ChromaAnimateComponentData? data
                    )
                    || data == null
                )
                {
                    continue;
                }

                foreach (
                    (
                        string componentName,
                        Dictionary<string, PointDefinition<float>?> properties
                    ) in data.CoroutineInfos
                )
                {
                    if (!IsKnownComponent(componentName))
                    {
                        continue;
                    }

                    foreach (Track track in data.Track)
                    {
                        foreach (KeyValuePair<string, PointDefinition<float>?> pair in properties)
                        {
                            if (pair.Value == null || !IsKnownProperty(componentName, pair.Key))
                            {
                                continue;
                            }

                            items.Add(
                                (
                                    customEvent.beat,
                                    index++,
                                    track,
                                    componentName,
                                    pair.Key,
                                    pair.Value,
                                    data.Duration,
                                    data.Easing
                                )
                            );
                        }
                    }
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
                    (left, right) =>
                        ChromaAnimateComponentOwnership.Conflicts(
                            left.Track,
                            left.Component,
                            left.Property,
                            right.Track,
                            right.Component,
                            right.Property
                        )
                );
                registry.Add(
                    from,
                    to,
                    new ChromaAnimateComponentPreviewAction(
                        items[i].Track,
                        items[i].Component,
                        items[i].Property,
                        items[i].Points,
                        from,
                        items[i].Duration,
                        items[i].Easing
                    )
                );
            }
        }

        internal static bool IsKnownComponent(string componentName)
        {
            return componentName is BLOOM_FOG_ENVIRONMENT or TUBE_BLOOM_PRE_PASS_LIGHT;
        }

        internal static bool IsKnownProperty(string componentName, string property)
        {
            return componentName switch
            {
                BLOOM_FOG_ENVIRONMENT => property
                    is ATTENUATION
                        or OFFSET
                        or HEIGHT_FOG_HEIGHT
                        or HEIGHT_FOG_STARTY,
                TUBE_BLOOM_PRE_PASS_LIGHT => property
                    is COLOR_ALPHA_MULTIPLIER
                        or BLOOM_FOG_INTENSITY_MULTIPLIER,
                _ => false,
            };
        }
    }
}
