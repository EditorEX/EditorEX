using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using System;
using System.Collections.Generic;

namespace EditorEX.MapData.LevelDataLoaders
{
    public static class LevelDataLoaderUtil
    {
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
