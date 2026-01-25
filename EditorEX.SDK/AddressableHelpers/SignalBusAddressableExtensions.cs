using System;
using System.Collections.Generic;
using EditorEX.SDK.Signals;
using Zenject;

namespace EditorEX.SDK.AddressableHelpers
{
    public static class SignalBusAddressableExtensions
    {
        private static Dictionary<
            Tuple<string, object>,
            Action<IAddressableCollectorItemLoadedSignal>
        > _callbacks = new();

        public static void SubscribeToAddressable(
            this SignalBus signalBus,
            string label,
            object uniqueID,
            Action<IAddressableCollectorItemLoadedSignal> action
        )
        {
            Action<IAddressableCollectorItemLoadedSignal> callback = x =>
            {
                if (x.Label == label)
                {
                    action(x);
                }
            };

            if (uniqueID == null)
            {
                uniqueID = UnityEngine.Random.Range(0, 1000);
            }

            _callbacks.Add(new Tuple<string, object>(label, uniqueID), callback);

            signalBus.Subscribe(callback);
        }

        public static void UnsubscribeFromAddressable(this SignalBus signalBus, string label)
        {
            foreach (var callback in _callbacks)
            {
                if (callback.Key.Item1 == label)
                {
                    signalBus.TryUnsubscribe(callback.Value);
                }
            }
        }

        public static void TryUnsubscribeFromAddressable(this SignalBus signalBus, string label)
        {
            foreach (var callback in _callbacks)
            {
                if (callback.Key.Item1 == label)
                {
                    signalBus.TryUnsubscribe(callback.Value);
                }
            }
        }

        public static void UniqueUnsubscribeFromAddressable(
            this SignalBus signalBus,
            object uniqueID
        )
        {
            foreach (var callback in _callbacks)
            {
                if (callback.Key.Item2 == uniqueID)
                {
                    signalBus.Unsubscribe(callback.Value);
                }
            }
        }

        public static void UniqueTryUnsubscribeFromAddressable(
            this SignalBus signalBus,
            object uniqueID
        )
        {
            foreach (var callback in _callbacks)
            {
                if (callback.Key.Item2 == uniqueID)
                {
                    signalBus.TryUnsubscribe(callback.Value);
                }
            }
        }
    }
}
