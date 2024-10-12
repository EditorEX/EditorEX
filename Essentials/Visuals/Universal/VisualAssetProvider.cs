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
        public GameObject gameNotePrefab;
        public GameObject obstaclePrefab;

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

            gameNotePrefab = Instantiate(Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().FirstOrDefault()._normalBasicNotePrefab.gameObject);
            gameNotePrefab.SetActive(false);
            obstaclePrefab = Instantiate(Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().FirstOrDefault()._obstaclePrefab.gameObject);
            obstaclePrefab.SetActive(false);

            onFinishLoading?.Invoke();

            SceneManager.UnloadSceneAsync("StandardGameplay");
        }
    }
}
