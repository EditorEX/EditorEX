using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Zenject;

namespace EditorEX.Essentials.Visuals.Universal
{
    public class VisualAssetProvider : MonoBehaviour
    {
        private Material basicNoteMaterial;
        private Material gameNoteMaterial;

        private void Start()
        {
            StartCoroutine(LoadObjects());
        }

        private IEnumerator LoadObjects()
        {
            var load = Addressables.LoadSceneAsync("StandardGameplay", LoadSceneMode.Additive, true, int.MaxValue);
            yield return load;

            Material[] allMats = Resources.FindObjectsOfTypeAll<Material>();

            //foreach (var mat in allMats)
            //{
                //Plugin.Log.Info(mat.name);
            //}

            gameNoteMaterial = allMats.FirstOrDefault(x => x.name == "NoteHD");
            basicNoteMaterial = allMats.FirstOrDefault(x => x.name == "BeatmapEditorObject");

            SceneManager.UnloadSceneAsync("StandardGameplay");
        }
    }
}
