using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using Heck.Animation;
using Vivify;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifySetGlobalPropertyPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorSetGlobalProperty _setGlobalProperty;

        private VivifySetGlobalPropertyPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorSetGlobalProperty setGlobalProperty
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _setGlobalProperty = setGlobalProperty;
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
                    object PropertyId,
                    List<MaterialProperty> Properties,
                    float Duration,
                    Functions Easing
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != SET_GLOBAL_PROPERTY)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(customEvent, out SetGlobalPropertyData? data)
                    || data == null
                    || data.Properties.Count == 0
                )
                {
                    continue;
                }

                foreach (MaterialProperty property in data.Properties)
                {
                    items.Add(
                        (
                            customEvent.beat,
                            index++,
                            property.Id,
                            [property],
                            data.Duration,
                            data.Easing
                        )
                    );
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
                        VivifyPreviewOwnership.ConflictsProperty(left.PropertyId, right.PropertyId)
                );
                registry.Add(
                    from,
                    to,
                    new VivifySetGlobalPropertyPreviewAction(
                        items[i].Properties,
                        _setGlobalProperty,
                        from,
                        items[i].Duration,
                        items[i].Easing
                    )
                );
            }
        }
    }
}
