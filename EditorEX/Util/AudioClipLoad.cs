using System.IO;
using UnityEngine;

namespace EditorEX.Util
{
    public static class AudioClipLoad
    {
        public static AudioClip? AssignNameOrNull(AudioClip? clip, string filePath)
        {
            if (clip == null)
            {
                return null;
            }

            clip.name = Path.GetFileName(filePath);
            return clip;
        }

        public static string? ResolveExistingFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            return filePath;
        }

        public static string? ResolveFromLevel(
            string projectPath,
            string? songFilename,
            string? songFilePath
        )
        {
            string? fromModel = ResolveExistingFile(songFilePath);
            if (fromModel != null)
            {
                return fromModel;
            }

            if (string.IsNullOrEmpty(songFilename))
            {
                return null;
            }

            return ResolveExistingFile(Path.Combine(projectPath, songFilename));
        }
    }
}
