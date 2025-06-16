using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapEditor3D;

namespace EditorEX.Util
{
    public static class DirectorySearchUtil
    {
        public static List<string> GetDirectoriesWithInfoDat(string parentDirectory)
        {
            var directoriesWithInfoDat = new List<string>();

            // Recursively search through the parent directory and subdirectories
            SearchDirectory(parentDirectory, directoriesWithInfoDat);

            return directoriesWithInfoDat;
        }

        private static void SearchDirectory(string directory, List<string> result)
        {
            try
            {
                // Check if the directory contains an Info.dat file
                if (File.Exists(Path.Combine(directory, "Info.dat")))
                {
                    result.Add(directory);
                }

                // Recursively search through subdirectories
                foreach (
                    var subDirectory in Directory
                        .GetDirectories(directory)
                        .Where(
                            (string path) =>
                                !BeatmapFileUtils.GetDirectoryName(path).StartsWith("~")
                        )
                )
                {
                    SearchDirectory(subDirectory, result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error accessing directory: {directory}. Exception: {ex.Message}"
                );
            }
        }
    }
}
