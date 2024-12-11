using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using System;
using System.Collections.Generic;

namespace EditorEX.MapData.LevelDataLoaders
{
    public static class LevelDataLoaderUtil
    {
        public static IEnumerable<U> GetEditorData<T, U>(IEnumerable<T> list, Func<T, BeatmapEditorRotationProcessor_v2, U> convert, BeatmapEditorRotationProcessor_v2 rotationProcessor) where U : BaseEditorData where T : ICustomData
        {
            foreach (T obj in list)
            {
                var customData = obj.customData;
                var editorData = convert(obj, rotationProcessor);
                CustomDataRepository.AddCustomData(editorData, customData);

                yield return editorData;
            }
        }

        public static IEnumerable<U> GetEditorData<T, U>(IEnumerable<T> list, Func<T, BeatmapEditorRotationProcessor_v3, U> convert, BeatmapEditorRotationProcessor_v3 rotationProcessor) where U : BaseEditorData where T : ICustomData
        {
            foreach (T obj in list)
            {
                var customData = obj.customData;
                var editorData = convert(obj, rotationProcessor);
                CustomDataRepository.AddCustomData(editorData, customData);

                yield return editorData;
            }
        }

        public static IEnumerable<U> GetEditorData<T, U>(IEnumerable<T> list, Func<T, U> convert) where U : BaseEditorData where T : ICustomData
        {
            foreach (T obj in list)
            {
                var customData = obj.customData;
                var editorData = convert(obj);
                CustomDataRepository.AddCustomData(editorData, customData);

                yield return editorData;
            }
        }
    }
}
