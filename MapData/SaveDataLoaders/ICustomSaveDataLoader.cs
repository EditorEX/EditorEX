using System;

namespace EditorEX.MapData.SaveDataLoaders
{
    public interface ICustomSaveDataLoader
    {
        public bool IsVersion(Version version);
        public void Load(string projectPath);
    }
}
