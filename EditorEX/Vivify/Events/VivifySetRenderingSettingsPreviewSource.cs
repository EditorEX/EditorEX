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
    internal sealed class VivifySetRenderingSettingsPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorSetRenderingSettings _settings;

        private VivifySetRenderingSettingsPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorSetRenderingSettings settings
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _settings = settings;
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
                    RenderingSettingsProperty Property,
                    float Duration,
                    Functions Easing
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != SET_RENDERING_SETTINGS)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(
                        customEvent,
                        out SetRenderingSettingsData? data
                    )
                    || data == null
                )
                {
                    continue;
                }

                foreach (RenderingSettingsProperty property in data.Properties)
                {
                    items.Add((customEvent.beat, index++, property, data.Duration, data.Easing));
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
                        VivifyPreviewOwnership.ConflictsSetting(
                            left.Property.Name,
                            right.Property.Name
                        )
                );
                registry.Add(
                    from,
                    to,
                    new VivifySetRenderingSettingsPreviewAction(
                        _settings,
                        items[i].Property,
                        from,
                        items[i].Duration,
                        items[i].Easing
                    )
                );
            }
        }
    }
}
