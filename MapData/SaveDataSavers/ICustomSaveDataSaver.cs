using BeatmapEditor3D.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.MapData.SaveDataSavers
{
    public interface ICustomSaveDataSaver
    {
        public bool IsVersion(Version version);
        public void Save(BeatmapProjectManager beatmapProjectManager, bool clearDirty);
    }
}
