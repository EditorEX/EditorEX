using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.ObjectPrefab.Managers;
using Heck.Deserialize;
using Vivify;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    internal class EditorAssignObjectPrefab : IInitializable
    {
        private readonly EditorBeatmapObjectPrefabManager _beatmapObjectPrefabManager;
        private readonly EditorDeserializedData _editorDeserializedData;

        private EditorAssignObjectPrefab(
            [Inject(Id = ID)] EditorDeserializedData editorDeserializedData,
            EditorBeatmapObjectPrefabManager beatmapObjectPrefabManager
        )
        {
            _editorDeserializedData = editorDeserializedData;
            _beatmapObjectPrefabManager = beatmapObjectPrefabManager;
        }

        public void Initialize()
        {
            foreach (
                ICustomEventCustomData customEventCustomData in _editorDeserializedData
                    .CustomEventCustomDatas
                    .Values
            )
            {
                if (customEventCustomData is not AssignObjectPrefabData data)
                {
                    continue;
                }

                foreach ((string? key, AssignObjectPrefabData.IPrefabInfo value) in data.Assets)
                {
                    if (key == null || value is not AssignObjectPrefabData.ObjectPrefabInfo info)
                    {
                        continue;
                    }

                    string? asset = info.Asset;
                    if (!string.IsNullOrEmpty(asset))
                    {
                        _beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(asset!, 10);
                    }

                    string? anyDirectionAsset = info.AnyDirectionAsset;
                    if (!string.IsNullOrEmpty(anyDirectionAsset) && key == NOTE_PREFAB)
                    {
                        _beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(
                            anyDirectionAsset!,
                            10
                        );
                    }

                    break;
                }
            }
        }
    }
}
