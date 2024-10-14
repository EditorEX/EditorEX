using BeatmapEditor3D;
using EditorEX.SDK.ContextMenu;
using EditorEX.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.SDKImplementation
{
    public class DefaultEditorBeatmapListContextMenuProvider : ContextMenuProvider<BeatmapListContextMenuObject>
    {
        public override ContextOption<BeatmapListContextMenuObject>[] GetContextOptions()
        {
            return new ContextOption<BeatmapListContextMenuObject>[]
            {
                new ("Open Folder", OpenFolder),
                new ("Delete", Delete),
            };
        }

        private void OpenFolder(BeatmapListContextMenuObject contextObject)
        {
            FileUtil.OpenFileBrowser(contextObject.beatmapInfoData.beatmapFolderPath);
        }

        private void Delete(BeatmapListContextMenuObject contextObject)
        {
        }
    }
}
