using EditorEX.SDK.Collectors;
using EditorEX.SDK.Signals;
using System;
using Zenject;

namespace EditorEX.SDK.AddressableHelpers
{
    public class AddressableSignalBus
    {
        public SignalBus SignalBus { get; private set; } = null!;

        private AddressableCollector _addressableCollector = null!;

        [Inject]
        private void Construct(
            SignalBus signalBus,
            AddressableCollector addressableCollector)
        {
            SignalBus = signalBus;
            _addressableCollector = addressableCollector;
        }

        public void Subscribe<T>(string label, object uniqueID, Action<AddressableCollectorItemLoadedSignal<T>> action) where T : UnityEngine.Object
        {
            Subscribe(label, uniqueID, x =>
            {
                if (x is AddressableCollectorItemLoadedSignal<T> signalT)
                {
                    action.Invoke(signalT);
                }
                else
                {
                    throw new ArgumentException("Signal is not of type " + typeof(T).Name);
                }
            });
        }

        public void Subscribe(string label, object uniqueID, Action<IAddressableCollectorItemLoadedSignal> action)
        {
            if (_addressableCollector.CheckAvailability(label))
            {
                action.Invoke(_addressableCollector.GetSignal(label));
            }
            else
            {
                SignalBus.SubscribeToAddressable(label, uniqueID, action);
            }
        }
    }
}
