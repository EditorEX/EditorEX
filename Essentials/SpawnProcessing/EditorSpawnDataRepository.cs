using BeatmapEditor3D.DataModels;
using System.Collections.Generic;

namespace EditorEX.Essentials.SpawnProcessing
{
    public class SpawnDataRepoData
    {
        public Dictionary<BaseEditorData?, EditorObjectSpawnData> SpawnDataAssociation = new Dictionary<BaseEditorData?, EditorObjectSpawnData>();
    }

    public static class EditorSpawnDataRepository
    {
        private static SpawnDataRepoData _repoData = new SpawnDataRepoData();

        public static void ClearAll()
        {
            _repoData = new SpawnDataRepoData();
        }

        public static void RemoveSpawnData(BaseEditorData? data)
        {
            if (_repoData.SpawnDataAssociation.ContainsKey(data))
            {
                _repoData.SpawnDataAssociation.Remove(data);
            }
        }

        public static EditorObjectSpawnData GetSpawnData(BaseEditorData? data)
        {
            if (!_repoData.SpawnDataAssociation.ContainsKey(data))
            {
                var spawnData = new EditorObjectSpawnData();
                spawnData.flipLineIndex = (data as BaseBeatmapObjectEditorData).column;
                spawnData.beforeJumpNoteLineLayer = (NoteLineLayer)(data as BaseBeatmapObjectEditorData).row;
                _repoData.SpawnDataAssociation[data] = spawnData;
            }
            return _repoData.SpawnDataAssociation[data];
        }
    }
}
