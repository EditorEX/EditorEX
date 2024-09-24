using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterEditor.CustomJSONData.Util
{
    public static class CustomDataExtensions
    {
        public static CustomData GetCustomData(this BaseEditorData data)
        {
            return CustomDataRepository.GetCustomData(data);
        }
    }
}
