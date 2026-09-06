using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using Vivify;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifySetCameraPropertyPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorSetCameraProperty _setCameraProperty;

        private VivifySetCameraPropertyPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorSetCameraProperty setCameraProperty
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _setCameraProperty = setCameraProperty;
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
                    string Id,
                    List<string> Channels,
                    CameraProperty Property
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != SET_CAMERA_PROPERTY)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(customEvent, out SetCameraPropertyData? data)
                    || data == null
                )
                {
                    continue;
                }

                List<string> channels = VivifyPreviewOwnership.CameraChannels(
                    data.Property.HasDepthTextureMode,
                    data.Property.HasClearFlags,
                    data.Property.HasBackgroundColor,
                    data.Property.HasCulling,
                    data.Property.HasBloomPrePass,
                    data.Property.HasMainEffect
                );
                if (channels.Count == 0)
                {
                    continue;
                }

                items.Add((customEvent.beat, index++, data.Id, channels, data.Property));
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
                        VivifyPreviewOwnership.ConflictsCamera(
                            left.Id,
                            left.Channels,
                            right.Id,
                            right.Channels
                        )
                );
                List<string> owned = items[i].Channels;
                CameraProperty property = items[i].Property;
                string id = items[i].Id;
                registry.Add(
                    from,
                    to,
                    new VivifyResourcePreviewAction(
                        () => _setCameraProperty.SetCameraProperties(id, property),
                        () => _setCameraProperty.ClearChannels(id, owned)
                    )
                );
            }
        }
    }
}
