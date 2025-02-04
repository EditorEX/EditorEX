using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Collectors
{
    public class AddressableCollector : IInitializable
    {
        private List<IAddressableCollectorItemLoadedSignal> _signals = new();
        private List<IAddressableCollectorItem> _collectingItems;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(
            List<IAddressableCollectorItem> collectingItems,
            SignalBus signalBus)
        {
            _collectingItems = collectingItems;
            _signalBus = signalBus;
        }

        public bool CheckAvailability(string label)
        {
            bool result = false;

            foreach (var item in _collectingItems)
            {
                if (item.Label == label)
                {
                    result = item.InternalValue != null;
                    break;
                }
            }

            return result;
        }

        public T GetObject<T>(string label) where T : UnityEngine.Object
        {
            T result = null;

            foreach (var item in _collectingItems)
            {
                if (item.Label == label)
                {
                    if (item.InternalValue is T value)
                    {
                        result = value;
                        break;
                    }
                    else
                    {
                        throw new ArgumentException("Value is not of type " + typeof(T).Name);
                    }
                }
            }

            return result;
        }

        public IAddressableCollectorItemLoadedSignal GetSignal(string label)
        {
            return _signals.FirstOrDefault(x => x.Label == label);
        }

        public void Initialize()
        {
            foreach (var item in _collectingItems)
            {
                var opHandle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<UnityEngine.Object>(item.Key);

                opHandle.Completed += (op) =>
                {
                    if (op.OperationException != null)
                    {
                        Debug.LogError($"Failed to load asset {item.Key} ({item.Label}): {op.OperationException.Message}");
                        return;
                    }
                    item.InternalValue = op.Result;

                    var signal = Activator.CreateInstance(typeof(AddressableCollectorItemLoadedSignal<>).MakeGenericType(item.InternalValue.GetType()), item.Label, item.InternalValue);
                    _signals.Add(signal as IAddressableCollectorItemLoadedSignal);
                    _signalBus.Fire(signal);
                };
            }
        }
    }
}
