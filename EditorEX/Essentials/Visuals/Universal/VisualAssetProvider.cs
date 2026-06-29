using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace EditorEX.Essentials.Visuals.Universal
{
    public class VisualAssetProvider : MonoBehaviour
    {
        public GameObject gameNotePrefab;
        public GameObject obstaclePrefab;

        public GameObject burstSliderHeadPrefab;
        public GameObject burstSliderElementPrefab;

        public event Action onFinishLoading;

        private void Start()
        {
            StartCoroutine(LoadObjects());
        }

        private IEnumerator LoadObjects()
        {
            var load = Addressables.LoadSceneAsync(
                "StandardGameplay",
                LoadSceneMode.Additive,
                true,
                int.MaxValue
            );
            yield return load;

            var objectsInstaller = Resources
                .FindObjectsOfTypeAll<BeatmapObjectsInstaller>()
                .FirstOrDefault();

            gameNotePrefab = Instantiate(objectsInstaller._normalBasicNotePrefab.gameObject);
            gameNotePrefab.SetActive(false);

            obstaclePrefab = Instantiate(objectsInstaller._obstaclePrefab.gameObject);
            obstaclePrefab.SetActive(false);

            burstSliderHeadPrefab = Instantiate(
                objectsInstaller._burstSliderHeadNotePrefab.gameObject
            );
            burstSliderHeadPrefab.SetActive(false);

            burstSliderElementPrefab = Instantiate(
                objectsInstaller._burstSliderNotePrefab.gameObject
            );
            burstSliderElementPrefab.SetActive(false);

            onFinishLoading?.Invoke();

            SceneManager.UnloadSceneAsync("StandardGameplay");
        }
    }
}
