using CustomJSONData.CustomBeatmap;
using System.Collections.Generic;

namespace EditorEX.CustomJSONData.Util
{
    public static class CustomBeatmapDataExtensions
    {
        public static void RemoveBeatmapCustomEventData(this CustomBeatmapData beatmapData, CustomEventData customEventData)
        {
            LinkedListNode<BeatmapDataItem> linkedListNode = beatmapData._allBeatmapDataItemToNodeMap[customEventData];
            beatmapData._beatmapDataItemsPerTypeAndId.RemoveItem(customEventData);
            beatmapData._allBeatmapData.Remove(linkedListNode);
            beatmapData._allBeatmapDataItemToNodeMap.Remove(customEventData);
        }
    }
}