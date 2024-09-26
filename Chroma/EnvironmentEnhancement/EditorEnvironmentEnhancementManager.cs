using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditorEX.Chroma.EnvironmentEnhancement;
using EditorEX.CustomJSONData;
using EditorEX.Essentials;
using Chroma.EnvironmentEnhancement.Component;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Heck.Animation.Transform;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static Chroma.ChromaController;
using static Heck.HeckController;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement
{
    internal class EditorEnvironmentEnhancementManager : MonoBehaviour
    {
        private SiraLog _log;
        private Dictionary<string, Track> _tracks;
        private bool _leftHanded;
        private EditorGeometryFactory _geometryFactory;
        private TrackLaneRingOffset _trackLaneRingOffset;
        private ParametricBoxControllerTransformOverride _parametricBoxControllerTransformOverride;
        private EditorDuplicateInitializer _duplicateInitializer;
        private EditorComponentCustomizer _componentCustomizer;
        private TransformControllerFactory _controllerFactory;
        private SavedEnvironmentLoader _savedEnvironmentLoader;
        private bool _usingOverrideEnvironment;

        [Inject]
        private void Construct(
            SiraLog log,
            Dictionary<string, Track> tracks,
            [Inject(Id = LEFT_HANDED_ID)] bool leftHanded,
            EditorGeometryFactory geometryFactory,
            TrackLaneRingOffset trackLaneRingOffset,
            ParametricBoxControllerTransformOverride parametricBoxControllerTransformOverride,
            EditorDuplicateInitializer duplicateInitializer,
            EditorComponentCustomizer componentCustomizer,
            TransformControllerFactory controllerFactory,
            SavedEnvironmentLoader savedEnvironmentLoader,
            EnvironmentSceneSetupData sceneSetupData)
        {
            _log = log;
            _tracks = tracks;
            _leftHanded = leftHanded;
            _geometryFactory = geometryFactory;
            _trackLaneRingOffset = trackLaneRingOffset;
            _parametricBoxControllerTransformOverride = parametricBoxControllerTransformOverride;
            _duplicateInitializer = duplicateInitializer;
            _componentCustomizer = componentCustomizer;
            _controllerFactory = controllerFactory;
            _savedEnvironmentLoader = savedEnvironmentLoader;
            _usingOverrideEnvironment = sceneSetupData.hideBranding;
        }

        private void Start()
        {
            StartCoroutine(DelayedStart());
        }

        private static void GetChildRecursive(Transform gameObject, ref List<Transform> children)
        {
            foreach (Transform child in gameObject)
            {
                children.Add(child);
                GetChildRecursive(child, ref children);
            }
        }

        // TODO: add a null check on OverrideEnvironmentSettings
        internal IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            bool v2 = MapContext.Version.Major < 3;
            IEnumerable<CustomData>? environmentData = null;

            // seriously what the fuck beat games
            // GradientBackground permanently yeeted because it looks awful and can ruin multi-colored chroma maps
            GameObject? gradientBackground = GameObject.Find("/Environment/GradientBackground");
            if (gradientBackground != null)
            {
                gradientBackground.SetActive(false);
            }

            environmentData = CustomDataRepository.GetBeatmapData().customData
                    .Get<List<object>>(v2 ? V2_ENVIRONMENT : ENVIRONMENT)?
                    .Cast<CustomData>();

            if (v2)
            {
                try
                {
                    if (LegacyEnvironmentRemoval.Init(CustomDataRepository.GetBeatmapData()))
                    {
                        yield break;
                    }
                }
                catch (Exception e)
                {
                    _log.Error("Could not run Legacy Enviroment Removal");
                    _log.Error(e);
                }
            }

            // _usingOverrideEnvironment kinda a jank way to allow forcing map environment
            if (environmentData == null && _usingOverrideEnvironment)
            {
                // custom environment
                v2 = false;
                environmentData = _savedEnvironmentLoader.SavedEnvironment?.Environment;
            }

            if (environmentData == null)
            {
                yield break;
            }

            List<GameObjectInfo> allGameObjectInfos = GetAllGameObjects();

            string[] gameObjectInfoIds = allGameObjectInfos.Select(n => n.FullID).ToArray();

            foreach (CustomData gameObjectData in environmentData)
            {
                try
                {
                    int? dupeAmount = gameObjectData.Get<int?>(v2 ? V2_DUPLICATION_AMOUNT : DUPLICATION_AMOUNT);
                    bool? active = gameObjectData.Get<bool?>(v2 ? V2_ACTIVE : ACTIVE);
                    TransformData spawnData = new(gameObjectData, v2);
                    int? lightID = gameObjectData.Get<int?>(V2_LIGHT_ID);

                    List<GameObjectInfo> foundObjects;
                    CustomData? geometryData = gameObjectData.Get<CustomData?>(v2 ? V2_GEOMETRY : GEOMETRY);
                    if (geometryData != null)
                    {
                        GameObjectInfo newObjectInfo = new(_geometryFactory.Create(geometryData, v2));
                        allGameObjectInfos.Add(newObjectInfo);
                        foundObjects = new List<GameObjectInfo> { newObjectInfo };

                        // cause i know ppl are gonna fck it up
                        string? id = gameObjectData.Get<string>(GAMEOBJECT_ID);
                        LookupMethod? lookupMethod = gameObjectData.GetStringToEnum<LookupMethod?>(LOOKUP_METHOD);
                        if (id != null || lookupMethod != null)
                        {
                            throw new InvalidOperationException("you cant have geometry and an id you goofball");
                        }
                    }
                    else
                    {
                        string id = gameObjectData.GetRequired<string>(v2 ? V2_GAMEOBJECT_ID : GAMEOBJECT_ID);
                        LookupMethod lookupMethod =
                            gameObjectData.GetStringToEnumRequired<LookupMethod>(v2 ? V2_LOOKUP_METHOD : LOOKUP_METHOD);
                        foundObjects = LookupID.Get(allGameObjectInfos, gameObjectInfoIds, id, lookupMethod);

                        if (foundObjects.Count == 0)
                        {
                            throw new InvalidOperationException(
                                $"ID [\"{id}\"] using method [{lookupMethod:G}] found nothing.");
                        }
                    }

                    CustomData? componentData = null;
                    if (!v2)
                    {
                        componentData = gameObjectData.Get<CustomData>(ComponentConstants.COMPONENTS);
                    }
                    else if (lightID != null)
                    {
                        componentData = new CustomData(new[]
                        {
                            new KeyValuePair<string, object?>(
                                ComponentConstants.LIGHT_WITH_ID,
                                new CustomData(new[]
                                {
                                    new KeyValuePair<string, object?>(LIGHT_ID, lightID.Value)
                                }))
                        });
                    }

                    List<GameObject> gameObjects;

                    // handle duplicating
                    if (dupeAmount.HasValue)
                    {
                        gameObjects = new List<GameObject>();
                        if (foundObjects.Count > 100)
                        {
                            throw new InvalidOperationException(
                                "Extreme value reached, you are attempting to duplicate over 100 objects! Environment enhancements stopped");
                        }

                        foreach (GameObjectInfo gameObjectInfo in foundObjects)
                        {
                            GameObject gameObject = gameObjectInfo.GameObject;
                            Transform parent = gameObject.transform.parent;
                            Scene scene = gameObject.scene;

                            for (int i = 0; i < dupeAmount.Value; i++)
                            {
                                List<IComponentData> componentDatas = new();
                                EditorDuplicateInitializer.PrefillComponentsData(gameObject.transform, componentDatas);
                                GameObject newGameObject = Object.Instantiate(gameObject);
                                _duplicateInitializer.PostfillComponentsData(
                                    newGameObject.transform,
                                    gameObject.transform,
                                    componentDatas);
                                SceneManager.MoveGameObjectToScene(newGameObject, scene);

                                // ReSharper disable once Unity.InstantiateWithoutParent
                                // need to move shit to right scene first
                                newGameObject.transform.SetParent(parent, true);
                                _duplicateInitializer.InitializeComponents(
                                    newGameObject.transform,
                                    gameObject.transform,
                                    allGameObjectInfos,
                                    componentDatas);

                                List<GameObjectInfo> gameObjectInfos =
                                    allGameObjectInfos.Where(n => n.GameObject == newGameObject).ToList();
                                gameObjects.AddRange(gameObjectInfos.Select(n => n.GameObject));
                            }
                        }

                        // Update array with new duplicated objects
                        gameObjectInfoIds = allGameObjectInfos.Select(n => n.FullID).ToArray();
                    }
                    else
                    {
                        if (lightID.HasValue)
                        {
                            _log.Error("LightID requested but no duplicated object to apply to");
                        }

                        gameObjects = foundObjects.Select(n => n.GameObject).ToList();
                    }

                    foreach (GameObject gameObject in gameObjects)
                    {
                        if (active.HasValue)
                        {
                            gameObject.SetActive(active.Value);
                        }

                        Transform transform = gameObject.transform;

                        spawnData.Apply(transform, _leftHanded, v2);

                        // Handle TrackLaneRing
                        TrackLaneRing? trackLaneRing = gameObject.GetComponentInChildren<TrackLaneRing>();
                        if (trackLaneRing != null)
                        {
                            _trackLaneRingOffset.SetTransform(trackLaneRing, spawnData);
                        }

                        // Handle ParametricBoxController
                        ParametricBoxController parametricBoxController =
                            gameObject.GetComponentInChildren<ParametricBoxController>();
                        if (parametricBoxController != null)
                        {
                            _parametricBoxControllerTransformOverride.SetTransform(parametricBoxController, spawnData);
                        }

                        if (componentData != null)
                        {
                            _componentCustomizer.Customize(transform, componentData);
                        }

                        List<Track>? track = gameObjectData.GetNullableTrackArray(_tracks, v2)?.ToList();
                        if (track == null)
                        {
                            continue;
                        }

                        TransformController controller = _controllerFactory.Create(gameObject, track, true);
                        if (trackLaneRing != null)
                        {
                            controller.RotationUpdated += () => _trackLaneRingOffset.UpdateRotation(trackLaneRing);
                            controller.PositionUpdated += () => TrackLaneRingOffset.UpdatePosition(trackLaneRing);
                        }
                        else if (parametricBoxController != null)
                        {
                            controller.PositionUpdated += () =>
                                _parametricBoxControllerTransformOverride.UpdatePosition(parametricBoxController);
                            controller.ScaleUpdated += () =>
                                _parametricBoxControllerTransformOverride.UpdateScale(parametricBoxController);
                        }

                        track.ForEach(n => n.AddGameObject(gameObject));
                    }
                }
                catch (Exception e)
                {
                    _log.Error($"Error processing environment data for: {gameObjectData}");
                    _log.Error(e);
                }
            }
        }

        private List<GameObjectInfo> GetAllGameObjects()
        {
            List<GameObjectInfo> result = new();

            // I'll probably revist this formula for getting objects by only grabbing the root objects and adding all the children
            List<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(n =>
            {
                if (n == null)
                {
                    return false;
                }

                string sceneName = n.scene.name;
                if (sceneName == null)
                {
                    return false;
                }

                return (sceneName.Contains("Environment") && !sceneName.Contains("Menu")) || n.GetComponent<TrackLaneRing>() != null;
            }).ToList();

            // Adds the children of whitelist GameObjects
            // Mainly for grabbing cone objects in KaleidoscopeEnvironment
            gameObjects.ToList().ForEach(n =>
            {
                List<Transform> allChildren = new();
                GetChildRecursive(n.transform, ref allChildren);

                foreach (Transform transform in allChildren)
                {
                    if (!gameObjects.Contains(transform.gameObject))
                    {
                        gameObjects.Add(transform.gameObject);
                    }
                }
            });

            List<string> objectsToPrint = new();

            foreach (GameObject gameObject in gameObjects)
            {
                GameObjectInfo gameObjectInfo = new(gameObject);
                result.Add(new GameObjectInfo(gameObject));
                objectsToPrint.Add(gameObjectInfo.FullID);
            }

            return result;
        }
    }
}
