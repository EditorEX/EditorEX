using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.ContextMenu.Objects;
using EditorEX.Util;

namespace EditorEX.SDKImplementation
{
    public class DefaultEditorBeatmapListContextMenuProvider
        : ContextMenuProvider<BeatmapListContextMenuObject>
    {
        public override ContextOption<BeatmapListContextMenuObject>[] GetContextOptions()
        {
            return new ContextOption<BeatmapListContextMenuObject>[]
            {
                new("Open Folder", OpenFolder),
                new("Delete", Delete),
            };
        }

        private void OpenFolder(BeatmapListContextMenuObject contextObject)
        {
            FileUtil.OpenFileBrowser(contextObject.BeatmapInfoData.beatmapFolderPath);
        }

        private void Delete(BeatmapListContextMenuObject contextObject) { }
    }
}
