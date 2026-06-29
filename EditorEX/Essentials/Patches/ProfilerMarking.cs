using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using SiraUtil.Affinity;
using Unity.Profiling;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Patches;

[AffinityPatch]
public class ProfilerMarking : IAffinity
{
    private Dictionary<Type, ProfilerMarker> _markers = new();
    private Dictionary<SignalSubscriptionId, ProfilerMarker> _signalMarkers = new();

    [AffinityPatch(typeof(TickablesTaskUpdater), nameof(TickablesTaskUpdater.UpdateItem))]
    [AffinityPrefix]
    private bool Prefix(ITickable task)
    {
        if (!_markers.TryGetValue(task.GetType(), out var marker))
        {
            marker = new ProfilerMarker($"Tickable.{task.GetType().Name}.Tick");
            _markers[task.GetType()] = marker;
        }

        using (marker.Auto())
        {
            task.Tick();
        }
        return false;
    }

    [AffinityPatch(
        typeof(SignalBus),
        nameof(SignalBus.SubscribeInternal),
        AffinityMethodType.Normal,
        null,
        typeof(SignalSubscriptionId),
        typeof(Action<object>)
    )]
    [AffinityPrefix]
    private bool Prefix(SignalSubscriptionId id, ref Action<object> callback)
    {
        SignalSubscriptionId _id = id;
        Action<object> _callback = callback;
        Action<object> newCallback = o =>
        {
            if (!_signalMarkers.TryGetValue(_id, out var marker))
            {
                var method = _id._callback;
                string? name = null;
                string? typeName = null;
                if (method is Action action)
                {
                    name = action.Method.Name;
                    typeName = action.Method.DeclaringType?.Name;
                }
                else if (method is Action<object> action2)
                {
                    name = action2.Method.Name;
                    typeName = action2.Method.DeclaringType?.Name;
                }
                marker = new ProfilerMarker(
                    $"SignalSubscriber.{typeName ?? "Unknown"}.{name ?? "Unknown"} (Subscription to {_id._signalId._type.Name})"
                );
                _signalMarkers[_id] = marker;
            }

            using (marker.Auto())
            {
                _callback(o);
            }
        };

        callback = newCallback;
        return true;
    }

    /*[AffinityPatch(typeof(SignalSubscription), nameof(SignalSubscription.Invoke))]
    [AffinityPrefix]
    private bool Prefix(SignalSubscription __instance, object signal)
    {
        if (!_signalMarkers.TryGetValue(__instance._signalId, out var marker))
        {
            var method = __instance._callback.Method;
            if (method.Name.Contains("SubscribeId")) return true;
            marker = new ProfilerMarker(
                $"SignalSubscriber.{method?.DeclaringType?.Name ?? "Unknown"}.{method?.Name ?? "Unknown"} (Subscription to {__instance._signalId._type.Name})"
            );
            _signalMarkers[__instance._signalId] = marker;
        }

        using (marker.Auto())
        {
            __instance._callback(signal);
        }
        return false;
    }*/

    [AffinityPatch(
        typeof(BeatmapEditorCommandRunner),
        nameof(BeatmapEditorCommandRunner.ExecuteCommand)
    )]
    [AffinityPrefix]
    private bool Prefix(BeatmapEditorCommandRunner __instance, IBeatmapEditorCommand command)
    {
        if (!_markers.TryGetValue(command.GetType(), out var marker))
        {
            marker = new ProfilerMarker($"BeatmapEditorCommand.{command.GetType().Name}.Execute");
            _markers[command.GetType()] = marker;
        }

        using (marker.Auto())
        {
            command.Execute();
            IBeatmapEditorCommandWithHistory beatmapEditorCommandWithHistory =
                command as IBeatmapEditorCommandWithHistory;
            if (
                beatmapEditorCommandWithHistory != null
                && beatmapEditorCommandWithHistory.shouldAddToHistory
            )
            {
                IBeatmapEditorCommandWithHistoryMergeable beatmapEditorCommandWithHistoryMergeable =
                    command as IBeatmapEditorCommandWithHistoryMergeable;
                if (beatmapEditorCommandWithHistoryMergeable != null)
                {
                    IBeatmapEditorCommandWithHistoryMergeable beatmapEditorCommandWithHistoryMergeable2 =
                        __instance._beatmapEditorHistory.last
                        as IBeatmapEditorCommandWithHistoryMergeable;
                    if (
                        beatmapEditorCommandWithHistoryMergeable2 != null
                        && beatmapEditorCommandWithHistoryMergeable.ShouldMergeWith(
                            beatmapEditorCommandWithHistoryMergeable2
                        )
                    )
                    {
                        beatmapEditorCommandWithHistoryMergeable.MergeWith(
                            beatmapEditorCommandWithHistoryMergeable2
                        );
                        __instance._beatmapEditorHistory.ReplaceLast(
                            beatmapEditorCommandWithHistoryMergeable
                        );
                        return false;
                    }
                }
                __instance._beatmapEditorHistory.Add(beatmapEditorCommandWithHistory);
            }
            __instance.Log(command, "EXECUTE");
        }
        return false;
    }
}
