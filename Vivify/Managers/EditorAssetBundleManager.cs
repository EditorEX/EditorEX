using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.Essentials.Patches;
using Heck;
using SiraUtil.Logging;
using UnityEngine;
using Object = UnityEngine.Object;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Managers;

public class EditorAssetBundleManager : IDisposable
{
    private readonly Dictionary<string, Object> _assets = new();
    private readonly SiraLog _log;
    private readonly AssetBundle? _mainBundle;

    private EditorAssetBundleManager(
        SiraLog log,
        PopulateBeatmap populateBeatmap)
    {
        _log = log;

        string path = Path.Combine(
            Path.GetDirectoryName(populateBeatmap._beatmapLevelDataModel.songFilePath)!,
            BUNDLE_FILE);

        if (!File.Exists(path))
        {
            _log.Error($"[{BUNDLE_FILE}] not found");
            return;
        }

        _mainBundle = AssetBundle.LoadFromFile(path);

        if (_mainBundle == null)
        {
            _log.Error($"Failed to load [{path}]");
            return;
        }

        string[] assetnames = _mainBundle.GetAllAssetNames();
        foreach (string name in assetnames)
        {
            Object asset = _mainBundle.LoadAsset(name);
            _assets.Add(name, asset);
        }
    }

    public void Dispose()
    {
        if (_mainBundle != null)
        {
            _mainBundle.Unload(true);
        }
    }

    internal bool TryGetAsset<T>(string assetName, out T? asset)
    {
        if (_assets.TryGetValue(assetName, out Object gameObject))
        {
            if (gameObject is T t)
            {
                asset = t;
                return true;
            }

            _log.Error($"Found {assetName}, but was null or not [{typeof(T).FullName}]");
        }
        else
        {
            _log.Error($"Could not find {typeof(T).FullName} [{assetName}]");
        }

        asset = default;
        return false;
    }
}