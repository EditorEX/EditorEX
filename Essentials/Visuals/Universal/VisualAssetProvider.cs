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
        public Material gameNoteMaterial;
        public GameObject gameNotePrefab;

        public event Action onFinishLoading;

        private void Start()
        {
            StartCoroutine(LoadObjects());
        }

        private IEnumerator LoadObjects()
        {
            var load = Addressables.LoadSceneAsync("StandardGameplay", LoadSceneMode.Additive, true, int.MaxValue);
            yield return load;

            UnityEngine.Object[] allObjects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();

            gameNoteMaterial = allObjects.FirstOrDefault(x => x.name == "NoteHD") as Material;
            gameNotePrefab = Instantiate(Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().FirstOrDefault()._normalBasicNotePrefab.gameObject);
            gameNotePrefab.SetActive(false);

            onFinishLoading();

            SceneManager.UnloadSceneAsync("StandardGameplay");
        }
    }
}
