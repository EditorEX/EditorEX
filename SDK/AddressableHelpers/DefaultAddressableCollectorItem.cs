using System;
using UnityEngine;

namespace EditorEX.SDK.AddressableHelpers
{
    public class DefaultAddressableCollectorItem<T> : IAddressableCollectorItem where T : UnityEngine.Object
    {
        public string Label { get; }

        public string Key { get; }

        public UnityEngine.Object InternalValue
        {
            get
            {
                return _value;
            }
            set
            {
                if (value is T valueT)
                {
                    _value = valueT;
                }
                else
                {
                    throw new ArgumentException("Value is not of type " + typeof(T).Name);
                }
            }
        }

        public DefaultAddressableCollectorItem(string label, string key)
        {
            Label = label;
            Key = key;
        }

        private T _value;
    }
}
