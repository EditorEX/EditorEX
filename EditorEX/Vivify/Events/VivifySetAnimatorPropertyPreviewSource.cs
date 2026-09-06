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
    internal sealed class VivifySetAnimatorPropertyPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorSetAnimatorProperty _setAnimatorProperty;

        private VivifySetAnimatorPropertyPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorSetAnimatorProperty setAnimatorProperty
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _setAnimatorProperty = setAnimatorProperty;
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
                    string PrefabId,
                    string Name,
                    List<AnimatorProperty> Properties,
                    float Duration,
                    Functions Easing
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != SET_ANIMATOR_PROPERTY)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(customEvent, out SetAnimatorPropertyData? data)
                    || data == null
                    || data.Properties.Count == 0
                )
                {
                    continue;
                }

                foreach (AnimatorProperty property in data.Properties)
                {
                    items.Add(
                        (
                            customEvent.beat,
                            index++,
                            data.Id,
                            property.Name,
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
                        VivifyPreviewOwnership.ConflictsAnimator(
                            left.PrefabId,
                            left.Name,
                            right.PrefabId,
                            right.Name
                        )
                );
                registry.Add(
                    from,
                    to,
                    new VivifySetAnimatorPropertyPreviewAction(
                        items[i].PrefabId,
                        items[i].Properties,
                        _setAnimatorProperty,
                        from,
                        items[i].Duration,
                        items[i].Easing
                    )
                );
            }
        }
    }
}
