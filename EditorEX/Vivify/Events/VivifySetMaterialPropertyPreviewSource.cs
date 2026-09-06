using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.Managers;
using Heck.Animation;
using UnityEngine;
using Vivify;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifySetMaterialPropertyPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorAssetBundleManager _assetBundleManager;
        private readonly EditorSetMaterialProperty _setMaterialProperty;

        private VivifySetMaterialPropertyPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorAssetBundleManager assetBundleManager,
            EditorSetMaterialProperty setMaterialProperty
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _assetBundleManager = assetBundleManager;
            _setMaterialProperty = setMaterialProperty;
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
                    string Asset,
                    List<object> PropertyIds,
                    Material Material,
                    List<MaterialProperty> Properties,
                    float Duration,
                    Functions Easing
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != SET_MATERIAL_PROPERTY)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(customEvent, out SetMaterialPropertyData? data)
                    || data == null
                    || !_assetBundleManager.TryGetAsset(data.Asset, out Material? material)
                    || material == null
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
                            data.Asset,
                            [property.Id],
                            material,
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
                        VivifyPreviewOwnership.Conflicts(
                            left.Asset,
                            left.PropertyIds,
                            right.Asset,
                            right.PropertyIds
                        )
                );
                registry.Add(
                    from,
                    to,
                    new VivifySetMaterialPropertyPreviewAction(
                        items[i].Material,
                        items[i].Properties,
                        _setMaterialProperty,
                        from,
                        items[i].Duration,
                        items[i].Easing
                    )
                );
            }
        }
    }
}
