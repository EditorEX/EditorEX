using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;
using Zenject.Internal;
using Object = UnityEngine.Object;

namespace EditorEX.Chroma.Patches.EnvironmentComponent;

// Based from https://github.com/Aeroluna/Heck
internal class EditorRingAwakeInstantiator : IAffinity
{
    private static readonly FieldInfo _trackLaneRingPrefab = AccessTools.Field(
        typeof(TrackLaneRingsManager),
        nameof(TrackLaneRingsManager._trackLaneRingPrefab)
    );

    private static readonly MethodInfo _queueInject = AccessTools.Method(
        typeof(EditorRingAwakeInstantiator),
        nameof(QueueInject)
    );

    private static readonly MethodInfo _queueInjectParent = AccessTools.Method(
        typeof(EditorRingAwakeInstantiator),
        nameof(QueueInjectParent)
    );

    private static readonly FieldAccessor<
        TrackLaneRingsManager,
        DiContainer
    >.Accessor _containerAccessor = FieldAccessor<TrackLaneRingsManager, DiContainer>.GetAccessor(
        nameof(TrackLaneRingsManager._container)
    );

    private static void FindTrackLaneRingManager(
        Transform transform,
        List<TrackLaneRingsManager> managers
    )
    {
        managers.Add(transform.GetComponent<TrackLaneRingsManager>());

        foreach (Transform child in transform)
        {
            FindTrackLaneRingManager(child, managers);
        }
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(SceneDecoratorContext), nameof(SceneDecoratorContext.Initialize))]
    private void InitManagers(SceneDecoratorContext __instance, DiContainer container)
    {
        if (__instance.DecoratedContractName != "Environment")
        {
            return;
        }

        List<TrackLaneRingsManager> managers = new(1);
        __instance
            .gameObject.scene.GetRootGameObjects()
            .Do(n => FindTrackLaneRingManager(n.transform, managers));
        foreach (TrackLaneRingsManager manager in managers)
        {
            if (manager == null)
            {
                continue;
            }

            TrackLaneRingsManager managerref = manager;
            _containerAccessor(ref managerref) = container;
            manager.Start();
        }
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(TrackLaneRingsManager), nameof(TrackLaneRingsManager.Start))]
    private bool InitOnce(TrackLaneRing[]? ____rings)
    {
        return ____rings == null;
    }

    private static TrackLaneRing QueueInject(DiContainer container, TrackLaneRing prefab) =>
        QueueInjectParent(container, prefab, null);

    private static TrackLaneRing QueueInjectParent(
        DiContainer container,
        TrackLaneRing prefab,
        Transform? parent
    )
    {
        TrackLaneRing trackLaneRing = Object.Instantiate(prefab, parent);
        List<MonoBehaviour> injectables = [];
        ZenUtilInternal.GetInjectableMonoBehavioursUnderGameObject(
            trackLaneRing.gameObject,
            injectables
        );
        injectables.ForEach(container.QueueForInject);
        return trackLaneRing;
    }

    [AffinityPatch(typeof(TrackLaneRingsManager), nameof(TrackLaneRingsManager.Start))]
    [AffinityTranspiler]
    private IEnumerable<CodeInstruction> QueueInjectTranspiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        return new CodeMatcher(instructions)
            /*
             * -- this._rings[i] = this._container.InstantiatePrefabForComponent<TrackLaneRing>(this._trackLaneRingPrefab, base.transform);
             * ++ this._rings[i] = QueueInjectParent(this._container, this._trackLaneRingPrefab);
            */
            .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _trackLaneRingPrefab))
            .Advance(3)
            .Set(OpCodes.Call, _queueInjectParent)
            /*
             * -- this._rings[j] = this._container.InstantiatePrefabForComponent<TrackLaneRing>(this._trackLaneRingPrefab);
             * ++ this._rings[j] = QueueInject(this._container, this._trackLaneRingPrefab);
            */
            .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _trackLaneRingPrefab))
            .Advance(1)
            .Set(OpCodes.Call, _queueInject)
            .InstructionEnumeration();
    }
}
