﻿using BeatmapEditor3D;
using EditorEX.CustomDataModels;
using EditorEX.MapData.SaveDataSavers;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.CustomJSONData.Patches
{
    // We just copy every file instead of picking what files we need. This is to allow modded files without having to specify each one.
    internal class BeatmapFileUtilsPatch : IAffinity
    {
        private readonly SiraLog _siraLog;

        private BeatmapFileUtilsPatch(
            SiraLog siraLog)
        {
            _siraLog = siraLog;
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapFileUtils), nameof(BeatmapFileUtils.CopyBeatmapProject))]
        private bool CopyBeatmapProject(string sourceDirectoryPath, string destinationDirectoryPath, string songFilename, string coverImageFilename, string[] difficultyFilenames)
        {
            if (!new DirectoryInfo(sourceDirectoryPath).Exists)
            {
                _siraLog.Error("Source directory does not exist or could not be found: " + sourceDirectoryPath);
                return false;
            }
            if (Directory.Exists(destinationDirectoryPath))
            {
                _siraLog.Error("Destination directory already exists: " + destinationDirectoryPath);
                return false;
            }
            Directory.CreateDirectory(destinationDirectoryPath);

            CopyFilesRecursively(sourceDirectoryPath, destinationDirectoryPath);
            return false;
        }
    }
}