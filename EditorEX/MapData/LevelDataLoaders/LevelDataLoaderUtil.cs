using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;

namespace EditorEX.MapData.LevelDataLoaders
{
    public static class LevelDataLoaderUtil
    {
        public static IEnumerable<U> GetEditorData<T, U>(
            IEnumerable<T> list,
            Func<T, BeatmapEditorRotationProcessor_v2, U> convert,
            BeatmapEditorRotationProcessor_v2 rotationProcessor,
            ICustomDataRepository customDataRepository
        )
            where U : BaseEditorData
            where T : ICustomData
        {
            foreach (T obj in list)
            {
                var customData = obj.customData;
                var editorData = convert(obj, rotationProcessor);
                customDataRepository.AddCustomData(editorData, customData);

                yield return editorData;
            }
        }

        public static IEnumerable<U> GetEditorData<T, U>(
            IEnumerable<T> list,
            Func<T, BeatmapEditorRotationProcessor_v3, U> convert,
            BeatmapEditorRotationProcessor_v3 rotationProcessor,
            ICustomDataRepository customDataRepository
        )
            where U : BaseEditorData
            where T : ICustomData
        {
            foreach (T obj in list)
            {
                var customData = obj.customData;
                var editorData = convert(obj, rotationProcessor);
                customDataRepository.AddCustomData(editorData, customData);

                yield return editorData;
            }
        }

        public static IEnumerable<U> GetEditorData<T, U>(
            IEnumerable<T> list,
            Func<T, U> convert,
            ICustomDataRepository customDataRepository
        )
            where U : BaseEditorData
            where T : ICustomData
        {
            foreach (T obj in list)
            {
                var customData = obj.customData;
                var editorData = convert(obj);
                customDataRepository.AddCustomData(editorData, customData);

                yield return editorData;
            }
        }
    }
}
