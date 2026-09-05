using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;

namespace EditorEX.NoodleExtensions.Patches
{
    internal static class EditorFakeNoteTick
    {
        internal static bool FrameHasTickableNote(
            BeatmapObjectsFrameDataContainer frame,
            EditorDeserializedData? noodle
        )
        {
            for (int column = 0; column <= 3; column++)
            {
                for (int row = 0; row <= 2; row++)
                {
                    if (
                        IsTickable(frame, column, row, BeatmapObjectGridType.Note, noodle)
                        || IsTickable(frame, column, row, BeatmapObjectGridType.ChainHead, noodle)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsTickable(
            BeatmapObjectsFrameDataContainer frame,
            int column,
            int row,
            BeatmapObjectGridType type,
            EditorDeserializedData? noodle
        )
        {
            if (!frame.TryGet(column, row, type, out BaseBeatmapObjectEditorData data))
            {
                return false;
            }

            return noodle == null
                || !noodle.Resolve(data, out EditorNoodleObjectData? noodleData)
                || noodleData?.Fake != true;
        }
    }
}
