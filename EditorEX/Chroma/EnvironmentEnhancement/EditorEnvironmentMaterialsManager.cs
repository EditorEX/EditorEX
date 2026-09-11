using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chroma.EnvironmentEnhancement;
using EditorEX.SDK.AddressableHelpers;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using Zenject;
using _AsyncOperation = UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>;

namespace EditorEX.Chroma.EnvironmentEnhancement;

// Based from https://github.com/Aeroluna/Heck
internal class EditorEnvironmentMaterialsManager : MonoBehaviour
{
    private Dictionary<ShaderType, Material>? _environmentMaterials;
    private SiraLog _log = null!;

    internal Dictionary<ShaderType, Material> EnvironmentMaterials
    {
        get
        {
            if (_environmentMaterials == null)
            {
                throw new InvalidOperationException("Environment materials not yet fetched!");
            }

            return _environmentMaterials;
        }
    }

    private IEnumerator Activate()
    {
        if (_environmentMaterials != null)
        {
            yield break;
        }

        string[] environments = ["BTSEnvironment", "BillieEnvironment", "InterscopeEnvironment"];
        IEnumerable<_AsyncOperation> loads = environments.Select(Load).ToArray();
        foreach (_AsyncOperation asyncOperationHandle in loads)
        {
            yield return asyncOperationHandle;
        }

        Material[] environmentMaterials = Resources.FindObjectsOfTypeAll<Material>();

        _environmentMaterials = new Dictionary<ShaderType, Material>();
        Save(ShaderType.BTSPillar, "BTSDarkEnvironmentWithHeightFog");
        Save(ShaderType.BillieWater, "WaterfallFalling");
        Save(ShaderType.WaterfallMirror, "WaterfallMirror");
        Save(ShaderType.InterscopeConcrete, "Concrete2");
        Save(ShaderType.InterscopeCar, "Car");

        foreach (string environment in environments)
        {
            SceneManager.UnloadSceneAsync(environment);
        }

        yield break;

        void Save(ShaderType key, string matName)
        {
            Material? material = environmentMaterials.FirstOrDefault(e => e.name == matName);
            if (material != null)
            {
                // must be copied because the original gets unloaded.
                material = new Material(material);

                _environmentMaterials[key] = material;
                _log.Trace($"Saving [{matName}] to [{key}]");
            }
            else
            {
                _log.Error($"Could not find [{matName}]");
            }
        }

        _AsyncOperation Load(string environmentName)
        {
            _log.Trace($"Loading environment [{environmentName}]");
            return Addressables.LoadSceneAsync(
                environmentName,
                LoadSceneMode.Additive,
                true,
                int.MaxValue
            );
        }
    }

    [Inject]
    private void Construct(SiraLog log)
    {
        _log = log;
    }

    private void Initialize()
    {
        StartCoroutine(Activate());
    }

    // Exists because AppInit.GetAppStartType check scenecount for some reason and we cannot load extra scenes during appinit
    // credit to meivyn for figuring out this bug
    internal class EditorEnvironmentMaterialsManagerInitializer : IInitializable
    {
        private readonly EditorEnvironmentMaterialsManager _environmentMaterialsManager;

        private EditorEnvironmentMaterialsManagerInitializer(
            EditorEnvironmentMaterialsManager environmentMaterialsManager
        )
        {
            _environmentMaterialsManager = environmentMaterialsManager;
        }

        public void Initialize()
        {
            _environmentMaterialsManager.Initialize();
        }
    }
}
