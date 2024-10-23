using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Zenject;

namespace EditorEX.CustomDataModels
{
    public class CustomPlatformsListModel : IInitializable
    {
        public IReadOnlyList<CustomPlatformInfo> CustomPlatforms => _customPlatforms;

        private List<CustomPlatformInfo> _customPlatforms = new List<CustomPlatformInfo>();

        public void Initialize()
        {
            var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "CustomPlatforms"));
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Extension.IndexOf("plat", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _customPlatforms.Add(new CustomPlatformInfo(fileInfo));
                }
            }
        }

        public class CustomPlatformInfo
        {
            public CustomPlatformInfo(FileInfo fileInfo)
            {
                FilePath = Path.GetFileNameWithoutExtension(fileInfo.Name);

                using var md5 = MD5.Create();
                using Stream stream = File.OpenRead(fileInfo.FullName);

                var hashBytes = md5.ComputeHash(stream);
                var sb = new StringBuilder();

                for (var i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2").ToLower());

                Hash = sb.ToString();
            }

            public CustomPlatformInfo(string filePath, string hash)
            {
                FilePath = filePath;
                Hash = hash;
            }

            public string FilePath { get; set; }
            public string Hash { get; set; }

            public static CustomPlatformInfo DeserializeV2(CustomData customData)
            {
                return new CustomPlatformInfo(customData.Get<string>("_customEnvironment") ?? "", customData.Get<string>("_customEnvironmentHash") ?? "");
            }

            public static CustomPlatformInfo DeserializeV4(CustomData customData)
            {
                return new CustomPlatformInfo(customData.Get<string>("customEnvironment") ?? "", customData.Get<string>("customEnvironmentHash") ?? "");
            }

            public static void SerializeV2(CustomData customData, CustomPlatformInfo customPlatformInfo)
            {
                customData["_customEnvironment"] = customPlatformInfo.FilePath;
                customData["_customEnvironmentHash"] = customPlatformInfo.Hash;
            }

            public static void SerializeV4(CustomData customData, CustomPlatformInfo customPlatformInfo)
            {
                customData["customEnvironment"] = customPlatformInfo.FilePath;
                customData["customEnvironmentHash"] = customPlatformInfo.Hash;
            }
        }
    }
}
